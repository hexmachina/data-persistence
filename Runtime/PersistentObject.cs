using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TW.DataPersistence
{

	[System.Serializable]
	public class ObjectPersistenceData : ObjectDataBase
	{
		public string guid;
		public Dictionary<int, string> componentsMap = new();
	}

	[ExecuteInEditMode, DisallowMultipleComponent, RequireComponent(typeof(ComponentsTracker))]
	public class PersistentObject : MonoBehaviour, ISerializationCallbackReceiver
	{

		private ComponentsTracker _componentTracker;

		// System guid we use for comparison and generation
		System.Guid guid = System.Guid.Empty;

		// Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
		// Fun fact, we tried using strings at first, but that allocated memory and was twice as slow
		[SerializeField]
		private byte[] serializedGuid;

		public ComponentsTracker ComponentsTracker
		{
			get
			{
				if (_componentTracker == null)
				{
					TryGetComponent(out _componentTracker);
				}
				return _componentTracker;
			}
		}

		public IReadOnlyDictionary<int, Object> TrackedComponents => ComponentsTracker.Tracked;

		public System.Guid GUID => guid;

		private void Awake()
		{
			CreateGuid();
			ObjectPersistenceManager.Instance.Register(this);
		}

		private void OnDestroy()
		{
			if (!ObjectPersistenceManager.IsInstantiated)
			{
				return;
			}
			ObjectPersistenceManager.Instance.Unregister(this);

		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			// similar to on Serialize, but gets called on Copying a Component or Applying a Prefab
			// at a time that lets us detect what we are
			if (IsAssetOnDisk())
			{
				serializedGuid = null;
				guid = System.Guid.Empty;
			}
			else
			{
				CreateGuid();
			}
		}
#endif

		public bool IsGuidAssigned()
		{
			return guid != System.Guid.Empty;
		}

		// When de-serializing or creating this component, we want to either restore our serialized GUID
		// or create a new one.
		void CreateGuid()
		{
			// if our serialized data is invalid, then we are a new object and need a new GUID
			if (serializedGuid == null || serializedGuid.Length != 16)
			{
				// if in editor, make sure we aren't a prefab of some kind
#if UNITY_EDITOR
				if (IsAssetOnDisk())
				{
					return;
				}
				Undo.RecordObject(this, "Added GUID");
#endif
				guid = System.Guid.NewGuid();
				serializedGuid = guid.ToByteArray();

#if UNITY_EDITOR
				// If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
				// force a save of the modified prefab instance properties
				if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
				{
					PrefabUtility.RecordPrefabInstancePropertyModifications(this);
				}
#endif
			}
			else if (guid == System.Guid.Empty)
			{
				// otherwise, we should set our system guid to our serialized guid
				guid = new System.Guid(serializedGuid);
			}

			// register with the GUID Manager so that other components can access this
			//if (guid != System.Guid.Empty)
			//{
			//	if (!GuidManager.Add(this))
			//	{
			//		// if registration fails, we probably have a duplicate or invalid GUID, get us a new one.
			//		serializedGuid = null;
			//		guid = System.Guid.Empty;
			//		CreateGuid();
			//	}
			//}
		}

#if UNITY_EDITOR
		private bool IsEditingInPrefabMode()
		{
			if (EditorUtility.IsPersistent(this))
			{
				// if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
				return true;
			}
			else
			{
				// If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
				var mainStage = StageUtility.GetMainStageHandle();
				var currentStage = StageUtility.GetStageHandle(gameObject);
				if (currentStage != mainStage)
				{
					var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
					if (prefabStage != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsAssetOnDisk()
		{

			return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();

		}
#endif


		public void OnAfterDeserialize()
		{
			if (serializedGuid != null && serializedGuid.Length == 16)
			{
				guid = new System.Guid(serializedGuid);
			}
		}

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			// This lets us detect if we are a prefab instance or a prefab asset.
			// A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
			if (IsAssetOnDisk())
			{
				serializedGuid = null;
				guid = System.Guid.Empty;
			}
			else
#endif
			{
				if (guid != System.Guid.Empty)
				{
					serializedGuid = guid.ToByteArray();
				}
			}
		}

		public System.Guid GetGuid()
		{
			if (guid == System.Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
			{
				guid = new System.Guid(serializedGuid);
			}

			return guid;
		}

	}
}

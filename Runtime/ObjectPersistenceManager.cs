using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW.DataPersistence
{
	[Serializable]
	public class PersistenceContainer : ObjectDataBase
	{
		public List<string> data = new();
	}
	public class ObjectPersistenceManager
	{
		private static ObjectPersistenceManager _instance;
		public static ObjectPersistenceManager Instance
		{
			get
			{
				_instance ??= new ObjectPersistenceManager();
				return _instance;
			}
		}

		private Dictionary<System.Guid, PersistentObject> _persistMap = new();

		public Dictionary<Type, ObjectTrackerBase> trackers = new();

		public ObjectPersistenceManager()
		{
			RegisterTrackers();
		}

		public void Register(PersistentObject persistence)
		{
			if (_persistMap.ContainsKey(persistence.GUID))
			{
				Debug.LogWarning($"ObjectPersistence with GUID {persistence.GUID} is already registered.");
				return;
			}
			_persistMap[persistence.GUID] = persistence;
		}

		private void RegisterTrackers()
		{

			var trackerTypes = ReflectionHelpers.FindDerivedTypes(typeof(ObjectTrackerBase));
			foreach (var type in trackerTypes)
			{
				var genericArg = ReflectionHelpers.GetFirstGenericArgument(type, typeof(ObjectTrackerBase));
				if (genericArg == null || trackers.ContainsKey(genericArg))
				{
					continue;
				}

				ObjectTrackerBase trackerInstance = Activator.CreateInstance(type) as ObjectTrackerBase;
				if (trackerInstance == null)
				{
					continue;
				}
				trackers[genericArg] = trackerInstance;
			}
		}

		public string SerializeAll()
		{
			string json = string.Empty;
			using (var container = new PersistenceContainer())
			{
				foreach (var target in _persistMap.Values)
				{
					container.data.Add(Serialize(target));
				}
				json = JsonUtility.ToJson(container, false);
			}

			return json;
		}

		private string Serialize(PersistentObject persistence)
		{
			string json = string.Empty;
			using (var data = new ObjectPersistenceData
			{
				guid = persistence.GUID.ToString(),
				address = persistence.AddressableName,
				components = new()
			})
			{
				foreach (var component in persistence.TrackedComponents)
				{
					var type = component.GetType();
					if (trackers.ContainsKey(type))
					{
						data.components.Add(trackers[type].OnSerialize(component));
					}
				}
				json = JsonUtility.ToJson(data, false);
			}

			return json;
		}

		public void DeserializeAll(string json)
		{
			var dataList = JsonUtility.FromJson<PersistenceContainer>(json);
			foreach (var data in dataList.data)
			{
				Deserialize(data);
			}
		}

		public void Deserialize(string json)
		{
			var data = JsonUtility.FromJson<ObjectPersistenceData>(json);
			var guid = new System.Guid(data.guid);
			if (!_persistMap.ContainsKey(guid))
			{
				Debug.LogError($"No ObjectPersistence found with GUID {data.guid}");
				return;
			}
			var target = _persistMap[guid];

			for (int i = 0; i < target.TrackedComponents.Count; i++)
			{
				var component = target.TrackedComponents[i];
				var type = component.GetType();
				if (trackers.ContainsKey(type) && i < data.components.Count)
				{
					trackers[type].OnDeserialize(data.components[i], target.gameObject);
				}
			}
		}
	}

}

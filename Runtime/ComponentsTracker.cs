using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TW.DataPersistence
{
	[Serializable]
	public class TrackedObject
	{
		public int id;
		public Object target;
	}


	[DisallowMultipleComponent]
	public class ComponentsTracker : MonoBehaviour, ISerializationCallbackReceiver
	{
		[SerializeField, NonReorderable] private List<Object> _trackedComponents = new();

		[SerializeField] private List<TrackedObject> trackedObjects = new();
		[SerializeField] private Dictionary<int, Object> _trackedDict = new();
		public Dictionary<int, Object> Tracked => _trackedDict;

		private void OnValidate()
		{
			for (int i = trackedObjects.Count - 1; i >= 0; i--)
			{
				if (!_trackedComponents.Contains(trackedObjects[i].target))
				{
					trackedObjects.RemoveAt(i);
				}
			}
			foreach (var item in _trackedComponents)
			{
				bool found = false;
				foreach (var tracked in trackedObjects)
				{
					if (item == tracked.target)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					trackedObjects.Add(new() { id = UnityEngine.Random.Range(int.MinValue, int.MaxValue), target = item });
				}
			}
		}

		public void OnAfterDeserialize()
		{
			_trackedDict.Clear();
			foreach (var item in trackedObjects)
			{
				_trackedDict[item.id] = item.target;
			}
		}

		public void OnBeforeSerialize()
		{

		}
	}
}

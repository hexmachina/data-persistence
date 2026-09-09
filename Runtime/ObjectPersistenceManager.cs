using Newtonsoft.Json;
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

		public static bool IsInstantiated => _instance != null;

		private readonly Dictionary<System.Guid, PersistentObject> _persistMap = new();

		private readonly Dictionary<Type, ObjectTrackerBase> _trackers = new();

		public ObjectPersistenceManager()
		{
			RegisterTrackers();
		}

		public void Register(PersistentObject persistence)
		{
			if (_persistMap.ContainsKey(persistence.GUID))
			{
				var source = _persistMap[persistence.GUID];
				if (source != null)
				{
					if (source != persistence)
					{
						Debug.LogWarning($"ObjectPersistence ${persistence.name} and ${source.name} have the same GUID {persistence.GUID}", persistence);
					}
					return;
				}
			}
			_persistMap[persistence.GUID] = persistence;
		}

		public void Unregister(PersistentObject persistence)
		{
			if (_persistMap.ContainsKey(persistence.GUID))
			{
				_persistMap.Remove(persistence.GUID);
			}
		}

		private void RegisterTrackers()
		{

			var trackerTypes = ReflectionHelpers.FindDerivedTypes(typeof(ObjectTrackerBase));
			foreach (var type in trackerTypes)
			{
				var genericArg = ReflectionHelpers.GetFirstGenericArgument(type, typeof(ObjectTrackerBase));
				if (genericArg == null || _trackers.ContainsKey(genericArg))
				{
					continue;
				}

				ObjectTrackerBase trackerInstance = Activator.CreateInstance(type) as ObjectTrackerBase;
				if (trackerInstance == null)
				{
					continue;
				}
				_trackers[genericArg] = trackerInstance;
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
				json = JsonConvert.SerializeObject(container);
			}

			return json;
		}

		private string Serialize(PersistentObject persistence)
		{
			string json = string.Empty;
			using (var data = new ObjectPersistenceData
			{
				guid = persistence.GUID.ToString(),
				componentsMap = new()

			})
			{
				foreach (var (id, component) in persistence.TrackedComponents)
				{
					var type = component.GetType();
					if (!_trackers.ContainsKey(type))
					{
						continue;
					}
					data.componentsMap[id] = _trackers[type].OnSerialize(component);
				}
				json = JsonConvert.SerializeObject(data);
			}

			return json;
		}

		public void DeserializeAll(string json)
		{
			var dataList = JsonConvert.DeserializeObject<PersistenceContainer>(json);
			foreach (var data in dataList.data)
			{
				Deserialize(data);
			}
		}

		public void Deserialize(string json)
		{
			var data = JsonConvert.DeserializeObject<ObjectPersistenceData>(json);
			var guid = new System.Guid(data.guid);
			if (!_persistMap.ContainsKey(guid))
			{
				Debug.LogError($"No ObjectPersistence found with GUID {data.guid}");
				return;
			}
			var target = _persistMap[guid];
			foreach (var (key, value) in data.componentsMap)
			{
				if (!target.TrackedComponents.TryGetValue(key, out var component))
				{
					continue;
				}
				var type = component.GetType();
				if (!_trackers.ContainsKey(type))
				{
					Debug.LogWarning($"{type.Name} is not a tracked type.");
					continue;
				}
				_trackers[type].OnDeserialize(value, target.gameObject);

			}
		}
	}

}

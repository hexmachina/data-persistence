using System.Collections.Generic;
using UnityEngine;

namespace TW.DataPersistence
{
	public class PersistentStateController : MonoBehaviour, ISerializationCallbackReceiver
	{
		[System.Serializable]
		public class PersistKeyValue
		{
			public string key;
			public PersistBase value;
		}

		[SerializeField] protected List<PersistKeyValue> _persists;

		protected readonly Dictionary<string, PersistBase> _persistDict = new();

		public void OnBeforeSerialize()
		{

		}

		public void OnAfterDeserialize()
		{
			_persistDict.Clear();
			foreach (var item in _persists)
			{
				_persistDict.TryAdd(item.key, item.value);
			}
		}



		protected void LoadState(StateCollection stateCollection)
		{
			foreach (var (key, value) in _persistDict)
			{
				if (stateCollection == null)
				{
					value.ReceiveData(null);
					continue;
				}

				if (stateCollection.State.TryGetValue(key, out string json))
				{
					value.ReceiveData(json);
				}
				else
				{
					value.ReceiveData(null);

				}
			}
		}
	}
}

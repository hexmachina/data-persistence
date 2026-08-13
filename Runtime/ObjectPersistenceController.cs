using System.IO;
using UnityEngine;

namespace TW.DataPersistence
{
	[DisallowMultipleComponent]
	public class ObjectPersistenceController : MonoBehaviour
	{
		public void Save()
		{
			string path = Path.Combine(Application.dataPath, "test.json");
			string json = ObjectPersistenceManager.Instance.SerializeAll();
			File.WriteAllText(path, json);

		}

		public void Load()
		{
			string path = Path.Combine(Application.dataPath, "test.json");
			if (!File.Exists(path))
			{
				Debug.LogError($"File not found at path: {path}");
				return;
			}
			string jsonData = File.ReadAllText(path);
			ObjectPersistenceManager.Instance.DeserializeAll(jsonData);
		}
	}
}


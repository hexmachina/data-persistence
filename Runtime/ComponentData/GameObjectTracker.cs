using UnityEngine;

namespace TW.DataPersistence
{
	[System.Serializable]
	public class GameObjectData : ObjectDataBase
	{
		public string name;
		public int layer;
		public bool isActive;
		public string tag;

	}
	public class GameObjectTracker : ObjectTrackerBase<GameObject, GameObjectData>
	{
		protected override GameObjectData Store(GameObject component)
		{
			var data = new GameObjectData
			{
				name = component.name,
				layer = component.layer,
				isActive = component.activeSelf,
				tag = component.tag
			};
			return data;
		}
		protected override void Apply(GameObjectData data, GameObject component)
		{
			component.name = data.name;
			component.layer = data.layer;
			component.SetActive(data.isActive);
			component.tag = data.tag;
		}

	}
}

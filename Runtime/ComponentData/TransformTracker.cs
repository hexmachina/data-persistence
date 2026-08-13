using UnityEngine;

namespace TW.DataPersistence
{
	[System.Serializable]
	public class TranformData : ComponentDataBase
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;

	}

	public class TransformTracker : ComponentTrackerBase<Transform, TranformData>
	{
		protected override TranformData Store(Transform component)
		{
			var data = new TranformData
			{
				componentIndex = component.GetComponentIndex(),
				position = component.position,
				rotation = component.rotation,
				scale = component.localScale
			};
			return data;
		}
		protected override void Apply(TranformData data, Transform component)
		{
			if (component.TryGetComponent(out Rigidbody rb))
			{
				rb.position = data.position;
				rb.rotation = data.rotation;
			}
			else
			{
				component.SetPositionAndRotation(data.position, data.rotation);
			}
			component.localScale = data.scale;
		}
	}
}

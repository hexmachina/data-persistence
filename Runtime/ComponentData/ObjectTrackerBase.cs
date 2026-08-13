using UnityEngine;

namespace TW.DataPersistence
{
	public abstract class ObjectTrackerBase
	{
		public abstract void OnDeserialize(string json, GameObject go);

		public abstract string OnSerialize(Object obj);

	}

	public abstract class ObjectTrackerBase<T, U> : ObjectTrackerBase where T : Object where U : ObjectDataBase
	{
		protected abstract U Store(T component);
		protected abstract void Apply(U data, T component);

		public override void OnDeserialize(string json, GameObject go)
		{
			using (var data = JsonUtility.FromJson<U>(json))
			{
				var obj = go as T;
				if (obj == null)
				{
					Debug.LogError($"GameObject {go.name} is not of type {typeof(T).Name}");
					return;
				}
				Apply(data, obj);
			}
		}

		public override string OnSerialize(Object obj)
		{
			string json = null;
			using (var data = Store(obj as T))
			{
				if (data == null)
				{
					Debug.LogError($"Failed to store data for object {obj.name} of type {typeof(T).Name}");
					return null;
				}
				json = JsonUtility.ToJson(data, true);
			}
			return json;
		}
	}

	public abstract class ComponentTrackerBase<T, U> : ObjectTrackerBase where T : Component where U : ComponentDataBase
	{
		protected abstract U Store(T component);
		protected abstract void Apply(U data, T component);

		public override void OnDeserialize(string json, GameObject go)
		{
			using (var data = JsonUtility.FromJson<U>(json))
			{
				var component = go.GetComponentAtIndex<T>(data.componentIndex);
				if (component == null)
				{
					Debug.LogError($"Component of type {typeof(T).Name} with index {data.componentIndex} not found on GameObject {go.name}");
					return;
				}
				Apply(data, component);
			}
		}

		public override string OnSerialize(Object obj)
		{
			var component = obj as T;
			if (component == null)
			{
				Debug.LogError($"Object {obj.name} is not of type {typeof(T).Name}");
				return null;
			}
			return OnSerialize(component);
		}

		public virtual string OnSerialize(T component)
		{
			string json = null;
			using (var data = Store(component))
			{
				if (data == null)
				{
					Debug.LogError($"Failed to store data for component {component.name} of type {typeof(T).Name}");
					return null;
				}
				json = JsonUtility.ToJson(data, true);
			}
			return json;
		}
	}



}

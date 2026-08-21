using System;

namespace TW.DataPersistence
{
	[Serializable]
	public abstract class ObjectDataBase : IDisposable
	{
		public virtual void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}

	[Serializable]
	public abstract class ComponentDataBase : ObjectDataBase
	{
		public int componentIndex;

	}


	[Serializable]
	public abstract class MonoBehaviourDataBase : ComponentDataBase
	{
		public bool enabled;
	}



}

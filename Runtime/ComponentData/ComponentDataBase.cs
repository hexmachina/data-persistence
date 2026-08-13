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

	public abstract class ComponentDataBase : ObjectDataBase
	{
		public int componentIndex;

	}


	public abstract class MonoBehaviourDataBase : ComponentDataBase
	{
		public bool enabled;
	}



}

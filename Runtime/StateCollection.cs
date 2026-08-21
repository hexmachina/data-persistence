using System;
using System.Collections.Generic;

namespace TW.DataPersistence
{
	public class StateCollection : IDisposable
	{
		public Dictionary<string, string> State = new();

		public void Dispose()
		{

			GC.SuppressFinalize(this);
		}
	}
}

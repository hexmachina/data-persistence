using UnityEngine;

namespace TW.DataPersistence
{
	public abstract class PersistBase : MonoBehaviour
	{
		public abstract void ReceiveData(string data);
		public abstract string SendData();
	}
}

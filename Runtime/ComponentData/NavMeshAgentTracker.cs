using System;
using UnityEngine;
using UnityEngine.AI;

namespace TW.DataPersistence
{
	[Serializable]
	public class NavMeshAgentData : ComponentDataBase
	{
		public Vector3 destination;
		public Vector3 velocity;
		public Vector3 nextPosition;
		public float speed;
		public float angularSpeed;
		public float acceleration;
		public float stoppingDistance;
		public bool autoBraking;
		public bool isStopped;
		public int areaMask;
		public bool updatePosition;
		public bool updateRotation;
		public bool updateUpAxis;
		public int avoidancePriority;
		public ObstacleAvoidanceType obstacleAvoidanceType;
	}

	public class NavMeshAgentTracker : ObjectTrackerBase<NavMeshAgent, NavMeshAgentData>
	{
		protected override void Apply(NavMeshAgentData data, NavMeshAgent component)
		{

		}

		protected override NavMeshAgentData Store(NavMeshAgent component)
		{
			var data = new NavMeshAgentData();
			return data;
		}
	}
}

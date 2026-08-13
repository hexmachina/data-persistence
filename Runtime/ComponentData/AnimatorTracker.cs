using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW.DataPersistence
{
	[Serializable]
	public class AnimatorData : MonoBehaviourDataBase
	{
		public List<AnimatorParameterData> parameters = new();
		public List<AnimatorStateData> states = new();
	}
	[Serializable]
	public class AnimatorParameterData : ObjectDataBase
	{
		public int nameHash;
		public AnimatorControllerParameterType type;
		public float floatValue;
		public int intValue;
		public bool boolValue;
	}
	[Serializable]
	public class AnimatorStateData : ObjectDataBase
	{
		public float weight;
		public int nameHash;
		public float normalizedTime;
	}
	public class AnimatorTracker : ComponentTrackerBase<Animator, AnimatorData>
	{
		protected override void Apply(AnimatorData data, Animator component)
		{
			component.enabled = data.enabled;
			if (component.runtimeAnimatorController == null)
			{
				return;
			}
			foreach (var parameter in data.parameters)
			{
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Float:
						component.SetFloat(parameter.nameHash, parameter.floatValue);
						break;
					case AnimatorControllerParameterType.Int:
						component.SetInteger(parameter.nameHash, parameter.intValue);
						break;
					case AnimatorControllerParameterType.Bool:
						component.SetBool(parameter.nameHash, parameter.boolValue);
						break;
				}
			}
			for (int i = 0; i < data.states.Count; i++)
			{
				var stateData = data.states[i];
				component.SetLayerWeight(i, stateData.weight);
				component.Play(stateData.nameHash, i, stateData.normalizedTime);
			}

		}

		protected override AnimatorData Store(Animator component)
		{
			if (component.runtimeAnimatorController == null)
			{
				Debug.LogWarning($"Animator {component.name} does not have a runtime animator controller assigned. Skipping serialization.");
				return null;
			}
			var data = new AnimatorData
			{
				componentIndex = component.GetComponentIndex(),
				enabled = component.enabled
			};
			foreach (var parameter in component.parameters)
			{
				var parameterData = new AnimatorParameterData()
				{
					nameHash = parameter.nameHash,
					type = parameter.type,

				};
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Float:
						parameterData.floatValue = component.GetFloat(parameter.nameHash);
						break;
					case AnimatorControllerParameterType.Int:
						parameterData.intValue = component.GetInteger(parameter.nameHash);
						break;
					case AnimatorControllerParameterType.Bool:
						parameterData.boolValue = component.GetBool(parameter.nameHash);
						break;
				}
				data.parameters.Add(parameterData);
			}

			for (int i = 0; i < component.layerCount; i++)
			{
				var stateInfo = component.GetCurrentAnimatorStateInfo(i);
				var stateData = new AnimatorStateData()
				{
					weight = component.GetLayerWeight(i),
					nameHash = stateInfo.shortNameHash,
					normalizedTime = stateInfo.normalizedTime
				};
				data.states.Add(stateData);
			}
			return data;
		}
	}
}

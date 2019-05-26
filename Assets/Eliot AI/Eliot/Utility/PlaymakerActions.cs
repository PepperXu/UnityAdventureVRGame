#if PLAYMAKER

using System.Collections;
using Eliot.AgentComponents;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Eliot.Utility.playmaker.actions
{
	/// <summary>
	/// Make Agents in certain radius aware of some suspicious noise for a certain period of time.
	/// </summary>
	[ActionCategory("Eliot")]
	[HutongGames.PlayMaker.Tooltip("Make Agents in certain radius aware of some " +
	                               "suspicious noise for a certain period of time")]
	public class MakeNoise : FsmStateAction
	{
		[HutongGames.PlayMaker.Tooltip("Range in which the Agents will be affected")]
		public float Range;
		[HutongGames.PlayMaker.Tooltip("Duration of time for which the Agents will be affected")]
		public float Duration;
		[HutongGames.PlayMaker.Tooltip("Origin position of the noise")]
		public FsmGameObject Transform;

		public override void Reset()
		{
			Range = 15;
			Duration = 10;
			Transform = new FsmGameObject(Owner);
		}

		public override void OnEnter()
		{
			Eliot.AgentComponents.Skill.MakeNoise(Range, GetTransform(), Duration);
		}

		private Transform GetTransform()
		{
			return Transform.Value.transform ? Transform.Value.transform : Owner.transform;
		}
	}

	/// <summary>
	/// Take advantage of Eliot Skills in PlayMaker.
	/// </summary>
	[ActionCategory("Eliot")]
	[HutongGames.PlayMaker.Tooltip("Take advantage of Eliot Skills in PlayMaker")]
	public class InvokeSkill : FsmStateAction
	{
		[HutongGames.PlayMaker.Tooltip("The Skill to be invoked")]
		public Eliot.AgentComponents.Skill Skill;
		[HutongGames.PlayMaker.Tooltip("If true, the Skill will be invoked regardless " +
		                               "of whether the owner Agent has it or not")]
		public bool ForceInvoke;
		[HutongGames.PlayMaker.Tooltip("If true, the owner Agent will be used as an origin" +
		                               " of the Skill. If false, Skill will be invoked from a temporary object")]
		public bool InvokeFromOwner;
		[HutongGames.PlayMaker.Tooltip("The origin game object of the Skill")]
		public FsmGameObject SkillOrigin;
		
		public override void Reset()
		{
			InvokeFromOwner = true;
			SkillOrigin = new FsmGameObject(Owner);
		}
		
		public override void OnEnter()
		{
			switch (InvokeFromOwner)
			{
				case true:
					InvokeSkillGetAgent();
					break;
				case false:
					InvokeSkillNewGameObject();
					break;
			}
		}

		/// <summary>
		/// Create a temporary game object and invoke the Skill from it.
		/// </summary>
		private void InvokeSkillNewGameObject()
		{
			var go = new GameObject("__skill__");
			go.transform.position = SkillOrigin.Value.transform.position;
			go.transform.rotation = SkillOrigin.Value.transform.rotation;
			var agent = Agent.AddAgentComponent(go, true);
			agent.AddSkill(Skill);
			agent.Skill(Skill.name, true)();
			agent.StartCoroutine(DestroyIn(Skill.LifeTime(), go));
		}

		/// <summary>
		/// Wait for a given amount of time and destroy game object.
		/// </summary>
		/// <param name="seconds"></param>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		private IEnumerator DestroyIn(float seconds, GameObject gameObject)
		{
			yield return new WaitForSeconds(seconds);
			GameObject.Destroy(gameObject);
		}

		/// <summary>
		/// Invoke Skill from the object.
		/// </summary>
		private void InvokeSkillGetAgent()
		{
			Agent agent;
			if (Owner.gameObject.GetComponent<Agent>())
				agent = Owner.gameObject.GetComponent<Agent>();
			else 
				agent = Agent.AddAgentComponent(Owner.gameObject, true);

			if (agent.Skills.Contains(Skill))
			{
				agent.Skill(Skill.name, true)();
			}
			else if (ForceInvoke)
			{
				agent.AddSkill(Skill);
				agent.Skill(Skill.name, true)();
			}
		}
	}

	/// <summary>
	/// Interact with Agent using Skill.
	/// </summary>
	[ActionCategory("Eliot")]
	[HutongGames.PlayMaker.Tooltip("Interact with Agent using Skill")]
	public class RayCastToAgent : FsmStateAction
	{
		[HutongGames.PlayMaker.Tooltip("Origin transform of the ray")]
		public GameObject RayCastOrigin;
		[HutongGames.PlayMaker.Tooltip("Maximum distance of the ray cast")]
		public float MaxDistance = 5;
		[HutongGames.PlayMaker.Tooltip("Filter target by layer")]
		public FsmInt[] LayerMask;
		[HutongGames.PlayMaker.Tooltip("Whether to invert the filter")]
		public FsmBool InvertMask;
		[HutongGames.PlayMaker.Tooltip("The Skill that will affect the target Agent")]
		public Skill Skill;
		[HutongGames.PlayMaker.Tooltip("Whether to log the debug info")]
		public bool debug;
		
		public override void Reset()
		{
			LayerMask = new FsmInt[0];
			InvertMask = false;
			debug = false;
		}
		
		public override void OnEnter()
		{
			RaycastHit hit;
			if (Physics.Raycast(RayCastOrigin.transform.position,
				RayCastOrigin.transform.TransformDirection(Vector3.forward), out hit, MaxDistance, ActionHelpers.LayerArrayToLayerMask(LayerMask, InvertMask.Value)))
			{
				var obj = hit.collider.gameObject;
				if (obj.GetComponent<Agent>())
				{
					var agent = obj.GetComponent<Agent>();
					agent.AddEffect(Skill, null);
				}
				else
				{
					if(debug)
						Debug.Log("Object doesn't have the Agent component attached to it");
				}
			}
			else
			{
				if(debug)
					Debug.Log("Raycast Hit did not hit any collider.");
			}
		}
	}
}

#endif
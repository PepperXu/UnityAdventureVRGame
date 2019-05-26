using System;
using System.Collections.Generic;
using Eliot.Utility;
using UnityEngine;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// <para>Control death of the Agent.</para>
	/// <para>Hold information about possible loot, ragdoll, animations etc.</para>
	/// </summary>
	[Serializable]
	public class Death
	{
		/// Reference to an object which will be instantiated on Die().
		[Header("Spawn")] 
		[SerializeField] private GameObject _ragdollPref;
		/// If true, Agent will drop all items from inventory upon death.
		[SerializeField] private bool _dropItems = true;
		
		[Header("Send Message On Death")]
		[Tooltip("Set a reference to any GameObject on scene to let it know about Agent's death.")]
		[SerializeField] private GameObject _messageReceiver;
		[Tooltip("Name of the method to be invoked.")]
		[SerializeField] private string _message;
		[Tooltip("String parameter to pass to the invoked method. Can be left empty.")]
		[SerializeField] private string _params;

		/// Sound that will be played upon Agent's death.
		[Space] [SerializeField] private List<AudioClip> _onDeathSounds;
		
		/// A link to actual controller.
		private Agent _agent;
		
		/// <summary>
		/// <para>Initialisation.</para>
		/// </summary>
		public void Init(Agent agent){ _agent = agent; if (!_ragdollPref) _ragdollPref = null; }
		
		/// <summary>
		/// Create new Death component with specific parameters.
		/// </summary>
		/// <param name="ragdollPref"></param>
		/// <returns></returns>
		public static Death CreateDeath(GameObject ragdollPref)
		{
			return new Death
			{
				_ragdollPref = ragdollPref
			};
		}

		/// <summary>
		/// <para>Spawn ragdoll, loot, etc. Log the event.</para>
		/// </summary>
		public void Die()
		{
			if (_agent.Inert) return;
			if(_agent.Motion.GetDefaultTarget())
				GameObject.Destroy(_agent.Motion.GetDefaultTarget().gameObject);
			if (_agent.AgentAnimation.ApplyRootMotion)
				GameObject.Destroy(_agent.AgentAnimation.Animator.gameObject);
			if (_ragdollPref)
			{
				var ragdoll = GameObject.Instantiate(_ragdollPref, _agent.transform.position, _agent.transform.rotation) as GameObject;
				if (_onDeathSounds.Count > 0)
				{
					var audioSource = ragdoll.GetComponent<AudioSource>()
						? ragdoll.GetComponent<AudioSource>()
						: ragdoll.AddComponent<AudioSource>();
					audioSource.PlayRandomClip(_onDeathSounds);
				}
			}

			if (_dropItems)
			{
				_agent.Inventory.DropAllItems();
			}

			GameObject.Destroy(_agent.gameObject);

			if (_messageReceiver)
			{
				if(!string.IsNullOrEmpty(_params))
					_messageReceiver.SendMessage(_message, _params, SendMessageOptions.DontRequireReceiver);
				else _messageReceiver.SendMessage(_message, SendMessageOptions.DontRequireReceiver);
			}
		}

		/// <summary>
		/// Set action to be performed upon death.
		/// </summary>
		/// <param name="messageReceiver"></param>
		/// <param name="message"></param>
		/// <param name="param"></param>
		public void SetOnDeathAction(GameObject messageReceiver, string message, string param = null)
		{
			_messageReceiver = messageReceiver;
			_message = message;
			_params = param;
		}
	}
}
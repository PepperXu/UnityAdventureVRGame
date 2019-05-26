using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// <para>Incapsulates the settings regarding the way the Agent should
	/// treat the current values of any his parameters or, in fact, define
	/// new parameters that are not fit to any other Agent's specific component.</para>
	/// </summary>
	[Serializable]
	public class Settings
	{
		/// <summary>
		/// How much HP is Low HP?
		/// </summary>
		public int LowHealth
		{
			get { return _lowHealth; }
			set { _lowHealth = value; }
		}

		/// <summary>
		/// How much Energy is Low Energy?
		/// </summary>
		public int LowEnergy
		{
			get { return _lowEnergy; }
			set { _lowEnergy = value; }
		}

		/// <summary>
		/// The distance to the Agent that is concidered a close distance.
		/// </summary>
		public float CloseDistance
		{
			get { return _closeDistance; }
			set { _closeDistance = value; }
		}

		/// <summary>
		/// The distance to the Agent that is concidered a mid distance.
		/// </summary>
		public float MidDistance
		{
			get { return _midDistance; }
			set { _midDistance = value; }
		}

		/// <summary>
		/// The distance to the Agent that is concidered a far distance.
		/// </summary>
		public float FarDistance
		{
			get { return _farDistance; }
			set { _farDistance = value; }
		}

		/// <summary>
		/// Range of degrees in front of Agent in which it can cast
		/// spell if it needs to be cast by ray.
		/// </summary>
		public float AimFieldOfView
		{
			get { return _aimFieldOfView; }
			set { _aimFieldOfView = value; }
		}

		/// <summary>
		/// Range of degrees behind the Agent which are concidered to be his back.
		/// </summary>
		public float BackFieldOfView
		{
			get { return _backFieldOfView; }
			set { _backFieldOfView = value; }
		}

		/// <summary>
		/// A Transform that is to be used as an origin of cast spells.
		/// </summary>
		public Transform ShootOrigin
		{
			get { return _shootOrigin; }
			set { _shootOrigin = value; }
		}

		/// <summary>
		/// Position in space where the Agent was at the level load or at spawning time.
		/// </summary>
		public Vector3 InitialPosition
		{
			get { return _initPosition; }
		}

		/// <summary>
		/// Distance that is concidered to be far from Agent's Waypoints Group if there is one.
		/// </summary>
		public float FarFromHome
		{
			get { return _farFromHome; }
			set { _farFromHome = value; }
		}

		/// <summary>
		/// Distance at which the Agent is concidered to be in range of his Waypoints Group if there is one.
		/// </summary>
		public float AtHomeRange
		{
			get { return _atHomeRange; }
			set { _atHomeRange = value; }
		}
		
		/// <summary>
		/// Returns wheather the user wants to dubug the Settings configuration.
		/// </summary>
		public bool Debug
		{
			get { return _debug; }
		}

		[Tooltip("How much HP is Low HP?")]
		[SerializeField] private int _lowHealth;
		[Tooltip("How much Energy is Low Energy?")]
		[SerializeField] private int _lowEnergy;
		[Space]
		[Tooltip("The distance to the Agent that is concidered a close distance.")]
		[SerializeField] private float _closeDistance = 2f;
		[Tooltip("The distance to the Agent that is concidered a mid distance.")]
		[SerializeField] private float _midDistance = 5f;
		[Tooltip("The distance to the Agent that is concidered a far distance.")]
		[SerializeField] private float _farDistance = 10f;
		[Tooltip("Distance that is concidered to be far from Agent's " +
		         "Waypoints Group if there is one.")]
		[SerializeField] private float _farFromHome = 50f;
		[Tooltip("Distance at which the Agent is concidered to be in range of " +
		         "his Waypoints Group if there is one.")]
		[SerializeField] private float _atHomeRange = 10f;
		[Space]
		[Tooltip("Range of degrees in front of Agent in which it can cast" +
		         " spell if it needs to be cast by ray.")]
		[SerializeField] private float _aimFieldOfView = 5f;
		[Tooltip("Range of degrees behind the Agent which are concidered to be his back.")]
		[SerializeField] private float _backFieldOfView = 5f;
		[Space]
		[Tooltip("A Transform that is to be used as an origin of cast spells.")]
		[SerializeField] private Transform _shootOrigin;
		
		[Space(10)]
		[Tooltip("Display the Settings configuration in editor?")]
		[SerializeField] private bool _debug = true;
		
		/// A link to actual controller.
		private Agent _agent;
		/// Position in space where the Agent was at the level load or at spawning time.
		private Vector3 _initPosition;
		/// Condition Interfaces of this instance of Agent's Settings component.
		private List<GeneralConditionInterface> _conditionInterfaces = new List<GeneralConditionInterface>();

        /// <summary>
        /// Initialize Agent's Settings component.
        /// </summary>
        /// <param name="agent"></param>
        public void Init(Agent agent)
		{
			_agent = agent;
			_initPosition = agent.transform.position;
			_shootOrigin = _agent.GetSkillOrigin();
		}

		/// <summary>
		/// Create new Settings component.
		/// </summary>
		public static Settings CreateSettings()
		{
			return new Settings();
		}

		#region ADD_INTARFACES
		public GeneralConditionInterface AddConditionInterface(string methodName)
		{
			return AgentFunctions.AddConditionInterface(methodName, ref _conditionInterfaces, _agent);
		}
		#endregion
    }
}
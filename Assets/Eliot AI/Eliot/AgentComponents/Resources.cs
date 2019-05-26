using System;
using System.Collections;
using System.Collections.Generic;
using Eliot.Utility;
using UnityEngine;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Keep information about finite resources (HP, Energy etc.).
	/// Resources usage is optional.
	/// </summary>
	[Serializable]
	public class Resources
	{
		/// <summary>
		/// Maximum amount of health that the Agent can have.
		/// </summary>
		public int MaxHealthPoints
		{
			get { return _healthPoints; }
		}

		/// <summary>
		/// Maximum amount of energy that the Agent can have.
		/// </summary>
		public int MaxEnergyPoints
		{
			get { return _energyPoints; }
		}

		/// <summary>
		/// Current amount of Agent's health.
		/// </summary>
		public int HealthPoints
		{
			get { return _curHealthPoints; }
		}

		/// <summary>
		/// Current amount of Agent's energy.
		/// </summary>
		public int EnergyPoints
		{
			get { return _curEnergyPoints; }
		}

		/// <summary>
		/// Wheather the agent uses health as a resource.
		/// </summary>
		public bool UsesHealth
		{
			get { return _useHealth; }
		}

		/// <summary>
		/// Wheather the agent uses energy as a resource.
		/// </summary>
		public bool UsesEnergy
		{
			get { return _useEnergy; }
		}

		/// <summary>
		/// Returns wheather the user wants to dubug the Resources configuration.
		/// </summary>
		public bool Debug
		{
			get { return _debug; }
		}

        public float AlertPoint
        {
            get { return _alertPoint; }
            set { _alertPoint = value; }
        }

		[Header("Health")] [SerializeField] private bool _useHealth;
		[SerializeField] private int _healthPoints = 10;
		[SerializeField] private int _curHealthPoints;
		[SerializeField] private bool _healOverTime = true;
		[SerializeField] private int _healAmount = 1;
		[SerializeField] private float _healCoolDown = 5;
		[Space]
		[Header("Energy")] [SerializeField] private bool _useEnergy;
		[SerializeField] private int _energyPoints = 10;
		[SerializeField] private int _curEnergyPoints;
		[SerializeField] private bool _addEnergyOverTime = true;
		[SerializeField] private int _addEnergyAmount = 1;
		[SerializeField] private float _addEnergyCoolDown = 5;
		[Space]
		[Header("Other")] 
		[SerializeField] private List<AudioClip> _onDamageSounds;
		[Tooltip("Duration for which Agent's movement and skills will be locked upon taking damage.")]
		[SerializeField] private float _lockTime = 0.3f;
		[Space]
		[Tooltip("Configuations of the actions to be taken upon Agent's death.")]
		[SerializeField] public Death Death;
        [Space]
        [SerializeField] private float _alertPoint;

		[Space(10)] 
		[Tooltip("Display the Resources configuration in editor?")]
		[SerializeField] private bool _debug = true;

		/// A link to actual controller.
		private Agent _agent;
		/// A link to Animation component of the Agent.
		private AgentAnimation _agentAnimation;
		/// Condition Interfaces of this instance of Agent's Resources component.
		private List<ResourcesConditionInterface> _conditionInterfaces = new List<ResourcesConditionInterface>();
		/// Last time the Agent automatically self-healed.
		private float _lastTimeHealed;
		/// Wheather the Agent is currently alive.
		private bool _alive = true;
		/// Last time the Agent automatically replenished his energy.
		private float _lastTimeAddedEnergy;

        /// <summary>
        /// Initialization of Agent's Resources component.
        /// </summary>
        /// <param name="agent"></param>
        public void Init(Agent agent)
		{
			_agent = agent;
			if (_useHealth) _curHealthPoints = _healthPoints;
			if (_useEnergy) _curEnergyPoints = _energyPoints;
			_agentAnimation = _agent.AgentAnimation;
			Death.Init(agent);
		}

		/// <summary>
		/// Create new resources component with specified configurations.
		/// </summary>
		/// <param name="useHealth"></param>
		/// <param name="maxHealth"></param>
		/// <param name="useEnergy"></param>
		/// <param name="maxEnergy"></param>
		/// <returns></returns>
		public static Resources CreateResources(bool useHealth = false, int maxHealth = 10, bool useEnergy = false,
			int maxEnergy = 10)
		{
			return new Resources
			{
				_useHealth = useHealth,
				_healthPoints = maxHealth,
				_useEnergy = useEnergy,
				_energyPoints = maxEnergy,
				Death = new Death()
			};
		}

		/// <summary>
		/// Take Agent's health, locking his components for specified
		/// amount of time and inducingspecific sound and animation.
		/// </summary>
		/// <param name="value"></param>
		public void Damage(int value)
		{
			if (!_useHealth) return;

			_agent.GetAudioSource().PlayRandomClip(_onDamageSounds);
			
			_curHealthPoints = Mathf.Max(0, _curHealthPoints - value);
			if (_lockTime > 0) _agent.Motion.Lock();
			if (_alive && _curHealthPoints <= 0)
			{
				_alive = false;
				Death.Die();
			}

			// Check wheather Agent needs to animate the damage.
			var animate = true;
			try
			{
				animate = !((_agent.CurrentSkill.State == SkillState.Loading
				             || _agent.CurrentSkill.State == SkillState.Invoking)
				            && !_agent.CurrentSkill.Interruptible);
			}
			catch (Exception)
			{
				/*ignore*/
			}

			if (animate)
			{
				switch (_agentAnimation.AnimationMode)
				{
					case AnimationMode.Legacy:
						_agentAnimation.Animate(AnimationState.TakingDamage, 0, true);
						break;
					case AnimationMode.Mecanim:
						_agentAnimation.Animate(_agentAnimation.Parameters.TakeDamageTrigger, _lockTime);
						break;
				}
				
			}
			if (_lockTime > 0) _agent.StartCoroutine(UnlockMovementEnum());
		}

		/// <summary>
		/// Replenish Agent's health.
		/// </summary>
		/// <param name="value"></param>
		public void Heal(int value)
		{
			if (!_useHealth) return;
			_curHealthPoints = Mathf.Min(_healthPoints, _curHealthPoints + value);
		}

		/// <summary>
		/// Replenish Agent's health for a random value in specified range.
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public void Heal(int minValue, int maxValue)
		{
			if (!_useHealth) return;
			Heal(UnityEngine.Random.Range(minValue, maxValue));
		}

		/// <summary>
		/// Depending on the value, damage Agent or heal him.
		/// </summary>
		/// <param name="value"></param>
		public void AddHealth(int value)
		{
			if (!_useHealth) return;

			if (value >= 0) Heal(value);
			else Damage(value);
		}

		/// <summary>
		/// Use energy if possible.
		/// </summary>
		/// <param name="value"></param>
		public void UseEnergy(int value)
		{
			if (!_useEnergy) return;
			if (_curEnergyPoints - value >= 0) _curEnergyPoints = Mathf.Max(0, _curEnergyPoints - value);
		}

		/// <summary>
		/// Use random amount of energy in specified range.
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public void UseEnergy(int minValue, int maxValue)
		{
			if (!_useEnergy) return;
			UseEnergy(UnityEngine.Random.Range(minValue, maxValue));
		}

		/// <summary>
		/// Replenish Agent's energy.
		/// </summary>
		/// <param name="value"></param>
		public void AddEnergy(int value)
		{
			if (!_useEnergy) return;
			_curEnergyPoints = Mathf.Min(_energyPoints, _curEnergyPoints + value);
		}

		/// <summary>
		/// Replenish random amount of Agent's energy in specified range.
		/// </summary>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public void AddEnergy(int minValue, int maxValue)
		{
			if (!_useEnergy) return;
			AddEnergy(UnityEngine.Random.Range(minValue, maxValue));
		}

		/// <summary>
		/// Update the self-replenishment properties of Agent's Resources.
		/// </summary>
		public void Update()
		{
			if (_useHealth && _healOverTime && Time.time >= _lastTimeHealed + _healCoolDown)
			{
				Heal(_healAmount);
				_lastTimeHealed = Time.time;
			}

			if (_useEnergy && _addEnergyOverTime && Time.time >= _lastTimeAddedEnergy + _addEnergyCoolDown)
			{
				AddEnergy(_addEnergyAmount);
				_lastTimeAddedEnergy = Time.time;
			}
		}

		/// <summary>
		/// Return wheather the health points are at maximum.
		/// </summary>
		/// <returns></returns>
		public bool HealthFull()
		{
			return _curHealthPoints == _healthPoints;
		}

		/// <summary>
		/// Return wheather the energy points are at maximum.
		/// </summary>
		/// <returns></returns>
		public bool EnergyFull()
		{
			return _curEnergyPoints == _energyPoints;
		}

		/// <summary>
		/// Wait for specified time and unlock Agent's movement.
		/// </summary>
		/// <returns></returns>
		private IEnumerator UnlockMovementEnum()
		{
			yield return new WaitForSeconds(_lockTime);
			_agent.Motion.Unlock();
		}


		#region ADD_INTARFACES
		public ResourcesConditionInterface AddConditionInterface(string methodName)
		{
			return AgentFunctions.AddConditionInterface(methodName, ref _conditionInterfaces, _agent);
		}
		#endregion
    }
}
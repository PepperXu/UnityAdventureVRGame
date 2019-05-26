using System;
using System.Collections;
using System.Collections.Generic;
using Eliot.Utility;
using UnityEngine;
using Unit = Eliot.Environment.Unit;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Skill incapsulates all the ways Agents can affect each other.
	/// Each skill is saved as a separate file.
	/// </summary>
	[Serializable][CreateAssetMenu(fileName = "New Skill", menuName = "Eliot/Skill")]
	public class Skill : ScriptableObject
	{
		/// <summary>
		/// Available options of the way the Skill affects target's health. 
		/// </summary>
		private enum AffectionWay { Add, Reduce }
		
		/// <summary>
		/// Represents current state of the Skill.
		/// </summary>
		public SkillState State
		{
			get { return _state; }
		}
		
		/// <summary>
		/// Return wheather this Skill can be interrupted.
		/// </summary>
		public bool Interruptible
		{
			get { return _interruptible; }
		}

		/// <summary>
		/// Return the range of the Skill.
		/// </summary>
		public float Range
		{
			get { return _range; }
		}

		/// <summary>
		/// How many energy points does this Skill require.
		/// </summary>
		public int EnergyCost
		{
			get { return _energyCost; }
		}

		/// <summary>
		/// Check if the Skill has healing properties.
		/// </summary>
		public bool AddsHealth
		{
			get { return _affectHealth && _healthAffectionWay == AffectionWay.Add; }
		}
		
		/// <summary>
		/// Check if the Skill has health reducing properties.
		/// </summary>
		public bool ReducesHealth
		{
			get { return _affectHealth && _healthAffectionWay == AffectionWay.Reduce; }
		}
		
		/// <summary>
		/// Check if the Skill has energy replenishing properties.
		/// </summary>
		public bool AddsEnergy
		{
			get { return _affectEnergy && _energyAffectionWay == AffectionWay.Add; }
		}
		
		/// <summary>
		/// Check if the Skill has energy reducing properties.
		/// </summary>
		public bool ReducesEnergy
		{
			get { return _affectEnergy && _energyAffectionWay == AffectionWay.Reduce; }
		}
		
		[Header("Basic")]
		[Tooltip("Can the loading of this skill be interrupted by other skills?")]
		[SerializeField] private bool _interruptible = true;
		[Tooltip("Can multiple identical skills affect the same Agent at the same time?")]
		[SerializeField] private bool _canStack;
		[SerializeField] private bool _canAffectEnemies = true;
		[SerializeField] private bool _canAffectFriends = true;
		[SerializeField] private InitSkillBy _initSkillBy;
		[Tooltip("Leave empty if Skill is not initialized by a projectile.")]
		[SerializeField] private Projectile _projectilePrefab;

		[Header("Area")]
		[SerializeField] private float _range;
		[SerializeField] private float _fieldOfView;

		[Header("Timing")]
		[SerializeField] private float _loadTime;
		[SerializeField] private float _invocationDuration;
		[SerializeField] private float _invocationPing;
		[SerializeField] private float _effectDuration;
		[SerializeField] private float _effectPing;
		[SerializeField] private float _coolDown;

		[Header("Economy")]
		[SerializeField] private int _minPower;
		[SerializeField] private int _maxPower;
		[SerializeField] private int _energyCost;
		[SerializeField] private float _pushPower;
		[Tooltip("Should this skill lock the target Agent?")]
		[SerializeField] private bool _interruptTarget;
		[Tooltip("Should Agent lock itself when casting the skill?")]
		[SerializeField] private bool _freezeMotion;

		[Header("Affection")]
		[SerializeField] private bool _affectHealth;
		[SerializeField] private AffectionWay _healthAffectionWay;
		[SerializeField] private bool _affectEnergy;
		[SerializeField] private AffectionWay _energyAffectionWay;
		[Space] 
		[Tooltip("Should the Skill change target's status?")]
		[SerializeField] private bool _setStatus;
		[SerializeField] private AgentStatus _status;
		[SerializeField] private float _statusDuration;
		[Space] 
		[Tooltip("Lets target know about casting position.")]
		[SerializeField] private bool _setPositionAsTarget;
		[Space] 
		[SerializeField] private bool _makeNoise;
		[SerializeField] private float _noiseDuration;
		[Tooltip("Side skills that are cast upon target as a consequence of casting the main one.")]
		[SerializeField] private List<Skill> _additionalEffects = new List<Skill>();

		[Header("Legacy Animations")]
		[SerializeField] private AnimationClip _loadingAnimation;
		[SerializeField] private AnimationClip _executingAnimation;
		[Header("Animator")]
		[SerializeField] private string _loadingMessage;
		[SerializeField] private string _executingMessage;

		[Header("FX")] 
		[Tooltip("Special effects Instantiated at the casting position.")]
		[SerializeField] private GameObject _onApplyFX;
		[Tooltip("Special effects Instantiated at target position.")]
		[SerializeField] private GameObject _onApplyFXOnTarget;
		[SerializeField] private bool _makeChildOfTarget;
		[Space]
		[Tooltip("Invoke any method on target upon applying the skill.")]
		[SerializeField] private string _onApplyMessageToTarget;
		[Tooltip("Pass a random value between min power and max power as" +
		         " a parameter of the above mentioned method.")]
		[SerializeField] private bool _passPowerToTarget;
		[Tooltip("Invoke any method on the caster Agent upon casting the skill.")]
		[SerializeField] private string _onApplyMessageToCaster;
		

		[Header("Audio")] 
		[SerializeField] private List<AudioClip> _loadingSkillSounds;
		[SerializeField] private List<AudioClip> _executingSkillSounds;
		
		/// A link to actual controller. Caster of the skill.
		private Agent _agent;
		/// Agent's Animation component.
		private AgentAnimation _agentAnimation;
		/// Wheather the skill is available for use.
		private bool _skillAvailable = true;
		/// Wheather the skill is interrupted.
		private bool _interrupted;
		/// Current state of the skill.
		private SkillState _state;
		/// Caster Agent's gameObject.
		private GameObject _sourceObject;
		/// Makes sure Agents with the same loadTime can both attack each other.
		private const float LoadTimeError = 0.05f;

		#region INTERFACE
		
		/// <summary>
		/// Make a non-file copy of the Skill.
		/// </summary>
		/// <returns></returns>
		public Skill Clone()
		{
			var result = CreateInstance<Skill>();
			result._canStack = _canStack;
			result._canAffectEnemies = _canAffectEnemies;
			result._canAffectFriends = _canAffectFriends;
			result._initSkillBy = _initSkillBy;
			result._projectilePrefab = _projectilePrefab;
			result.name = name;
			result._interruptible = _interruptible;
			result._interrupted = _interrupted;
			result._range = _range;
			result._fieldOfView = _fieldOfView;
			result._loadTime = _loadTime;
			result._invocationDuration = _invocationDuration;
			result._invocationPing = _invocationPing;
			result._effectDuration = _effectDuration;
			result._effectPing = _effectPing;
			result._coolDown = _coolDown;
			result._minPower = _minPower;
			result._maxPower = _maxPower;
			result._energyCost = _energyCost;
			result._pushPower = _pushPower;
			result._interruptTarget = _interruptTarget;
			result._freezeMotion = _freezeMotion;
			result._affectHealth = _affectHealth;
			result._healthAffectionWay = _healthAffectionWay;
			result._setStatus = _setStatus;
			result._status = _status;
			result._statusDuration = _statusDuration;
			result._setPositionAsTarget = _setPositionAsTarget;
			result._makeNoise = _makeNoise;
			result._noiseDuration = _noiseDuration;
			result._affectEnergy = _affectEnergy;
			result._energyAffectionWay = _energyAffectionWay;
			result._loadingAnimation = _loadingAnimation;
			result._executingAnimation = _executingAnimation;
			result._loadingMessage = _loadingMessage;
			result._executingMessage = _executingMessage;
			result._skillAvailable = _skillAvailable;
			result._state = _state;
			result._onApplyFX = _onApplyFX;
			result._onApplyFXOnTarget = _onApplyFXOnTarget;
			result._makeChildOfTarget = _makeChildOfTarget;
			result._onApplyMessageToTarget = _onApplyMessageToTarget;
			result._passPowerToTarget = _passPowerToTarget;
			result._onApplyMessageToCaster = _onApplyMessageToCaster;
			result._additionalEffects = _additionalEffects;
			result._loadingSkillSounds = _loadingSkillSounds;
			result._executingSkillSounds = _executingSkillSounds;
			return result;
		}
		
		/// <summary>
		/// Initialize all the components.
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public Skill Init(Agent agent, GameObject source)
		{
			_agent = agent;
			_sourceObject = source;
			_agentAnimation = _agent.AgentAnimation; 
			_state = SkillState.Idling; 
			return this; 
		}

		/// <summary>
		/// Trigger the execution of the skill.
		/// </summary>
		public void Invoke()
		{
			if (_skillAvailable && TrySpendEnergy(_energyCost))
			{
				_agent.StartCoroutine(INTERNAL_LoadEnum());
				_agent.CurrentSkill = this;
			} 
		}
		
		/// <summary>
		/// Prevent the skill from having its effects.
		/// </summary>
		public void Interrupt(){ if(_interruptible)_interrupted = true; }

		/// <summary>
		/// Let all Agents in specified radius hear suspitious noises from certain location.
		/// </summary>
		/// <param name="range"></param>
		/// <param name="transform"></param>
		/// <param name="duration"></param>
		public static void MakeNoise(float range, Transform transform, float duration)
		{
			var agents = FindObjectsOfType<Agent>();
			foreach (var agent in agents)
			{
				if (Vector3.Distance(agent.transform.position, transform.position) <= range)
				{
					agent.Perception.HearSomething(transform.position, duration);
				}
			}
		}
		
		#endregion
		
		#region INTERNAL

		/// <summary>
		/// Animate Agent's graphics by legacy Animation Clip.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="fadeLength"></param>
		private void Animate(AnimationClip clip, float fadeLength = 0.3F)
		{
			if(_agentAnimation != null)_agentAnimation.Animate(clip, fadeLength, true);
		}

		/// <summary>
		/// Set Agent's graphics Animator trigger.
		/// </summary>
		/// <param name="message"></param>
		private void Animate(string message)
		{
			if(_agentAnimation != null)_agentAnimation.Animate(message);
		}

		/// <summary>
		/// Trigger Agent's graphics default animation by state.
		/// </summary>
		/// <param name="state"></param>
		/// <param name="fadeLength"></param>
		private void Animate(AnimationState state, float fadeLength = 0.3F)
		{
			if(_agentAnimation != null)_agentAnimation.Animate(state, fadeLength, true);
		}

		/// <summary>
		/// Play animation of loading the skill.
		/// </summary>
		private void AnimateLoading()
		{
			if(_agentAnimation.AnimationMode == AnimationMode.Legacy) Animate(_loadingAnimation);
			else if(_agentAnimation.AnimationMode == AnimationMode.Mecanim) Animate(_loadingMessage);
			else Animate(AnimationState.LoadingSkill);
		}
		
		/// <summary>
		/// Play animation of executing the skill.
		/// </summary>
		private void AnimateInvoking()
		{
			if(_agentAnimation.AnimationMode == AnimationMode.Legacy) Animate(_executingAnimation);
			else if(_agentAnimation.AnimationMode == AnimationMode.Mecanim) Animate(_executingMessage);
			else Animate(AnimationState.UsingSkill);
		}
		
		/// <summary>
		/// Check if Agent has enough energy to invoke the skill
		/// and spend the necessary amount of it if true.
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		private bool TrySpendEnergy(int points)
		{
			if (_agent.Resources.EnergyPoints < points) return false;
			_agent.Resources.UseEnergy(points);
			return true;
		}
		#endregion
		
		#region LISTENERS
		
		/// <summary>
		/// Create all the necessary special effects upon starting to load the skill.
		/// </summary>
		/// <param name="source"></param>
		private void OnApplyEffectOnInit(GameObject source)
		{
			if (_onApplyFX)
			{
				var fx = Instantiate(_onApplyFX) as GameObject;
				fx.transform.position = _agent.GeneralSettings.ShootOrigin ? 
					_agent.GeneralSettings.ShootOrigin.position : _agent.Perception.Origin ? 
						_agent.Perception.Origin.position : _sourceObject.transform.position;
				if (_makeChildOfTarget) fx.transform.parent = _sourceObject.transform;
			}
			if (_makeNoise) MakeNoise(_range, _sourceObject.transform, _noiseDuration);
			if(_onApplyMessageToCaster.Length > 0)
				source.SendMessage(_onApplyMessageToCaster, SendMessageOptions.DontRequireReceiver);
		}
		
		/// <summary>
		/// Create all the necessary special effects upon applying the skill's effects.
		/// </summary>
		/// <param name="target"></param>
		private void OnApplyEffect(GameObject target)
		{
			if (_onApplyFXOnTarget)
			{
				var fx = Instantiate(_onApplyFXOnTarget) as GameObject;
				fx.transform.position = target.transform.position;
				if (_makeChildOfTarget) fx.transform.parent = target.transform;
			}
			if(_agent.Target && _onApplyMessageToTarget.Length > 0)
				_agent.Target.SendMessage(_onApplyMessageToTarget, SendMessageOptions.DontRequireReceiver);
		}
		#endregion
		
		#region INTERNEL ENUMS
		
		/// <summary>
		/// Start loading the skill.
		/// </summary>
		/// <returns></returns>
		private IEnumerator INTERNAL_LoadEnum()
		{
			_agent.GetAudioSource().PlayRandomClip(_loadingSkillSounds);
			
			_state = SkillState.Loading;
			if(_freezeMotion) _agent.Motion.Lock();
			AnimateLoading();
			_skillAvailable = false;
			yield return new WaitForSeconds(_loadTime + UnityEngine.Random.Range(-LoadTimeError, LoadTimeError));
			if (!_interrupted) _agent.StartCoroutine(INTERNAL_InvokeEnum());
			else {_interrupted = false; _agent.StartCoroutine(INTERNAL_CoolDownEnum()); }
		}
		
		/// <summary>
		/// Start applying skill's effects to the target or targets.
		/// </summary>
		/// <returns></returns>
		private IEnumerator INTERNAL_InvokeEnum()
		{
			_agent.GetAudioSource().PlayRandomClip(_executingSkillSounds);
			
			_state = SkillState.Invoking;
			if (_loadingAnimation && _executingAnimation && !_loadingAnimation.Equals(_executingAnimation)) 
				AnimateInvoking();
			var beginTime = Time.time;
			if(_invocationPing >= _invocationDuration) _agent.StartCoroutine(INTERNAL_ApplyEffectEnum());
			else while (Time.time <= beginTime + _invocationDuration)
			{
				_agent.StartCoroutine(INTERNAL_ApplyEffectEnum());
				if (_loadingAnimation && _executingAnimation && !_loadingAnimation.Equals(_executingAnimation)) 
					AnimateInvoking();
				yield return new WaitForSeconds(_invocationPing);
			}
			_agent.StartCoroutine(INTERNAL_CoolDownEnum());
		}
		
		/// <summary>
		/// Wait untill skill is recharged to be able to use it again.
		/// </summary>
		/// <returns></returns>
		private IEnumerator INTERNAL_CoolDownEnum()
		{
			_state = SkillState.CoolDown;
			if(_freezeMotion) _agent.Motion.Unlock();
			yield return new WaitForSeconds(_coolDown);
			_skillAvailable = true;
			_state = SkillState.Idling;

			if (_agentAnimation.AnimationMode == AnimationMode.Mecanim)
			{
				if(_loadingMessage.Length > 0)
					_agentAnimation.Animator.ResetTrigger(_loadingMessage);
				if(_executingMessage.Length > 0)
					_agentAnimation.Animator.ResetTrigger(_executingMessage);
			}
		}

		/// <summary>
		/// Implementation of finding the target.
		/// </summary>
		/// <returns></returns>
		private IEnumerator INTERNAL_ApplyEffectEnum()
		{
			// Create the loading special effects.
			OnApplyEffectOnInit(_sourceObject);
			
			// Cast the ray to find the target
			if (_initSkillBy == InitSkillBy.Ray)
			{
				//Try find target agent
				var targetDir = _agent.Target
					? _agent.Target.position - _agent.transform.position
					: Perception.InitRay(_range, 0, 90, _agent.Perception.Origin);
				var fov = _fieldOfView > 0 ? _fieldOfView : _agent.GeneralSettings.AimFieldOfView;
				if (Vector3.Angle(targetDir, _agent.Perception.Origin.forward) >= fov) yield break;
				RaycastHit hit;
				if (!Physics.Raycast(_agent.GeneralSettings.ShootOrigin.position, targetDir, out hit, _range)) yield break;
				var agent = hit.transform.gameObject.GetComponent<Agent>();
				
				var unit = hit.transform.gameObject.GetComponent<Unit>();
				if (unit)
				{
					var targetIsFriend = _agent.Unit.IsFriend(unit);
					if (targetIsFriend && !_canAffectFriends) yield break;
					if (!targetIsFriend && !_canAffectEnemies) yield break;
				}

				//Apply effects to him
				if (agent) agent.AddEffect(this, _agent);
				else INTERNAL_ApplyEffectNonAgent(hit.transform.gameObject); 
			}
			
			// Create a Projectile and pass the skill to it.
			else if (_initSkillBy == InitSkillBy.Projectile)
			{
				var origin = _agent.GeneralSettings.ShootOrigin ?? _agent.Perception.Origin;
				var pjtl = Instantiate(_projectilePrefab, origin.position, origin.rotation) as Projectile;
				//pjtl.GetComponent<Projectile>().Init(_agent, _agent.Target, this, _minPower, _maxPower, _canAffectEnemies, _canAffectFriends);
				pjtl.Init(_agent, _agent.Target, this, _minPower, _maxPower, _canAffectEnemies, _canAffectFriends);
			}
			
			// Apply the skill to oneself.
			else if (_initSkillBy == InitSkillBy.Self)
			{
				_agent.AddEffect(this, _agent);
			}
			
			// Apply effects to a target directly by a reference to the GameObject.
			else if (_initSkillBy == InitSkillBy.Direct)
			{
				if (!_agent.Target) yield return null;
				var agent = _agent.Target.GetComponent<Agent>();
				if (agent)
				{
					var unit = _agent.Target.GetComponent<Unit>();
					if (unit)
					{
						var targetIsFriend = _agent.Unit.IsFriend(unit);
						if (targetIsFriend && !_canAffectFriends) yield break;
						if (!targetIsFriend && !_canAffectEnemies) yield break;
					}
					
					agent.AddEffect(this, _agent);
				}
				else INTERNAL_ApplyEffectNonAgent(_agent.Target.gameObject); 
			}
			
			// Find all appropriate targets in cpecified radius and apply effects to them.
			else if (_initSkillBy == InitSkillBy.Radius)
			{
				var agents = FindObjectsOfType<Agent>();
				foreach (var agent in agents)
				{
					var targetIsFriend = _agent.Unit.IsFriend(agent.Unit);
					if (targetIsFriend && !_canAffectFriends) continue;
					if (!targetIsFriend && !_canAffectEnemies) continue;
					
					if(Vector3.Distance(agent.transform.position, _agent.transform.position) <= _range
					   && agent != _agent)
						agent.AddEffect(this, _agent);
				}
			}

			// Create special effects on target after applying the effects.
			if (_agent.Target)
			{
				var agent = _agent.Target.GetComponent<Agent>();
				if (agent)
				{
					var targetIsFriend = _agent.Unit.IsFriend(agent.Unit);
					if (targetIsFriend && !_canAffectFriends) yield break;
					if (!targetIsFriend && !_canAffectEnemies) yield break;
				}

				OnApplyEffect(_agent.Target.gameObject);
			}
		}
		
		/// <summary>
		/// Apply skill's effects over specific duration.
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="effectsList"></param>
		/// <returns></returns>
		public IEnumerator ApplyEffectEnum(Agent agent, List<Skill> effectsList = null)
		{
			var beginTime = Time.time;
			if(agent && _effectPing >= _effectDuration) INTERNAL_ApplyEffect(agent);
			else if(agent) while (Time.time <= beginTime + _effectDuration)
			{
				INTERNAL_ApplyEffect(agent);
				yield return new WaitForSeconds(_effectPing);
			}

			if (effectsList != null) effectsList.Remove(this);
		}
		
		/// <summary>
		/// Apply effects to the target changing its parameters.
		/// </summary>
		/// <param name="targetAgent"></param>
		private void INTERNAL_ApplyEffect(Agent targetAgent)
		{
			if (!targetAgent || !_agent) return;
			
			OnApplyEffect(targetAgent.gameObject);
			var hitPower = UnityEngine.Random.Range(_minPower, _maxPower + 1);
			
			if(_interruptTarget) targetAgent.Interrupt();
			if (_affectHealth)
			{
				switch (_healthAffectionWay)
				{
					case AffectionWay.Reduce:
						targetAgent.Resources.Damage(hitPower);
						break;
					case AffectionWay.Add:
						targetAgent.Resources.Heal(hitPower);
						break;
				}
			}

			if (_affectEnergy)
			{
				switch (_energyAffectionWay)
				{
					case AffectionWay.Reduce:
						targetAgent.Resources.UseEnergy(hitPower);
						break;
					case AffectionWay.Add:
						targetAgent.Resources.AddEnergy(hitPower);
						break;
				}
			}

			if (_pushPower > 0)
			{
				targetAgent.Motion.Push(_agent.transform.position, hitPower*_pushPower);
			}

			if (_setStatus) targetAgent.SetStatus(_status, _statusDuration);

			if (_setPositionAsTarget)
			{
				var tmp = targetAgent.Perception.Memory.GetDefaultEntity(_agent.transform.position);
				targetAgent.Target = tmp.transform;
			}

			if (_additionalEffects.Count <= 0) return;
			foreach (var effect in _additionalEffects)
				if(effect) targetAgent.AddEffect(effect, _agent);
		}
		
		/// <summary>
		/// If the target is not an Agent, invoke specified methods on it and create special effects.
		/// </summary>
		/// <param name="target"></param>
		private void INTERNAL_ApplyEffectNonAgent(GameObject target)
		{
			if (!target || !_agent) return;
			
			if (_onApplyFXOnTarget)
			{
				var fx = Instantiate(_onApplyFXOnTarget);
				fx.transform.position = target.transform.position;
				if (_makeChildOfTarget) fx.transform.parent = target.transform;
			}

			if (_passPowerToTarget)
			{
				var hitPower = UnityEngine.Random.Range(_minPower, _maxPower + 1); 
				target.SendMessage(_onApplyMessageToTarget, hitPower, SendMessageOptions.DontRequireReceiver);
			}
			else if (_onApplyMessageToTarget.Length > 0)
			{
				target.SendMessage(_onApplyMessageToTarget, SendMessageOptions.DontRequireReceiver);
			}

			if (_healthAffectionWay == AffectionWay.Reduce)
			{
				IntegrationDamageHandler.PassDamage(target, UnityEngine.Random.Range(_minPower, _maxPower + 1), _agent.gameObject, _pushPower);
			} 
		}
		#endregion
		
		#region UTILITY

		public float LifeTime()
		{
			return _loadTime + _invocationDuration + _effectDuration + _coolDown;
		}
		
		#endregion
	}
}
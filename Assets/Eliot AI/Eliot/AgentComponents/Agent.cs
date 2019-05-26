using System;
using System.Collections;
using System.Collections.Generic;
using Eliot.BehaviourEditor;
using Eliot.BehaviourEngine;
using Eliot.Environment;
using Eliot.Utility;
using Eliot.Repository;

#if ASTAR_EXISTS
using Pathfinding;
using Pathfinding.RVO;
#endif

using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif
using Unit = Eliot.Environment.Unit;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// <para>Agent incapsulates all the components of the behaviour of AI and works
	/// as a driver for the Behaviour model defined by user.</para>
	/// </summary>
	[Serializable]
	[RequireComponent(typeof(Unit))]
	public class Agent : MonoBehaviour
	{
		#region PROPERTIES

		/// <summary>
		/// The Behaviour model used by this Agent.
		/// </summary>
		public EliotBehaviour Behaviour
		{
			set { _behaviour = value; }
		}

		/// <summary>
		/// Behaviour driver that is built from Behaviour model diagram.
		/// </summary>
		public BehaviourCore BehaviourCore
		{
			get { return _behaviourCore; }
		}

		/// <summary>
		/// The Unit component of this gameObject.
		/// </summary>
		public Unit Unit { get; private set; }

		/// <summary>
		/// Current target. Coincides with this gameObject's NavMeshAgent target.
		/// </summary>
		public Transform Target
		{
			get { return _target; }
			set
			{
				_target = value;
				_motion.TargetTransform = value;
			}
		}

		/// <summary>
		/// Used to calculate at which distance is it possible to interact with agent.
		/// Especially noticable for large Agents.
		/// </summary>
		public float Radius
		{
			get { return _motion.Radius; }
		}

		/// <summary>
		/// How much time in seconds passes between updates.
		/// </summary>
		public float Ping
		{
			get { return _ping; }
			set { _ping = value; }
		}

		/// <summary>
		/// Current or last used skill.
		/// </summary>
		public Skill CurrentSkill
		{
			get { return _currentSkill; }
			set { _currentSkill = value; }
		}

		/// <summary>
		/// Set Agent inert when you want to use Agent's API, but dont want
		/// him to influence the behabiour of the game object.
		/// </summary>
		public bool Inert
		{
			get { return _inert; }
		}

		/// <summary>
		/// Set true if you want this Agent to apply changes to his attributes as
		/// defined by each waypoint he steps onto.
		/// </summary>
		public bool ApplyChangesOnWaypoints
		{
			get { return _applyChangesOnWaypoints; }
		}

		/// <summary>
		/// Current Agent's status. What status actually influences totally depends on
		/// the user's definition of it in the behaviour model.
		/// </summary>
		public AgentStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}

		/// <summary>
		/// Use to restrict walkable area, to set desired target places one by one
		/// or to pool agents in desired area.
		/// </summary>
		public WaypointsGroup Waypoints
		{
			get { return _waypoints; }
			set { _waypoints = value; }
		}

		/// <summary>
		/// Settings regarding Agent's internal economy.
		/// </summary>
		public Resources Resources
		{
			get { return _resources; }
			set { _resources = value; }
		}

		/// <summary>
		/// Settings regarding Agent's ability to understand what's going on around him.
		/// </summary>
		public Perception Perception
		{
			get { return _perception; }
			set { _perception = value; }
		}

		/// <summary>
		/// Settings of the way the Agent should move around.
		/// </summary>
		public Motion Motion
		{
			get { return _motion; }
			set { _motion = value; }
		}

		/// <summary>
		/// Setup reference to either Animation or Animator component that should be animated.
		/// Setup AnimationClips corresponding to certain Agent's actions.
		/// </summary>
		public AgentAnimation AgentAnimation
		{
			get { return _agentAnimation; }
			set { _agentAnimation = value; }
		}

		/// <summary>
		/// Items container. Agent can pickup items, throw them away, use them or wield them as weapons.
		/// </summary>
		public Inventory Inventory
		{
			get { return _inventory; }
			set { _inventory = value; }
		}

		/// <summary>
		/// All other settings of the Agent. User might want to extend it for more specific use of Unit.
		/// </summary>
		public Settings GeneralSettings
		{
			get { return _generalSettings; }
			set { _generalSettings = value; }
		}

		public List<Skill> Skills
		{
			get { return _skills; }
		}

		/// <summary>
		/// If true then information about current skill is shown as
		/// Handles text near selected Agent.
		/// </summary>
		public bool DebugSkill
		{
			get { return _debugSkill; }
		}

		/// <summary>
		/// If true then information about Agent's WaypointsGroup is shown
		/// as Handles text near selected Agent.
		/// </summary>
		public bool DebugWaypoints
		{
			get { return _debugWaypoints; }
		}

		/// <summary>
		/// If true then information about current Agent's target is shown
		/// as Handles text near selected Agent.
		/// </summary>
		public bool DebugTarget
		{
			get { return _debugTarget; }
		}

		/// <summary>
		/// Agent's default graphics. Agent's graphics is set to defualt when
		/// an Item from Inventory is unwield.
		/// </summary>
		private List<GameObject> DefaultGraphics { get; set; }

		/// <summary>
		/// Default perception origin position.
		/// </summary>
		private Vector3 DefaultPerceptionOriginPosition { get; set; }

		/// <summary>
		/// Default perception origin rotation.
		/// </summary>
		private Quaternion DefaultPerceptionOriginRotation { get; set; }

		/// <summary>
		/// Default skill origin position.
		/// </summary>
		private Vector3 DefaultSkillOriginPosition { get; set; }

		/// <summary>
		/// Default skill origin rotation.
		/// </summary>
		private Quaternion DefaultSkillOriginRotation { get; set; }

		/// <summary>
		/// Default animation mode.
		/// </summary>
		private AnimationMode DefaultAnimationMode { get; set; }

		#endregion

		#region FIELDS

		[Tooltip("Set Agent inert when you want to use Agent's API " +
		         "but dont want him to influence the behabiour of the game object.")]
		[SerializeField]
		private bool _inert;

		[Space] [Tooltip("The Behaviour model used by this Agent.")] [SerializeField]
		private EliotBehaviour _behaviour;

		[Tooltip("How much time in seconds passes between updates.")] [SerializeField]
		private float _ping;

		[Tooltip("Use to restrict walkable area, to set desired target " +
		         "places one by one or to pool agents in desired area.")]
		[SerializeField]
		private WaypointsGroup _waypoints;

		[Tooltip("Set true if you want this Agent to apply changes to his " +
		         "attributes as defined by each waypoint he steps onto.")]
		[SerializeField]
		private bool _applyChangesOnWaypoints;

		[Space] [Tooltip("Settings regarding Agent's internal economy.")] [SerializeField]
		private Resources _resources;

		[Tooltip("Settings regarding Agent's ability to understand " +
		         "what's going on around him.")]
		[SerializeField]
		private Perception _perception;

		[Tooltip("Settings of the way the Agent should move around.")] [SerializeField]
		private Motion _motion;

		[Tooltip("Setup reference to either Animation or Animator component that " +
		         "should be animated. Setup AnimationClips corresponding to certain Agent's actions.")]
		[SerializeField]
		private AgentAnimation _agentAnimation;

		[Tooltip("Items container. Agent can pickup items, throw them away, " +
		         "use them or wield them as weapons.")]
		[SerializeField]
		private Inventory _inventory;

		[Space]
		[Tooltip("All other settings of the Agent. User might want " +
		         "to extend it for more specific use of Unit.")]
		[SerializeField]
		private Settings _generalSettings;

		[Tooltip("Skills that the Agent can use.")] [SerializeField]
		private List<Skill> _skills = new List<Skill>();

		[Tooltip("Skills that are being applied to Agent. (Buffs / DoTs etc.)")] [SerializeField]
		private List<Skill> _activeEffects = new List<Skill>();

		[Space]
		[Tooltip("If true then information about current skill is shown as " +
		         "Handles text near selected Agent.")]
		[SerializeField]
		private bool _debugSkill = true;

		[Tooltip("If true then information about Agent's WaypointsGroup is " +
		         "shown as Handles text near selected Agent.")]
		[SerializeField]
		private bool _debugWaypoints = true;

		[Tooltip("If true then information about current Agent's target is " +
		         "shown as Handles text near selected Agent.")]
		[SerializeField]
		private bool _debugTarget = true;

		/// Current target. Coincides with this gameObject's NavMeshAgent target.
		private Transform _target;

		/// Last time the agent's behaviour was updated.
		private float _lastPingUpdate;

		/// Skills that the Agent can use.
		private List<Skill> _usableSkills = new List<Skill>();

		/// Current or last used skill.
		private Skill _currentSkill;

		/// Current Agent's status.
		private AgentStatus _status = AgentStatus.Normal;

		/// Behaviour driver that is built from Behaviour model diagram.
		private BehaviourCore _behaviourCore;

		#endregion

		/// <summary>
		/// Look for specific child of Agent's transform by name.
		/// Create such if not found.
		/// </summary>
		/// <param name="objName"></param>
		/// <returns></returns>
		public Transform FindTransformByName(string objName)
		{
			var child = transform.FindDeepChild(objName);
			if (child == null)
			{
				var newChild = new GameObject(objName);
				newChild.transform.parent = transform;
				newChild.transform.localPosition = new Vector3(0, 1, 0);
				child = newChild.transform;
			}

			return child;
		}

		/// <summary>
		/// Get the Transform Component of the Agent's graphics holder.
		/// </summary>
		private Transform GetGrapgicsContainer()
		{
			return FindTransformByName("__graphics__");
		}

		/// <summary>
		/// Get the Transform Component of the Agent's perception origin.
		/// </summary>
		public Transform GetPerceptionOrigin()
		{
			return FindTransformByName("__look__");
		}

		/// <summary>
		/// Get the Transform Component of the Agent's skill cast origin.
		/// </summary>
		public Transform GetSkillOrigin()
		{
			return FindTransformByName("__shoot__");
		}

		/// <summary>
		/// Get the Agent's AudioSource Component or add one if none is present.
		/// </summary>
		/// <returns></returns>
		public AudioSource GetAudioSource()
		{
			return GetComponent<AudioSource>() ? GetComponent<AudioSource>() : gameObject.AddComponent<AudioSource>();
		}

		/// <summary>
		/// Return the current target calculated by Motion component.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public Transform GetTargetObject(Vector3 pos)
		{
			return _motion.GetDefaultTarget(pos);
		}

		/// <summary>
		/// Enable Agent to use new skill.
		/// </summary>
		/// <param name="skill"></param>
		public void AddSkill(Skill skill)
		{
			if (!_skills.Contains(skill))
				_skills.Add(skill.Clone().Init(GetComponent<Agent>(), gameObject));
			if (!_usableSkills.Contains(skill))
				_usableSkills.Add(skill.Clone().Init(GetComponent<Agent>(), gameObject));
		}

		/// <summary>
		/// Prevent Agent from using certain skill.
		/// </summary>
		/// <param name="skill"></param>
		public void RemoveSkill(Skill skill)
		{
			if (_skills.Contains(skill))
				_skills.Remove(skill);
			if (_usableSkills.Contains(skill))
				_usableSkills.Remove(skill);
		}

		/// <summary>
		/// Add effect (DoT/Buff etc.) to Agent.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="caster"></param>
		public void AddEffect(Skill effect, Agent caster)
		{
			var contains = false;
			foreach (var skl in _activeEffects)
				if (skl.name == effect.name)
				{
					contains = true;
					break;
				}

			if (contains) return;

			var clone = effect.Clone().Init(caster!=null?caster:this, gameObject);
			_activeEffects.Add(clone);
			StartCoroutine(clone.ApplyEffectEnum(GetComponent<Agent>(), _activeEffects));
		}

		/// <summary>
		/// Use for adding an empty inert Agent Component to a gameObject via code.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="isInert"></param>
		/// <returns></returns>
		public static Agent AddAgentComponent(GameObject target, bool isInert)
		{
			var agent = target.AddComponent<Agent>();
			agent._inert = isInert;
			agent.Resources = Resources.CreateResources(false, 0, false, 0);
			agent.Resources.Death = Death.CreateDeath(null);
			agent.Perception =  Perception.CreatePerception(10, 180, 7);
			agent.Perception.Memory = Memory.CreateMemory(10);
			agent.Motion = Motion.CreateMotion(MotionEngine.Turret, 0, false, false);
			agent.GeneralSettings = Settings.CreateSettings();
			agent.Inventory = new Inventory(agent, null);
			agent.AgentAnimation = new AgentAnimation();
			return agent;
		}

		/// <summary>
		/// <para>Create a new Agent with specific components.</para>
		/// </summary>
		/// <param name="agentName"></param>
		/// <param name="resources"></param>
		/// <param name="memory"></param>
		/// <param name="perception"></param>
		/// <param name="motion"></param>
		/// <param name="death"></param>
		/// <param name="settings"></param>
		/// <param name="initSkill"></param>
		/// <param name="graphics"></param>
		/// <returns></returns>
		public static Agent CreateAgent(string agentName,
			Resources resources, Memory memory, Perception perception, Motion motion, Death death, Settings settings,
			List<Skill> initSkill, GameObject graphics = null)
		{
			// Create all necessary GameObjects
			var newAgent = new GameObject(agentName);
			var lookPos = new GameObject("__look__");
			var shootPos = new GameObject("__shoot__");
			var graphicsPos = new GameObject("__graphics__");
			var inventoryPos = new GameObject("__inventory__");
			lookPos.transform.parent = newAgent.transform;
			shootPos.transform.parent = newAgent.transform;
			graphicsPos.transform.parent = newAgent.transform;
			inventoryPos.transform.parent = newAgent.transform;
			lookPos.transform.localPosition = new Vector3(0, 1, 0);
			shootPos.transform.localPosition = new Vector3(0, 1, 0);
			graphicsPos.transform.localPosition = new Vector3(0, 1, 0);
			inventoryPos.transform.localPosition = new Vector3(0, 1, 0);

			var collider = newAgent.AddComponent<CapsuleCollider>();
			collider.center = new Vector3(0, 1, 0);

			// Instantiate graphics
			if (graphics)
			{
				var gr =
					Instantiate(graphics, graphicsPos.transform.position, graphicsPos.transform.rotation) as GameObject;
				gr.transform.parent = graphicsPos.transform;
			}

			// Add Agent Component and set all his components up
			var agent = newAgent.AddComponent<Agent>();
			agent._resources = resources;
			agent._resources.Death = death;
			agent._perception = perception;
			agent._perception.Origin = lookPos.transform;
			agent._perception.Memory = memory;
			agent._motion = motion;

			if (motion.Type == MotionEngine.NavMesh) // No need to add NavMeshAgent if Agent is not a creature
			{
				var navMesh = newAgent.AddComponent<NavMeshAgent>();
				navMesh.acceleration = 100;
				navMesh.angularSpeed = 1000;
			}

#if ASTAR_EXISTS
			else if (motion.Type == MotionEngine.Astar)
			{
				var starAi = agent.gameObject.AddComponent<AIPath>();
				starAi.height = 2f;
				starAi.radius = collider.radius;

				agent.gameObject.AddComponent<Seeker>();
				agent.gameObject.AddComponent<FunnelModifier>();
				var rvoController = agent.gameObject.AddComponent<RVOController>();
				rvoController.agentTimeHorizon = 0.01f;
				rvoController.obstacleTimeHorizon = 0.01f;

				if (!GameObject.FindObjectOfType<RVOSimulator>())
				{
					new GameObject("RVOSimulator").AddComponent<RVOSimulator>();
					Debug.Log("A new GameObject of type RVOSimulator has been created");
				}
			}
#endif

			agent._generalSettings = settings;
			agent._generalSettings.ShootOrigin = shootPos.transform;
			agent._inventory = new Inventory(agent, inventoryPos.transform);
			agent._skills = initSkill;

			return agent;
		}

		/// <summary>
		/// Initialize Agent on scene load.
		/// </summary>
		private void Start()
		{
			Unit = GetComponent<Unit>();
			var qa = GetComponent<Agent>();
			foreach (var skill in _skills)
				_usableSkills.Add(skill.Clone().Init(qa, gameObject));

			_resources.Init(qa);
			_perception.Init(qa);
			_motion.Init(qa);
			_generalSettings.Init(qa);
			_inventory.Init(qa, FindTransformByName("__inventory__"));
			_agentAnimation.Init(qa);

			DefaultGraphics = new List<GameObject>();
			var container = GetGrapgicsContainer();
			if (container.childCount > 0)
				for (var i = container.childCount - 1; i >= 0; i--)
					DefaultGraphics.Add(container.GetChild(i).gameObject);

			DefaultPerceptionOriginPosition = GetPerceptionOrigin().localPosition;
			DefaultPerceptionOriginRotation = GetPerceptionOrigin().localRotation;

			DefaultSkillOriginPosition = GetSkillOrigin().localPosition;
			DefaultSkillOriginRotation = GetSkillOrigin().localRotation;

			DefaultAnimationMode = _agentAnimation.AnimationMode;

			if (!_behaviour) return;
			_behaviourCore = CoresPool.GetCore(_behaviour, gameObject);
		}

		/// <summary>
		/// Update components logic and Behaviour states every frame.
		/// </summary>
		public void Update()
		{
			if (_inert) return;

			if (_behaviourCore == null) return;
			if (Time.time < _lastPingUpdate + _ping) return;
			_behaviourCore.Update();
			_resources.Update();
			_agentAnimation.Update();
			_lastPingUpdate = Time.time;

            Debug.Log(Status);
		}

		/// <summary>
		/// Update components that work with physics.
		/// </summary>
		private void FixedUpdate()
		{
			if (_inert) return;

			if (Time.time < _lastPingUpdate + _ping) return;
			_perception.Update();
		}

		/// <summary>
		/// Get the Invoke method of a skill if Agent has such.
		/// Return nothing if no such skill was found.
		/// </summary>
		/// <param name="skillName"></param>
		/// <param name="execute"></param>
		/// <returns></returns>
		public EliotAction Skill(string skillName, bool execute)
		{
			if (execute)
			{
				foreach (var skill in _usableSkills)
					if (skill.name == skillName)
					{
						_currentSkill = skill;
						return skill.Invoke;
					}
			}
			else
			{
				return () =>
				{
					foreach (var skill in _usableSkills)
						if (skill.name == skillName)
						{
							_currentSkill = skill;
						}
				};
			}

			return null;
		}

		/// <summary>
		/// Interrupt the execution of a skill.
		/// </summary>
		public void Interrupt()
		{
			if (_currentSkill != null)
				_currentSkill.Interrupt();
		}

		/// <summary>
		/// Assign new Behaviour model to Agent.
		/// </summary>
		/// <param name="behaviour"></param>
		public void SetBehaviour(EliotBehaviour behaviour)
		{
			try{
				_behaviour = behaviour;
				_behaviourCore = CoresPool.GetCore(_behaviour, gameObject);
			}catch(Exception){/**/}
		}

		/// <summary>
		/// Apply changes to Agent component according to Waypoint's settings.
		/// </summary>
		/// <param name="waypoint"></param>
		public void SetWaypointChanges(Waypoint waypoint)
		{
			// But do it only if both Waypoint and Agent agree to do the change.
			if (!_applyChangesOnWaypoints) return;
			if (!waypoint.MakeChangesToAgent) return;

			if (waypoint.NewBehaviour)
				SetBehaviour(waypoint.NewBehaviour);
			if (waypoint.NewWaypoints)
				_waypoints = waypoint.NewWaypoints;
		}

		/// <summary>
		/// Replace the Agent's graphics, keeping proper references to components if needed.
		/// </summary>
		/// <param name="newGraphics"></param>
		public void ReplaceGraphics(GameObject newGraphics)
		{
			// Clear the old graphics.
			var container = GetGrapgicsContainer();
			if (DefaultGraphics.Count > 0)
				foreach (var graphics in DefaultGraphics)
					graphics.SetActive(false);

			// Instantiate a new one
			var inst = Instantiate(newGraphics, container.position, container.rotation) as GameObject;
			inst.transform.parent = container;
			// Look for an AgentGraphics component on a new graphics gameObject
			var agentGraphics = inst.GetComponent<AgentGraphics>();
			if (!agentGraphics) return; // If there is none, job is finished
			// Recover the reference to Animation or Animator component.
			if (_agentAnimation.AnimationMode == AnimationMode.Legacy)
				_agentAnimation.LegacyAnimation = agentGraphics.Animation;
			else _agentAnimation.Animator = agentGraphics.Animator;

			// Set a new transform of the spell caster origin
			if (agentGraphics.NewShooterPosition)
			{
				_generalSettings.ShootOrigin.localPosition = agentGraphics.NewShooterPosition.localPosition;
				_generalSettings.ShootOrigin.localRotation = agentGraphics.NewShooterPosition.localRotation;
			}

			// Set a new transform of the perception origin
			if (agentGraphics.NewPerceptionOriginPosition)
			{
				_perception.Origin.localPosition = agentGraphics.NewPerceptionOriginPosition.localPosition;
				_perception.Origin.localRotation = agentGraphics.NewPerceptionOriginPosition.localRotation;
			}

			// Invoke any custom method for more complex changes in Agent's graphics
			if (agentGraphics.SendMessageOnChange)
			{
				if (!string.IsNullOrEmpty(agentGraphics.MethodParams))
					gameObject.SendMessage(agentGraphics.MethodName, agentGraphics.MethodParams,
						SendMessageOptions.DontRequireReceiver);
				else
					gameObject.SendMessage(agentGraphics.MethodName,
						SendMessageOptions.DontRequireReceiver);
			}
		}

		/// <summary>
		/// Set all settings that are related to graphics
		/// and internal objects positions to default.
		/// </summary>
		public void ResetGraphics()
		{
			var container = GetGrapgicsContainer();
			if (container.childCount > 0)
				for (var i = container.childCount - 1; i >= 0; i--)
					if (!DefaultGraphics.Contains(container.GetChild(i).gameObject))
						Destroy(container.GetChild(i).gameObject);

			foreach (var graphics in DefaultGraphics)
				graphics.SetActive(true);

			_perception.Origin.localPosition = DefaultPerceptionOriginPosition;
			_perception.Origin.localRotation = DefaultPerceptionOriginRotation;

			_generalSettings.ShootOrigin.localPosition = DefaultSkillOriginPosition;
			_generalSettings.ShootOrigin.localRotation = DefaultSkillOriginRotation;
		}

		/// <summary>
		/// Set a new status to the Agent for a specific duration.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="statusDuration"></param>
		public void SetStatus(AgentStatus stat, float statusDuration)
		{
			_status = stat;
			StartCoroutine(ResetStatusIn(statusDuration));
		}

		/// <summary>
		/// Reset status back to normal.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		private IEnumerator ResetStatusIn(float time)
		{
			yield return new WaitForSeconds(time);
			_status = AgentStatus.Normal;
		}

		/// <summary>
		/// Damage the Agent after, for example, raycast check.
		/// </summary>
		/// <param name="power"></param>
		public void Damage(int power)
		{
			Resources.Damage(power);
		}

		/// <summary>
		/// Damage the Agent after, for example, raycast check.
		/// </summary>
		/// <param name="power"></param>
		public void Damage(float power)
		{
			Resources.Damage(Mathf.RoundToInt(power));
		}
	}
}

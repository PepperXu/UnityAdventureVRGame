#pragma warning disable CS0649, CS1692
using System;
using System.Collections.Generic;
using Eliot.Environment;
using Eliot.Utility;
using UnityEngine;

namespace Eliot.AgentComponents
{
    /// <summary>
    /// Incapsulates Agent's abilities to move around in space.
    /// </summary>
    [Serializable]
    public class Motion
    {
        #region PROPERTIES
        /// <summary>
        /// Radius is used to calculate the distance for interaction.
        /// </summary>
        public float Radius
        {
            get
            {
                return _agent.GetComponent<CapsuleCollider>() != null ? _agent.GetComponent<CapsuleCollider>().radius : 0;
            }
        }

        /// <summary>
        /// Destination for Agent's movement.
        /// </summary>
        public Vector3 Target
        {
            get { return _motionEngine.GetTarget(); }
            set { _motionEngine.SetTarget(value); }
        }

        /// <summary>
        /// Availability of Agent's movement.
        /// </summary>
        public bool Locked
        {
            get { return _motionEngine.Locked(); }
        }

        /// <summary>
        /// Defines how hard is it to push Agent around.
        /// </summary>
        public float Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        /// <summary>
        /// Transform component of Agent's target.
        /// </summary>
        public Transform TargetTransform
        {
            get; set;
        }

        /// <summary>
        /// Current state of Agent's Motion Component.
        /// </summary>
        public MotionState State
        {
            get { return _state; }
            set { _state = value; }
        }

        /// <summary>
        /// Agent's Motion Type.
        /// </summary>
        public MotionEngine Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Returns wheather the user wants to dubug the Motion configuration.
        /// </summary>
        public bool Debug
        {
            get { return _debug; }
        }

        /// <summary>
        /// The speed with which the Agent walks.
        /// </summary>
        public float MoveSpeed
        {
            get { return _moveSpeed; }
        }

        /// <summary>
        /// The speed with which the Agent runs.
        /// </summary>
        public float RunSpeed
        {
            get { return _runSpeed; }
        }

        /// <summary>
        /// Units of energy per second which it takes Agent to walk.
        /// </summary>
        public int MoveCost
        {
            get { return _moveCost; }
        }

        /// <summary>
        /// Units of energy per second which it takes Agent to run.
        /// </summary>
        public int RunCost
        {
            get { return _runCost; }
        }

        /// <summary>
        /// Speed with which Agent will turn towards target.
        /// </summary>
        public float RotationSpeed
        {
            get { return _rotationSpeed; }
        }

        /// <summary>
        /// Defines the speed with which the Agent is moving while dodging.
        /// </summary>
        public float DodgeSpeed
        {
            get { return _dodgeSpeed; }
        }

        /// <summary>
        /// Time in seconds that passes between calling Dodge and actually dodging.
        /// </summary>
        public float DodgeDelay
        {
            get { return _dodgeDelay; }
        }

        /// <summary>
        /// Amount of time that needs to pass after dodge to be able to do it again.
        /// </summary>
        public float DodgeCoolDown
        {
            get { return _dodgeCoolDown; }
        }

        /// <summary>
        /// The amount of time for which the Agent is performoing the dodge.
        /// </summary>
        public float DodgeDuration
        {
            get { return _dodgeDuration; }
        }

        /// <summary>
        /// Duration of standing between location change while patroling.
        /// </summary>
        public float PatrolTime
        {
            get { return _patrolTime; }
        }

        /// <summary>
        /// Clip that is used as a sound of Agent's step while it is walking.
        /// </summary>
        public List<AudioClip> WalkingStepSounds
        {
            get { return _walkingStepSounds; }
        }

        /// <summary>
        /// Amount of time that passes between playing the WalkingStepSound.
        /// </summary>
        public float WalkingStepPing
        {
            get { return _walkingStepPing; }
        }

        /// <summary>
        /// Clip that is used as a sound of Agent's step while it is running.
        /// </summary>
        public List<AudioClip> RunningStepSounds
        {
            get { return _runningStepSounds; }
        }

        /// <summary>
        /// Amount of time that passes between playing the WalkingStepSound.
        /// </summary>
        public float RunningStepPing
        {
            get { return _runningStepPing; }
        }

        /// <summary>
        /// Clip that is used as a sound of Agent's dodge.
        /// </summary>
        public List<AudioClip> DodgingSounds
        {
            get { return _dodgingSounds; }
        }

        /// <summary>
        /// Moving part of the cannon.
        /// </summary>
        public Transform Head
        {
            get { return _head; }
        }

        /// <summary>
        /// Speed of rotation from side to side while idling.
        /// </summary>
        public float IdleRotationSpeed
        {
            get { return _idleRotationSpeed; }
            set { _idleRotationSpeed = value; }
        }

        /// <summary>
        /// Set true if cannon's idle rotation angles should be restricted.
        /// </summary>
        public bool ClampIdleRotation
        {
            get { return _clampIdleRotation; }
        }

        /// <summary>
        /// Starting angle of cannon's idle rotation.
        /// </summary>
        public float ClampedIdleRotStart
        {
            get { return _clampedIdleRotStart; }
        }

        /// <summary>
        /// Ending angle of cannon's idle rotation.
        /// </summary>
        public float ClampedIdleRotEnd
        {
            get { return _clampedIdleRotEnd; }
        }

        /// <summary>
        /// The engine that the Agent uses for pathfinding.
        /// </summary>
        public IMotionEngine Engine
        {
            get { return _motionEngine; }
        }

        #endregion

        #region CUSTOMIZATION
        /// Agent's Motion Type.
        [SerializeField] private MotionEngine _type;
        [Header("[Creature]")]
        [Tooltip("Defines how hard it is to push Agent around.")]
        [SerializeField] private float _weight = 1.0f;
        [Header("Move")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _runSpeed = 10f;
        [Tooltip("Units of energy per second which it takes Agent to move.")]
        [SerializeField] private int _moveCost = 0;
        [Tooltip("Units of energy per second which it takes Agent to run.")]
        [SerializeField] private int _runCost = 0;
        [Tooltip("Speed with which Agent will turn towards target.")]
        [SerializeField] private float _rotationSpeed = 10f;
        [Header("Dodge")]
        [SerializeField] private float _dodgeSpeed = 10f;
        [SerializeField] private float _dodgeDelay = 0.0f;
        [SerializeField] private float _dodgeCoolDown = 2f;
        [SerializeField] private float _dodgeDuration = .1f;
        [Header("Patrol")]
        [Tooltip("Duration of standing between location change while patroling.")]
        [SerializeField] private float _patrolTime = 5f;
        [Header("Audio")]
        [SerializeField] private List<AudioClip> _walkingStepSounds;
        [SerializeField] private float _walkingStepPing = 0.5f;
        [SerializeField] private List<AudioClip> _runningStepSounds;
        [SerializeField] private float _runningStepPing = 0.2f;
        [SerializeField] private List<AudioClip> _dodgingSounds;

        [Space(5)]
        [Header("[Turret]")]
        [Tooltip("Moving part of the cannon.")]
        [SerializeField] private Transform _head;
        [Tooltip("Speed of rotation from side to side while idling.")]
        [SerializeField] private float _idleRotationSpeed;
        [Tooltip("Set true if cannon's idle rotation angles should be restricted.")]
        [SerializeField] private bool _clampIdleRotation;
        [Tooltip("Starting angle of cannon's idle rotation.")]
        [SerializeField] private float _clampedIdleRotStart;
        [Tooltip("Ending angle of cannon's idle rotation.")]
        [SerializeField] private float _clampedIdleRotEnd;

        [Space(10)]
        [Tooltip("Display the Motion configuration in editor?")]
        [SerializeField] private bool _debug = true;
        #endregion

        #region FIELDS
        /// The engine used by this Agent for pathfinding.
        private IMotionEngine _motionEngine;
        /// Current state of Agent's Motion Component.
        private MotionState _state;
        /// Animation component of the Agent.
        private AgentAnimation _agentAnimation;
        /// Link to Agent Component of gameObject.
        private Agent _agent;
        /// Index of a waypoint from a waypoint group that is being an Agent's target.
        private int _curWaypointIndex;
        /// Make sure Agent's Motion never misses a target.
        private Transform _defaultTarget;
        /// Last time Agent's Motion component made a sound.
        private float _lastSoundTime;
        /// Action Interfaces for this Motion instance.
        private List<MotionActionInterface> _actionInterfaces = new List<MotionActionInterface>();
        /// Condition Interfaces for this Motion instance.
        private List<MotionConditionInterface> _conditionInterfaces = new List<MotionConditionInterface>();

        #endregion

        /// <summary>
        /// Get the Transform component of the default target object.
        /// </summary>
        /// <returns></returns>
        public Transform GetDefaultTarget()
        {
            if (!_defaultTarget)
            {
                var newGo = new GameObject("__target__[" + _agent.name + "]__");
                newGo.transform.position = _agent.transform.position;
                var waypoint = newGo.AddComponent<Waypoint>();
                var entity = newGo.AddComponent<Unit>();
                entity.Type = Environment.UnitType.Agent;
                _defaultTarget = waypoint.transform;
            }
            return _defaultTarget;
        }

        /// <summary>
        /// Move the default target object to specific position and get its Transform component.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Transform GetDefaultTarget(Vector3 pos)
        {
            if (!_defaultTarget)
            {
                var newGo = new GameObject("__target__[" + _agent.name + "]__");
                newGo.transform.position = pos;
                var waypoint = newGo.AddComponent<Waypoint>();
                var entity = newGo.AddComponent<Unit>();
                entity.Type = Environment.UnitType.Agent;
                _defaultTarget = waypoint.transform;
            }

            _defaultTarget.position = pos;
            return _defaultTarget;
        }

        /// <summary>
        /// Create new Motion component.
        /// </summary>
        public static Motion CreateMotion(MotionEngine type, float moveSpeed, bool canRun, bool canDodge)
        {
            return new Motion
            {
                _type = type,
                _moveSpeed = moveSpeed,
                _runSpeed = canRun ? moveSpeed * 2 : moveSpeed,
                _dodgeSpeed = canDodge ? 10f : 0f,
                _dodgeDuration = canDodge ? 0.1f : 0f,
            };
        }

        /// <summary>
        /// Invoke animation on Agent's graphics.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="fadeLength"></param>
        public void Animate(AnimationState state, float fadeLength = 0.3F)
        {
            if (_agentAnimation != null)
                _agentAnimation.Animate(state, fadeLength);
        }

        /// <summary>
        /// Play Audio Clip via Agent's AudioSource component.
        /// </summary>
        /// <param name="audio"></param>
        /// <param name="ping"></param>
        public void PlayAudio(List<AudioClip> audio, float ping)
        {
            if (!_agent.GetAudioSource().isPlaying && Time.time >= _lastSoundTime + ping)
            {
                _agent.GetAudioSource().PlayRandomClip(audio);
                _lastSoundTime = Time.time;
            }
        }

        /// <summary>
        /// Get Waypoint component of the Agent's current waypoint.
        /// </summary>
        /// <returns></returns>
        public Waypoint CurrentWaypoint()
        {
            return _agent.Waypoints ? _agent.Waypoints[_curWaypointIndex - 1] : null;
        }

        /// <summary>
        /// Initialize Motion component of the Agent.
        /// </summary>
        /// <param name="agent"></param>
        public void Init(Agent agent)
        {
            _agent = agent;
            _agentAnimation = agent.AgentAnimation;
            switch (_type)
            {
                case MotionEngine.Turret:
                    _motionEngine = new TurretMotionEngine();
                    break;
                case MotionEngine.NavMesh:
                    _motionEngine = new NavMeshMotionEngine();
                    break;
#if ASTAR_EXISTS
		        case MotionEngine.Astar:
			        _motionEngine = new AstarMotionEngine();
			        break;
#endif
            }
            if(_motionEngine != null)
                _motionEngine.Init(agent);
        }

        /// <summary>
        /// Lock the Agent's Motion component.
        /// </summary>
        public void Lock()
        {
            if(_motionEngine != null)
                _motionEngine.Lock();
        }

        /// <summary>
        /// Unlock the Agent's Motion component.
        /// </summary>
        public void Unlock()
        {
            if(_motionEngine != null)
                _motionEngine.Unlock();
        }

        #region EXTERNAL INFLUENCE
        /// <summary>
        /// Push Agent using NavMeshAgent's Warp method.
        /// </summary>
        /// <param name="pusherPos"></param>
        /// <param name="pushPower"></param>
        public void Push(Vector3 pusherPos, float pushPower)
        {
            _motionEngine.Push(pusherPos, pushPower);
        }
        #endregion

        #region ADD_INTARFACES
        public MotionActionInterface AddActionInterface(string methodName)
        {
            return AgentFunctions.AddActionInterface(methodName, ref _actionInterfaces, _agent);
        }

        public MotionConditionInterface AddConditionInterface(string methodName)
        {
            return AgentFunctions.AddConditionInterface(methodName, ref _conditionInterfaces, _agent);
        }
        #endregion
    }
}

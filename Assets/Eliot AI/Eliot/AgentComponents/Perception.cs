using System;
using System.Collections.Generic;
using Eliot.Environment;
using UnityEngine;

namespace Eliot.AgentComponents
{
    /// <summary>
    /// Incapsulates Agent's abilities to understand what is going on around.
    /// </summary>
    [Serializable]
    public class Perception
    {
        /// <summary>
        /// <para>Cache Agent's focus.</para>
        /// </summary>
        public Unit TargetUnit
        {
            get; set;
        }

        /// <summary>
        /// List of Entities that are spotted every Update.
        /// </summary>
        public List<Unit> SeenUnits
        {
            get { return _seenUnits; }
            set { _seenUnits = value; }
        }
        
        /// <summary>
        /// Last position in space where happened something that seemed suspitious to Agent.
        /// </summary>
        public Vector3 SuspiciousPosition
        {
            get { return _suspiciousPosition; }
        }

        /// <summary>
        /// Position out of which rays are cast.
        /// </summary>
        public Transform Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }

        /// <summary>
        /// Degrees for which the y rotation of rays' direction is being offset.
        /// </summary>
        public float Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        /// <summary>
        /// Field of Agent's view in degrees.
        /// </summary>
        public float FieldOfView
        {
            get { return _fieldOfView; }
            set { _fieldOfView = value; }
        }

        /// <summary>
        /// Number of rays that are being cast uniformally in Agent's field of view.
        /// </summary>
        public int Resolution
        {
            get { return _resolution; }
            set { _resolution = value; }
        }

        /// <summary>
        /// Maximum distance at which Agent can see anything with his rays.
        /// </summary>
        public float Range
        {
            get { return _range; }
            set { _range = value; }
        }

        /// <summary>
        /// Distance at which Agent will definitelly spot Entities even if they are right behind him.
        /// </summary>
        public float LookAroundRange
        {
            get { return _lookAroundRange; }
            set { _lookAroundRange = value; }
        }

        /// <summary>
        /// Number of rays used to spot Entities in any direction in small range.
        /// </summary>
        public int LookAroundResolution
        {
            get { return _lookAroundResolution; }
            set { _lookAroundResolution = value; }
        }

        /// <summary>
        /// Returns wheather the user wants to dubug the Perception configuration.
        /// </summary>
        public bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }
        
        /// <summary>
        /// Link to Perception's Memory component.
        /// </summary>
        public Memory Memory
        {
            get { return _memory; }
            set { _memory = value; }
        }

        #region CUSTOMIZATION
        [SerializeField] private float _offset = 90f;
        [SerializeField] private float _fieldOfView = 270f;
        [SerializeField] private int _resolution = 7;
        [SerializeField] private float _range = 10f;
        [Space(3)]
        [Tooltip("Distance at which Agent will definitelly spot Entities even if they are right behind him.")]
        [SerializeField] private float _lookAroundRange = 2f;
        [Tooltip("Number of rays used to spot Entities in any direction in small range.")]
        [SerializeField] private int _lookAroundResolution = 7;
        [Space(3)]
        [Tooltip("Position out of which rays are cast.")]
        [SerializeField] private Transform _origin = null;
        [Space(5)]
        [Tooltip("Cache for Agents' representation of the world.")]
        [SerializeField] private Memory _memory;
        [Space(10)]
        [Tooltip("Display the Perception configuration in editor?")]
        [SerializeField] private bool _debug = true;
        #endregion
        
        /// List of Entities that are spotted every Update.
        private List<Unit> _seenUnits;
        /// Last position in space where happened something that seemed suspitious to Agent.
        private Vector3 _suspiciousPosition;
        /// A link to actual controller.
        private Agent _agent;
        /// Condition Interfaces of this instance of Agent's Perception component.
        private List<PerceptionConditionInterface> _conditionInterfaces = new List<PerceptionConditionInterface>();

        /// <summary>
        /// Create new Perception component with specific configurations.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="fov"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static Perception CreatePerception(float distance = 10, float fov = 180, int resolution = 15)
        {
            return new Perception
            {
                _range = distance,
                _fieldOfView = fov,
                _resolution = resolution,
                _memory = new Memory()
            };
        }
        
        /// <summary>
        /// <para>Initialisation.</para>
        /// </summary>
        public void Init(Agent agent)
        {
            _seenUnits = new List<Unit>();
            _agent = agent;
            _origin = _agent.GetPerceptionOrigin();
            _memory.Init(agent);
            _suspiciousPosition = _agent.transform.position;
        }
        
        /// <summary>
        /// Get ragius of an unit if it is an Agent or 0 otherwise.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private static float Radius(Unit unit)
        {
            var agent = unit.GetComponent<Agent>();
            return agent ? agent.Radius : 0;
        }
        
        /// <summary>
        /// Get radius of current Agent's target.
        /// </summary>
        /// <returns></returns>
        public float Radius()
        {
            return Radius(TargetUnit);
        }
        
        /// <summary>
        /// Let Agent know that someithing is going on at cpecific position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="duration"></param>
        public void HearSomething(Vector3 position, float duration)
        {
            _suspiciousPosition = position;
            _agent.SetStatus(AgentStatus.HeardSomething, duration);
        }
        
        /// <summary>
        /// Build a ray starting at the origin and facing towards the target rotation.
        /// </summary>
        public static Vector3 InitRay(float r, float fi, float offset, Transform center)
        {
            var offsetRads = offset * Mathf.PI / 180;
            fi -= center.eulerAngles.y;
            var rads = fi * Mathf.PI / 180;
            rads = rads % (Mathf.PI * 2) + offsetRads % (Mathf.PI * 2);
            var x = center.position.x + r * Mathf.Cos(rads);
            var z = center.position.z + r * Mathf.Sin(rads);
            return new Vector3(x, center.position.y, z) - center.position;
        }

        /// <summary>
        /// Cast rays to understand what objects are around the Agent.
        /// </summary>
        /// <param name="seenUnits"></param>
        /// <param name="resolution"></param>
        /// <param name="fieldOfView"></param>
        /// <param name="range"></param>
        private void SeeIteration(ref List<Unit> seenUnits, int resolution, float fieldOfView, float range)
        {
            var n = resolution;
            var delta = fieldOfView / (n-1);
            var origin = Origin ?? _agent.transform;
            var vectors = new List<Vector3>();
			
            if (resolution % 2 != 0)
            {
                var rot = delta;
				
                vectors = new List<Vector3> {InitRay(range, 0, _offset, origin)};
                for (var i = 0; i < n / 2; i++)
                {
                    vectors.Add(InitRay(range, -rot, _offset, origin));
                    vectors.Add(InitRay(range, rot, _offset, origin));
                    rot += delta;
                }
            }
            else
            {
                for (var i = 0; i < resolution; i++)
                {
                    if (i % 2 == 0) continue;
                    vectors.Add(InitRay(range, -i * delta / 2, _offset, origin));
                    vectors.Add(InitRay(range, i * delta / 2, _offset, origin));
                }
            }

            //Add all QUnits in the FOV
            foreach (var vec in vectors)
            {
                RaycastHit hit;
                if (!Physics.Raycast(Origin.position, vec, out hit, range)) continue;
                var unit = hit.transform.gameObject.GetComponent<Unit>();
                if (unit && !seenUnits.Contains(unit)) seenUnits.Add(unit);
            }
        }
        
        /// <summary>
        /// Update what the Agent is seeing.
        /// </summary>
        public void Update()
        {
            //Refresh the list of seen units
            _seenUnits = new List<Unit>();

            //Update in range
            SeeIteration(ref _seenUnits, _resolution, _fieldOfView, _range);

            //Indisputably see
            if (_lookAroundRange <= 0 || _lookAroundResolution == 0) return;
            SeeIteration(ref _seenUnits, _lookAroundResolution, 360, _lookAroundRange);
            
            Memory.Update();
        }
        
        /// <summary>
        /// Search for entities with specified characteristics in FOV or in memory. 
        /// </summary>
        public bool SeeUnit(UnitQuery query, bool setAsTarget = true, bool remember = true)
        {
            //	Check if we actually see it
            if (_seenUnits.Count > 0)
                foreach (var unit in _seenUnits)
                    if (unit && query(unit))
                    {
                        TargetUnit = unit;
                        if(setAsTarget) _agent.Target = TargetUnit.transform;
                        if (remember) Memory.Memorise(unit);
                        return true;
                    }

            //	Otherwise check if we remember it
            if (!remember) return false;
            var unitEnemy = Memory.RememberUnit(query);
            if (unitEnemy == null)
                return false;
            
            TargetUnit = unitEnemy;
            _agent.Target = TargetUnit != null ? TargetUnit.transform : _agent.Motion.GetDefaultTarget();
            return true;
        }

        /// <summary>
        /// Search for entities with specified characteristics in FOV or in memory
        /// and select the one by user defined criteria.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="criterion"></param>
        /// <param name="chooseMax"></param>
        /// <param name="setAsTarget"></param>
        /// <param name="remember"></param>
        /// <returns></returns>
        public bool SelectUnitByCriteria(UnitQuery query, UnitCriterion criterion, bool chooseMax,
            bool setAsTarget = true, bool remember = true)
        {
            var units = new List<Unit>();
            //	Check if we actually see it
            if (_seenUnits.Count > 0)
            {
                foreach (var unit in _seenUnits)
                    if (unit && query(unit))
                        units.Add(unit);
            }
            else
            {
                if (remember)
                {
                    var rememberedUnits = Memory.RememberUnits(query);
                    foreach(var unit in rememberedUnits) units.Add(unit);
                }
            }

            if (units.Count == 0) return false;
            if (units.Count == 1)
            {
                TargetUnit = units[0];
                if(setAsTarget && TargetUnit) _agent.Target = TargetUnit.transform;
                if (remember) Memory.Memorise(units[0]);
                return true;
            }
            
            // Now select thhe one Unit that fits the criteria the best.
            var index = 0;
            var crit = criterion(units[0]);
            for (var i = 1; i < units.Count; i++)
            {
                if (chooseMax)
                {
                    if (!(criterion(units[i]) > crit)) continue;
                    index = i;
                    crit = criterion(units[i]);
                }
                else
                {
                    if (!(criterion(units[i]) < crit)) continue;
                    index = i;
                    crit = criterion(units[i]);
                }
            }
            
            TargetUnit = units[index];
            if(setAsTarget && TargetUnit) _agent.Target = TargetUnit.transform;
            if (remember) Memory.Memorise(units[index]);
            return true;
        }

        /// <summary>
        /// Return true if there are any obstacles between the Agent and its target.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="targetYOffset"></param>
        /// <returns></returns>
        public static bool ObstaclesBetweenMeAndTarget(Transform origin, Transform target, float targetYOffset = 0.5f)
        {
            var helper = new GameObject("__ObstaclesBetweenMeAndTarget__HELPER__");
            helper.transform.position = origin.transform.position;
            var newDir = Vector3.RotateTowards(origin.transform.forward, 
                target.position + new Vector3(0, targetYOffset, 0) - origin.transform.position, 100, 0);
            helper.transform.rotation = Quaternion.LookRotation(newDir);
            RaycastHit hit;
            Physics.Raycast(helper.transform.position, helper.transform.forward, out hit, Mathf.Infinity);
            GameObject.DestroyImmediate(helper);
            return hit.transform != target;
        }


        #region ADD_INTARFACES
        public PerceptionConditionInterface AddConditionInterface(string methodName)
        {
            return AgentFunctions.AddConditionInterface(methodName, ref _conditionInterfaces, _agent);
        }
        #endregion
    }
}
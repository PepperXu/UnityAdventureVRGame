using UnityEngine;

namespace Eliot.AgentComponents
{
    /// <summary>
    /// The Standard Library of general conditions.
    /// </summary>
    public class StandardGeneralConditionInterface : GeneralConditionInterface
    {
        public StandardGeneralConditionInterface(Agent agent) : base(agent)
        {
        }

        /// <summary>
        /// Check if Agent's status is Normal.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool StatusNormal()
        {
            return _agent.Status == AgentStatus.Normal;
        }

        /// <summary>
        /// Check if Agent's status is Alert.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool StatusAlert()
        {
            return _agent.Status == AgentStatus.Alert;
        }

        /// <summary>
        /// Check if Agent's status is Danger.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool StatusDanger()
        {
            return _agent.Status == AgentStatus.Danger;
        }

        /// <summary>
        /// Check if Agent's status is BeingAimedAt.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool StatusBeingAimedAt()
        {
            return _agent.Status == AgentStatus.BeingAimedAt;
        }

        /// <summary>
        /// Check if distance between Agent and his WaypointsGroup origin is bigger than the specified value.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool FarFromHome()
        {
            return Vector3.Distance(_agent.transform.position,
                       _agent.Waypoints ?
                           _agent.Waypoints.transform.position : _agent.GeneralSettings.InitialPosition)
                   >= _agent.GeneralSettings.FarFromHome;
        }

        /// <summary>
        /// Check if distance between Agent and his WaypointsGroup origin is smaller than the specified value.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool AtHome()
        {
            return _agent.Waypoints ? _agent.Waypoints.IsInsidePolygon(_agent.transform.position) :
                Vector3.Distance(_agent.transform.position, _agent.GeneralSettings.InitialPosition) <= _agent.GeneralSettings.AtHomeRange;
        }

        /// <summary>
        /// Check if Agent is casting a Skill at the moment.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour] public bool SkillReadyToUse() { return _agent.CurrentSkill; }

    }
}

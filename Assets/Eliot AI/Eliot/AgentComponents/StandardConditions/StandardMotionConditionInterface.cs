namespace Eliot.AgentComponents
{
    /// <summary>
    /// The Standard Library of motion related conditions.
    /// </summary>
    public class StandardMotionConditionInterface : MotionConditionInterface
    {
        public StandardMotionConditionInterface(Agent agent) : base(agent)
        {
        }

        #region MODEL INTERFACE

        /// <summary>
        /// Check wheather the current Agent's motion state is Idling.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool Idling()
        {
            return _motion.State == MotionState.Idling;
        }

        /// <summary>
        /// Check wheather the current Agent's motion state is Standing.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool Standing()
        {
            return _motion.State == MotionState.Standing;
        }

        /// <summary>
        /// Check wheather the current Agent's motion state is Walking.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool Walking()
        {
            return _motion.State == MotionState.Walking;
        }

        /// <summary>
        /// Check wheather the current Agent's motion state is WalkingAway.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool WalkingAway()
        {
            return _motion.State == MotionState.WalkingAway;
        }

        /// <summary>
        /// Check wheather the current Agent's motion state is Running.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool Running()
        {
            return _motion.State == MotionState.Running;
        }

        /// <summary>
        /// Check wheather the current Agent's motion state is RunningAway.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool RunningAway()
        {
            return _motion.State == MotionState.RunningAway;
        }

        /// <summary>
        /// Check wheather the current Agent's motion state is Dodging.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool Dodging()
        {
            return _motion.State == MotionState.Dodging;
        }

        #endregion
    }
}

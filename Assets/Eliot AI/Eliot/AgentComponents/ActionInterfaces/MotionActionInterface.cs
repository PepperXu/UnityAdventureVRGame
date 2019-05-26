namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates Agent's Motion actions by calling its API
	/// and giving names to these groups of calls. 
	/// </summary>
    public class MotionActionInterface : ActionInterface
	{
        // Agent's Motion component.
		protected readonly Motion _motionComponent;
        protected readonly IMotionEngine _motion;
        /// Link to the Agent.
        protected readonly Agent _agent;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="agent"></param>
		public MotionActionInterface(Agent agent)
		{
			_agent = agent;
			_motionComponent = agent.Motion;
			_motion = _motionComponent.Engine;
		}
	}
}
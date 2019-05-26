#pragma warning disable CS0414

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates the calls to Agent's Motion API in order
	/// to check any user-defined conditions related to it.
	/// </summary>
	public class MotionConditionInterface : ConditionInterface
	{

		/// Agent's Motion component.
		protected readonly Motion _motion;
        /// Link to the Agent.
        protected readonly Agent _agent;

		/// <summary>
		/// Initialize the interface.
		/// </summary>
		/// <param name="agent"></param>
		public MotionConditionInterface(Agent agent)
		{
			_agent = agent;
			_motion = agent.Motion;
		}
	}
}
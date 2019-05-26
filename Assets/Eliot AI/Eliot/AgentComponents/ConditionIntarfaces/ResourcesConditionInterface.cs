namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates the calls to Agent's Resources API in order
	/// to check any user-defined conditions related to it.
	/// </summary>
	public class ResourcesConditionInterface : ConditionInterface
	{
		/// Agent's Resources component.
		protected readonly Resources _resources;
        /// Link to the Agent.
        protected readonly Agent _agent;

		/// <summary>
		/// Initialize the interface.
		/// </summary>
		/// <param name="agent"></param>
		public ResourcesConditionInterface(Agent agent)
		{
			_agent = agent;
			_resources = agent.Resources;
		}
	}
}
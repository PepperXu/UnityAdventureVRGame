namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates calls Agent's general settings API
	/// and giving names to these groups of calls. 
	/// </summary>
	public class GeneralConditionInterface : ConditionInterface
	{
		/// Link to the Agent.
		protected readonly Agent _agent;

		/// <summary>
		/// Initialize the interface.
		/// </summary>
		/// <param name="agent"></param>
		public GeneralConditionInterface(Agent agent)
		{
			_agent = agent;
		}
	}
}
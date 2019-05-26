using Eliot.Environment;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Query units checking for wanted characteristics.
	/// </summary>
	/// <param name="unit"></param>
	public delegate bool UnitQuery(Unit unit);
	
	/// <summary>
	/// Query units looking for the quantity of wanted criterion.
	/// </summary>
	/// <param name="unit"></param>
	public delegate float UnitCriterion(Unit unit);
	
	
	/// <summary>
	/// Incapsulates the calls to Agent's Perception API in order
	/// to check any user-defined conditions related to it.
	/// </summary>
	public class PerceptionConditionInterface : ConditionInterface
	{
		/// Agent's Perception component.
		protected readonly Perception _perception;
        /// Link to the Agent.
        protected readonly Agent _agent;
		
		/// <summary>
		/// Initialize the interface.
		/// </summary>
		/// <param name="agent"></param>
		public PerceptionConditionInterface(Agent agent)
		{
			_agent = agent;
			_perception = agent.Perception;
		}		
	}
}
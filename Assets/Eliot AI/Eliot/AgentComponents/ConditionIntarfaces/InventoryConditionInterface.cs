#pragma warning disable CS0414

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates the calls to Agent's Inventory API in order
	/// to check any user-defined conditions related to it.
	/// </summary>
	public class InventoryConditionInterface : ConditionInterface
	{
		/// Agent's Inventory component.
		protected readonly Inventory _inventory;
        /// Link to the Agent.
        protected readonly Agent _agent;

		/// <summary>
		/// Initialize the interface.
		/// </summary>
		/// <param name="agent"></param>
		public InventoryConditionInterface(Agent agent)
		{
			_agent = agent;
			_inventory = agent.Inventory;
		}
	}
}
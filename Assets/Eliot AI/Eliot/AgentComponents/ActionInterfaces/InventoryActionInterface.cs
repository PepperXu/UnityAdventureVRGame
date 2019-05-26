namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates Agent's Inventory actions by calling its API
	/// and giving names to these groups of calls. 
	/// </summary>
    public class InventoryActionInterface : ActionInterface
	{
        /// Agent's Inventory component.
        protected readonly Inventory _inventory;
        /// Link to the Agent.
        protected readonly Agent _agent;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="agent"></param>
		public InventoryActionInterface(Agent agent)
		{
            _agent = agent;
			_inventory = agent.Inventory;
		}
	}
}
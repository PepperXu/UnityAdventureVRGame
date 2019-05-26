using Eliot.Environment;
using UnityEngine;

namespace Eliot.AgentComponents
{
    /// <summary>
    /// The Standard Library of inventory related actions.
    /// </summary>
    public class StandardInventoryActionInterface : InventoryActionInterface
    {
        public StandardInventoryActionInterface(Agent agent) : base(agent)
        {
        }

        /// <summary>
        /// Pick all the items that are close to the Agent.
        /// </summary>
        [IncludeInBehaviour]
        public void PickItems()
        {
            var allUnits = _agent.Perception.SeenUnits;
            foreach (var unit in _agent.Perception.Memory.Units)
                allUnits.Add(unit);
            foreach (var item in allUnits)
            {
                if (item.GetComponent<Item>()
                    && Vector3.Distance(item.transform.position, _agent.transform.position)
                    <= _agent.GeneralSettings.CloseDistance)
                {
                    item.GetComponent<Item>().AddToInventory(_inventory);
                    _agent.Perception.Memory.Forget(item);
                }
            }
        }

        /// <summary>
        /// Find the best weapon in Inventory and wield it.
        /// </summary>
        [IncludeInBehaviour] public void WieldBestWeapon() { _inventory.BestWeapon().Wield(_agent); }

        /// <summary>
        /// Find the worst item in Inventory and drop it.
        /// </summary>
        [IncludeInBehaviour] public void DropWorstItem() { _inventory.DropWorstItem(); }

        /// <summary>
        /// Try to find a healing potion in the Inventory and use it.
        /// </summary>
        [IncludeInBehaviour]
        public void UseBestHealingPotion()
        {
            Item bestPotion = null;
            for (var i = 0; i < _inventory.Items.Count; i++)
            {
                var item = _inventory.Items[i];
                if (item.Type == ItemType.Potion && item.Skill && item.Skill.AddsHealth && item.Value > (bestPotion == null ? -10000 : bestPotion.Value))
                    bestPotion = item;
            }
            if (bestPotion != null)
                bestPotion.Use(_agent);
        }

        /// <summary>
        /// Try to find an energy potion in the Inventory and use it.
        /// </summary>
        [IncludeInBehaviour]
        public void UseBestEnergyPotion()
        {
            Item bestPotion = null;
            for (var i = 0; i < _inventory.Items.Count; i++)
            {
                var item = _inventory.Items[i];
                if (item.Type == ItemType.Potion && item.Skill && item.Skill.AddsEnergy && item.Value > (bestPotion == null ? -10000 : bestPotion.Value))
                    bestPotion = item;
            }
            if (bestPotion != null)
                bestPotion.Use(_agent);
        }
    }
}

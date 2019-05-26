using Eliot.Environment;

namespace Eliot.AgentComponents
{
    /// <summary>
    /// The Standard Library of inventory related conditions.
    /// </summary>
    public class StandardInventoryConditionInterface : InventoryConditionInterface
    {
        public StandardInventoryConditionInterface(Agent agent) : base(agent)
        {
        }

        /// <summary>
        /// Check wheather the Agent has a better weapon in his inventory than the currently wielded one.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HaveBetterWeapon()
        {
            return _inventory.HaveBetterItem(ItemType.Weapon);
        }

        /// <summary>
        /// Check whether the Agent has any healing potion in its Inventory.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HaveHealingPotion()
        {
            foreach (var item in _inventory.Items)
                if (item.Type == ItemType.Potion && item.Skill && item.Skill.AddsHealth)
                    return true;
            return false;
        }

        /// <summary>
        /// Check whether the Agent has any energy replenishing potion in its Inventory.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HaveEnergyPotion()
        {
            foreach (var item in _inventory.Items)
                if (item.Type == ItemType.Potion && item.Skill && item.Skill.AddsEnergy)
                    return true;
            return false;
        }
    }
}

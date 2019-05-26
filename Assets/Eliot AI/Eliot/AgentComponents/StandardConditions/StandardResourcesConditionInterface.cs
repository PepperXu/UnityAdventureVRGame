namespace Eliot.AgentComponents
{
    /// <summary>
    /// The Standard Library of resources related conditions.
    /// </summary>
    public class StandardResourcesConditionInterface : ResourcesConditionInterface
    {
        public StandardResourcesConditionInterface(Agent agent) : base(agent) 
        {
        }

        /// <summary>
        /// Return true if Agent's health is at its maximum value.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HealthFull()
        { return _resources.HealthFull(); }

        /// <summary>
        /// Return true if Agent's health is equal or lower some user-defined value.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HealthLow()
        { return _resources.HealthPoints <= _agent.GeneralSettings.LowHealth; }

        /// <summary>
        /// Return true if Agent's health is equal or lower than two thirds of its maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HealthTwoThirds()
        { return _resources.HealthPoints <= _resources.MaxHealthPoints * 2 / 3; }

        /// <summary>
        /// Return true if Agent's health is equal or lower than half of its maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HealthHalf()
        { return _resources.HealthPoints <= _resources.MaxHealthPoints / 2; }

        /// <summary>
        /// Return true if Agent's health is equal or lower than one third of its maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HealthOneThird()
        { return _resources.HealthPoints <= _resources.MaxHealthPoints / 3; }

        /// <summary>
        /// Return true if Agent's energy is at its maximum value.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnergyFull()
        { return _resources.EnergyFull(); }

        /// <summary>
        /// Return true if Agent's energy is equal or lower some user-defined value.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnergyLow()
        { return _resources.EnergyPoints <= _agent.GeneralSettings.LowEnergy; }

        /// <summary>
        /// Return true if Agent's energy is equal or lower than two thirds of its maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnergyTwoThirds()
        { return _resources.EnergyPoints <= _resources.MaxEnergyPoints * 2 / 3; }

        /// <summary>
        /// Return true if Agent's energy is equal or lower than half of its maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnergyHalf()
        { return _resources.EnergyPoints <= _resources.MaxEnergyPoints / 2; }

        /// <summary>
        /// Return true if Agent's energy is equal or lower than one third of its maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnergyOneThird()
        { return _resources.EnergyPoints <= _resources.MaxEnergyPoints / 3; }

        /// <summary>
        /// Return true if Agent's energy is equal or lower than one third of its maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnoughEnergyForSkill()
        { return _resources.EnergyPoints >= (_agent.CurrentSkill ? _agent.CurrentSkill.EnergyCost : 0); }
    }
}

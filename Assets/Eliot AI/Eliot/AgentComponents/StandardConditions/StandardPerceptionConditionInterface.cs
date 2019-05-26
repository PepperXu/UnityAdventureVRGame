using System;
using Eliot.Environment;
using UnityEngine;

namespace Eliot.AgentComponents
{
    /// <summary>
    /// The Standard Library of perception related conditions.
    /// </summary>
    public class StandardPerceptionConditionInterface : PerceptionConditionInterface
    {
        public StandardPerceptionConditionInterface(Agent agent) : base(agent)
        {
        }

        /// <para>
        /// Define here names of queries which agent will run when requested by behaviour engine.
        /// Queries can check any public members of an object that is being checked.
        /// Agent's detection recognises all Units with colliders in the defined range.
        /// </para>

        /// <summary>
        /// Return true if Agent can see an enemy or has seen one recently.
        /// </summary>
        /// /// <returns></returns>
        [IncludeInBehaviour]
        public bool SeeEnemy()
        { return _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Agent && !_agent.Unit.IsFriend(unit)); }

        /// <summary>
        /// Return true if Agent can see a friend or has seen one recently.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool SeeFriend()
        { return _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Agent && _agent.Unit.IsFriend(unit)); }

        /// <summary>
        /// Return true if Agent can see a block or has seen one recently.
        /// </summary>
        [IncludeInBehaviour]
        public bool SeeBlock()
        { return _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Block); }

        /// <summary>
        /// Return true if Agent can see a corpse or has seen one recently.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool SeeCorpse()
        { return _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Corpse); }

        /// <summary>
        /// Return true if Agent can see an enemy that is about to attack him.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnemyIsLoadingAttack()
        {
            return _agent.Status == AgentStatus.BeingAimedAt
                   || _perception.SeeUnit(unit => unit && unit.Type == Environment.UnitType.Agent && unit.Team != _agent.Unit.Team
                                           && unit.GetComponent<Agent>() && unit.GetComponent<Agent>().CurrentSkill
                                             && unit.GetComponent<Agent>().CurrentSkill.State == SkillState.Loading);
        }

        /// <summary>
        /// Return true if Agent's current target is in front of him with an error
        /// in mesurement defined by Agent's general settings.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetIsInFrontOfMe()
        {
            return Vector3.Angle(_agent.Target ?
                           _agent.Target.position - _agent.transform.position :
                           _agent.transform.forward, _agent.transform.forward) <= _agent.GeneralSettings.AimFieldOfView;
        }

        /// <summary>
        /// Return true if Agent's target is in front of him and is turned with its back to the Agent.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool SeeTargetsBack()
        {
            if (!_agent.Target) return false;
            return TargetIsInFrontOfMe() &&
                   Vector3.Angle(_agent.Target.forward, _agent.transform.forward) <= _agent.GeneralSettings.BackFieldOfView;
        }

        /// <summary>
        /// Return true if Agent's current target is at the distance that is defined
        /// by user as Close Distance or closer.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetInCloseRange()
        {
            var dist = Vector3.Distance(_agent.transform.position, _agent.Target.position)
                       - (_agent.Target.GetComponent<Agent>() ? _perception.Radius() : 0);
            return dist <= _agent.GeneralSettings.CloseDistance;
        }

        /// <summary>
        /// Return true if Agent's current target is at the distance that is defined
        /// by user as Mid Distance or closer.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetInMidRange()
        {
            var dist = Vector3.Distance(_agent.transform.position, _agent.Target.position)
                       - (_agent.Target.GetComponent<Agent>() ? _perception.Radius() : 0);
            return dist <= _agent.GeneralSettings.MidDistance;
        }

        /// <summary>
        /// Return true if Agent's current target is at the distance that is defined by user as Far Distance or closer.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetInFarRange()
        {
            var dist = Vector3.Distance(_agent.transform.position, _agent.Target.position)
                       - (_agent.Target.GetComponent<Agent>() ? _perception.Radius() : 0);
            return dist <= _agent.GeneralSettings.FarDistance;
        }

        /// <summary>
        /// Return true if Agent's current target is at the distance that is defined by user as Far Distance or closer.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetInSkillRange()
        {
            var dist = Vector3.Distance(_agent.transform.position, _agent.Target.position) - _perception.Radius();
            return dist <= (_agent.CurrentSkill ? _agent.CurrentSkill.Range : _agent.GeneralSettings.FarDistance);
        }

        /// <summary>
        /// Return true if Agent can see a Unit with low health.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetHasLowHealth()
        {
            return _perception.SeeUnit(unit => unit.GetComponent<Agent>().Resources.HealthPoints
                                      <= unit.GetComponent<Agent>().GeneralSettings.LowHealth);
        }

        /// <summary>
        /// Return true if Agent can see a Unit with maximum health.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetHasFullHealth()
        { return _perception.SeeUnit(unit => unit.GetComponent<Agent>().Resources.HealthFull()); }

        /// <summary>
        /// Return true if Agent can see a Unit with health lower than maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetNotFullyHealthy()
        { return _perception.SeeUnit(unit => !unit.GetComponent<Agent>().Resources.HealthFull()); }

        /// <summary>
        /// Return true if Agent can see a friend Agent with health lower than maximum.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool FriendNotFullyHealthy()
        {
            return _perception.SeeUnit(unit => unit && unit.Type == Environment.UnitType.Agent
                                         && unit.Team == _agent.Unit.Team
                                         && unit.GetComponent<Agent>() && !unit.GetComponent<Agent>().Resources.HealthFull());
        }

        /// <summary>
        /// Return true if Agent can see enemy Agent who has the first one as a target.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnemyIsAimingAtMe()
        {
            return _perception.SeeUnit(unit => unit && unit.Type == Environment.UnitType.Agent && unit.Team != _agent.Unit.Team
                                         && unit.GetComponent<Agent>() && unit.GetComponent<Agent>().Target == _agent.transform);
        }

        /// <summary>
        /// Return true if there are any objects between Agent and its target.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool ObstaclesBeforeTarget()
        { return Perception.ObstaclesBetweenMeAndTarget(_agent.Perception.Origin, _agent.Target); }

        /// <summary>
        /// Return true if Agent's current target is an enemy and is trying to run.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool EnemyIsFleeing()
        {
            return _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Agent && unit.Team != _agent.Unit.Team
                                         && unit.GetComponent<Agent>()
                                         && (unit.GetComponent<Agent>().Motion.State == MotionState.Running
                                             || unit.GetComponent<Agent>().Motion.State == MotionState.Walking));
        }

        /// <summary>
        /// Return true if the Agent can see an enemy. If there are multiple enemies
        /// in the field of view, set the one with lowest health as a target.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetWeakestEnemy()
        {
            try
            {
                var flag = _perception.SelectUnitByCriteria
                (
                    query: unit => unit && unit.GetComponent<Agent>() && unit.Type == Environment.UnitType.Agent &&
                                   unit.Team != _agent.Unit.Team,
                    criterion: unit => unit.GetComponent<Agent>().Resources.HealthPoints,
                    chooseMax: false
                );
                return !flag ? _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Agent && unit.Team != _agent.Unit.Team) : flag;
            }
            catch (Exception)
            {
                return _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Agent && unit.Team != _agent.Unit.Team);
            }
        }

        /// <summary>
        /// Return true if the Agent can see an enemy. If there are multiple enemies
        /// in the field of view, set the one that is closest to the Agent.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetClosestEnemy()
        {
            try
            {
                var flag = _perception.SelectUnitByCriteria
                (
                    query: unit => unit && unit.GetComponent<Agent>() && unit.Type == Environment.UnitType.Agent &&
                                   unit.Team != _agent.Unit.Team,
                    criterion: unit => Vector3.Distance(unit.transform.position, _agent.transform.position),
                    chooseMax: false
                );
                return !flag ? _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Agent && unit.Team != _agent.Unit.Team) : flag;
            }
            catch (Exception)
            {
                return _perception.SeeUnit(unit => unit.Type == Environment.UnitType.Agent && unit.Team != _agent.Unit.Team);
            }
        }

        /// <summary>
        /// Return true if the Agent can see a weapon that is better than the currently wielded one.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool SeeBetterWeapon()
        {
            return _perception.SeeUnit(unit => unit && unit.Type == Environment.UnitType.Item
                                              && unit.GetComponent<Item>().Type == ItemType.Weapon
                                              && _agent.Inventory.ItemIsBetterThanCurrent(unit.GetComponent<Item>()));
        }

        /// <summary>
        /// Return true if the Agent heared something unusual and set the source of the sounds as a target.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool HeardSomething()
        {
            if (_agent.Status == AgentStatus.HeardSomething)
            {
                _agent.Target = _agent.Motion.GetDefaultTarget(_agent.Perception.SuspiciousPosition);
                if (Vector3.Distance(_agent.transform.position, _agent.Target.position) <= _agent.GeneralSettings.CloseDistance)
                {
                    _agent.Status = AgentStatus.Normal;
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check whether the Agent's target is just a dummy helper object or Agent itself.
        /// </summary>
        /// <returns></returns>
        [IncludeInBehaviour]
        public bool TargetIsFake()
        {
            return _agent.Target.GetComponent<Waypoint>() || (_agent.Target.gameObject == _agent.gameObject);
        }

        /// <summary>
        /// Return true if Agent can see any Item that is a potion and has healing properties.
        /// </summary>
        /// /// <returns></returns>
        [IncludeInBehaviour]
        public bool SeeHealingPotion()
        {
            return _perception.SeeUnit(unit => unit && unit.Type == Environment.UnitType.Item
                                                    && unit.GetComponent<Item>().Type == ItemType.Potion
                                                    && unit.GetComponent<Item>().Skill
                                                    && unit.GetComponent<Item>().Skill.AddsHealth);
        }

        /// <summary>
        /// Return true if Agent can see any Item that is a potion and has energy replenishing properties.
        /// </summary>
        /// /// <returns></returns>
        [IncludeInBehaviour]
        public bool SeeEnergyPotion()
        {
            return _perception.SeeUnit(unit => unit && unit.Type == Environment.UnitType.Item
                                                    && unit.GetComponent<Item>().Type == ItemType.Potion
                                                    && unit.GetComponent<Item>().Skill
                                                    && unit.GetComponent<Item>().Skill.AddsEnergy);
        }
    }
}

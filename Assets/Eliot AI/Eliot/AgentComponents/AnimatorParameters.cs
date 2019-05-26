using System;
using UnityEngine;

namespace Eliot.AgentComponents
{
    /// <summary>
    /// Set the names of parameters that are used to control this Agent's Animator Controller.
    /// </summary>
    [Serializable] public class AnimatorParameters
    {
        [Tooltip("The name of an Animator parameter on which the motion animations depend.")]
        public string Speed = "Speed";
        [Tooltip("The name of an Animator parameter on which the turning animations depend.")]
        public string Turn = "Turn";
        [Tooltip("The name of an Animator parameter which triggers the animation of dodging.")]
        public string DodgeTrigger = "Dodge";
        [Tooltip("The name of an Animator parameter which triggers the animation of taking damage.")]
        public string TakeDamageTrigger = "TakeDamage";
        [Tooltip("The name of an Animator parameter which triggers the animation of loading a skill.")]
        public string LoadSkillTrigger = "LoadSkill";
        [Tooltip("The name of an Animator parameter which triggers the animation of using a skill.")]
        public string UseSkillTrigger = "UseSkill";
    }
}
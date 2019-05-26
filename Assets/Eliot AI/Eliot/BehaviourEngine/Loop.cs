using System.Collections.Generic;

namespace Eliot.BehaviourEngine
{
    /// <summary>
    /// Loop is responsible for checking the specific condition
    /// and take appropriate action as far as the result goes.
    /// While the result of checking its condition is true, Loop
    /// substitutes Entry in the behaviour model.
    /// </summary>
    public class Loop : EliotComponent
    {
        /// Hold the boolean method and target instance to check the condition whenever needed.
        private readonly EliotCondition _condition;
        /// Links to other components that get activated when the result of checking condition is true.
        private readonly List<Transition> _transitionsWhile = new List<Transition>();
        /// Links to other components that get activated when the result of checking condition is false.
        private readonly List<Transition> _transitionsEnd = new List<Transition>();
        /// Link to the core, whose ActiveComponent can be substituted by this Loop.
        private readonly BehaviourCore _core;
        /// If true, action taken upon checking the condition will be reversed.
        private readonly bool _reverse;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Loop(){}
        
        /// <summary>
        /// Initialize the component.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="reverse"></param>
        /// <param name="qc"></param>
        /// <param name="id"></param>
        public Loop(BehaviourCore core, bool reverse, EliotCondition qc, string id = "L") : base(id)
        {
            _core = core;
            _reverse = reverse;
            if (qc != null)
                _condition = qc;
        }

        /// <summary>
        /// Make Loop do its job.
        /// </summary>
        public override void Update()
        {
            if (_condition == null) return;
			
            switch (_reverse ? !_condition() : _condition())
            {
                case true:
                {
                    if (_core.ActiveComponent != this) _core.ActiveComponent = this;
                    if (_transitionsWhile.Count == 0) return;
                    foreach(var transition in _transitionsWhile)
                        if (!transition.Update()) break;
                    break;
                }
                case false:
                {
                    if (_core.ActiveComponent == this) _core.ActiveComponent = _core.Entry;
                    if (_transitionsEnd.Count == 0) return;
                    foreach(var transition in _transitionsEnd)
                        if (!transition.Update()) break;
                    break;
                }
            }
        }

        /// <summary>
        /// Create transition in 'While' group between Loop and other component.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="minRate"></param>
        /// <param name="maxRate"></param>
        /// <param name="minCooldown"></param>
        /// <param name="maxCooldown"></param>
        /// <param name="terminate"></param>
        public override void ConnectWith(EliotComponent p, int minRate, int maxRate,
            float minCooldown, float maxCooldown, bool terminate)
        {
            if (!p) return;
            _transitionsWhile.Add(new Transition(p, minRate, maxRate, minCooldown, maxCooldown, terminate));
        }
        
        /// <summary>
        /// Create transition in 'End' group between Loop and other component.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="minRate"></param>
        /// <param name="maxRate"></param>
        /// <param name="minCooldown"></param>
        /// <param name="maxCooldown"></param>
        /// <param name="terminate"></param>
        public void ConnectWith_End(EliotComponent p, int minRate, int maxRate,
            float minCooldown, float maxCooldown, bool terminate)
        {
            if (!p) return;
            _transitionsEnd.Add(new Transition(p, minRate, maxRate, minCooldown, maxCooldown, terminate));
        }
    }
}

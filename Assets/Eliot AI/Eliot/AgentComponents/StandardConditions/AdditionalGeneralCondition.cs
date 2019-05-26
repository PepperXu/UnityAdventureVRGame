using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Eliot.AgentComponents
{
    public class AdditionalGeneralCondition : GeneralConditionInterface
    {
        private float _alertCap = 30f;

        public AdditionalGeneralCondition(Agent agent) : base(agent)
        {

        }
        
        [IncludeInBehaviour]
        public bool AlertPointMax()
        {
            return _agent.Resources.AlertPoint >= _alertCap;
        }

        [IncludeInBehaviour]
        public bool AlertPointMin()
        {
            return _agent.Resources.AlertPoint <= 0;
        }
    }
}

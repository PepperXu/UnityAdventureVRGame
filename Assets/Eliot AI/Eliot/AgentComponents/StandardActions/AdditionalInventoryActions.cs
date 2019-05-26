using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eliot.AgentComponents
{
    public class AdditionalInventoryActions : InventoryActionInterface
    {
        private float duration = 10f;

        public AdditionalInventoryActions(Agent agent) : base(agent)
        { 
        
        }

        [IncludeInBehaviour]
        public void IncreaseAlertPoint()
        {
            _agent.Resources.AlertPoint += 1f;
        }

        [IncludeInBehaviour]
        public void ResetAlertPoint()
        {
            _agent.Resources.AlertPoint = 0f;
        }

        [IncludeInBehaviour]
        public void SetStatusAlert()
        {
            _agent.SetStatus(AgentStatus.Alert, duration);
        }

        [IncludeInBehaviour]
        public void SetStatusDanger()
        {
            _agent.SetStatus(AgentStatus.Danger, duration);
        }
    }
}

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
using UnityEngine;
using Opsive.UltimateCharacterController.Events;

namespace Eliot.Utility
{
    /// <summary>
    /// Lets a UCC character controller damage Eliot agents.
    /// </summary>
    public class IntegrationUccHitscan : MonoBehaviour
    {
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public void Awake()
        {
            EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(gameObject, "OnObjectImpact", OnImpact);
        }

        /// <summary>
        /// The object has been impacted with another object.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="forceDirection">The direction that the object took damage from.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        private void OnImpact(float amount, Vector3 position, Vector3 forceDirection, GameObject attacker, Collider hitCollider)
        {
            Debug.Log(name + " impacted by " + attacker + " on collider " + hitCollider + ".");
            if (hitCollider.gameObject.GetComponent<Eliot.AgentComponents.Agent>())
            {
                hitCollider.gameObject.GetComponent<Eliot.AgentComponents.Agent>().Damage(Mathf.RoundToInt(amount));
            }
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public void OnDestroy()
        {
            EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(gameObject, "OnObjectImpact", null);
        }
    }
}
#endif
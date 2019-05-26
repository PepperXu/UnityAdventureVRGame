using UnityEngine;

namespace Eliot.Utility
{
	/// <summary>
	/// Handles passing the damage info from Eliot Skills to an arbitrary third-party asset.
	/// </summary>
	public static class IntegrationDamageHandler
	{
		/// <summary>
		/// Check if a target has necessary component and pass the damage info.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="power"></param>
		/// <param name="attacker"></param>
		/// <param name="pushPower"></param>
		public static void PassDamage(GameObject target, float power, GameObject attacker, float pushPower)
		{
#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
			if (target.GetComponent<Opsive.UltimateCharacterController.Traits.CharacterHealth>())
				PassDamage_Opsive_UCC(target, power, attacker, pushPower);
#endif
			
#if INVECTOR_SHOOTER
			if (target.GetComponent<Invector.vCharacterController.vCharacter>())
				PassDamage_Invector_Health(target, power, attacker);
#endif

#if GKC
			if (target.GetComponent<health>())
				PassDamage_TwoCubes_GKC(target, power, attacker);
#endif

#if UFPS
			if (target.GetComponent<vp_DamageTransfer>())
				PassDamage_UFPS(target, power, attacker);
#endif
			
#if OOTII_TPC
			if (target.GetComponent<com.ootii.Actors.LifeCores.ActorCore>())
				PassDamage_Ootii_TPC(target, power, attacker);
#endif
		}

#if ULTIMATE_CHARACTER_CONTROLLER_SHOOTER
		/// <summary>
		/// Pass damage to an Opsive UCC character. Works for either First and Thirt person controller.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="power"></param>
		/// <param name="attacker"></param>
		/// <param name="pushPower"></param>
		private static void PassDamage_Opsive_UCC(GameObject target, float power, GameObject attacker, float pushPower)
		{
			var position = attacker.transform.position;
			var direction = attacker.transform.position - target.transform.position;
			target.GetComponent<Opsive.UltimateCharacterController.Traits.CharacterHealth>().Damage(power, position, direction, pushPower, attacker);
		}
#endif
		
#if INVECTOR_SHOOTER
		/// <summary>
		/// Pass damage to an Invector character controller.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="power"></param>
		/// <param name="attacker"></param>
		private static void PassDamage_Invector_Health(GameObject target, float power, GameObject attacker)
		{
			var damage = new Invector.vDamage
			{
				damageValue = Mathf.RoundToInt(power),
				sender = attacker.transform,
				receiver = target.transform,
			};
			target.GetComponent<Invector.vCharacterController.vCharacter>().TakeDamage( damage );
		}
#endif

#if GKC
		/// <summary>
		/// Pass damage to a GKC character controller.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="power"></param>
		/// <param name="attacker"></param>
		private static void PassDamage_TwoCubes_GKC(GameObject target, float power, GameObject attacker)
		{
			var position = attacker.transform.position;
			var direction = attacker.transform.position - target.transform.position;
			target.GetComponent<health>().setDamage(power, direction, position, attacker, null, true, true);
		}
#endif

#if UFPS
		/// <summary>
		/// Pass damage to an original UFPS character controller.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="power"></param>
		/// <param name="attacker"></param>
		private static void PassDamage_UFPS(GameObject target, float power, GameObject attacker)
		{
			target.GetComponent<vp_DamageTransfer>().Damage(new vp_DamageInfo(power, attacker.transform));
		}
#endif
		
#if OOTII_TPC
		/// <summary>
		/// Pass damage to the Ootii Third Person Controller.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="power"></param>
		/// <param name="attacker"></param>
		private static void PassDamage_Ootii_TPC(GameObject target, float power, GameObject attacker)
		{
			var combatMessage = com.ootii.Actors.Combat.CombatMessage.Allocate();
			combatMessage.ID = com.ootii.Actors.Combat.CombatMessage.MSG_DEFENDER_ATTACKED;
			combatMessage.Damage = power;
			combatMessage.Attacker = attacker;
			combatMessage.Defender = target;

			var actorCore = target.GetComponent<com.ootii.Actors.LifeCores.ActorCore>();

			if (actorCore)
				actorCore.SendMessage(combatMessage);

			com.ootii.Actors.Combat.CombatMessage.Release(combatMessage);
		}
#endif
	}
}
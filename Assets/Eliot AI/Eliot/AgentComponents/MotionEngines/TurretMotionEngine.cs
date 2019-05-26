using UnityEngine;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Motion Engine that 
	/// </summary>
	public class TurretMotionEngine : IMotionEngine
	{
		#region FIELDS
		/// Link to Agent's Motion component.
		private Motion _motion;
		/// If true, Agent is not allowed to move in any way.
		private bool _locked;
		/// Turret's moving part.
		private Transform _head;
		/// Allowed error for cannon to switch direction of rotation while idling.
		private const float ClampedRotError = 1f;
		#endregion

		#region INTERFACE METHODS IMPLEMENTATION
		/// <summary>
		/// Return the position of current Agent's target.
		/// </summary>
		/// <returns></returns>
		public Vector3 GetTarget()
		{
			return _motion.TargetTransform.position;
		}

		/// <summary>
		/// Assign a new target position.
		/// </summary>
		/// <param name="pos"></param>
		public void SetTarget(Vector3 pos)
		{
			_motion.TargetTransform.position = pos;
		}

		/// <summary>
		/// Initialize an Agent so that it has all necessary parameters for using the engine.
		/// </summary>
		/// <param name="agent"></param>
		public void Init(Agent agent)
		{
			_motion = agent.Motion;
			_head = _motion.Head;
		}

		/// <summary>
		/// Check if the Agent's motion is currently locked.
		/// </summary>
		/// <returns></returns>
		public bool Locked()
		{
			return _locked;
		}

		/// <summary>
		/// Lock the Agent's Motion component.
		/// </summary>
		public void Lock()
		{
			_locked = true;
		}

		/// <summary>
		/// Unlock the Agent's Motion component.
		/// </summary>
		public void Unlock()
		{
			_locked = false;
		}

		/// <summary>
		/// Rotate Agent towards its target.
		/// </summary>
		/// <param name="targetPosition"></param>
		public void LookAtTarget(Vector3 targetPosition)
		{
			if (!_head) return; 
			var newDir = Vector3.RotateTowards(_head.forward, 
				targetPosition - _head.position, _motion.RotationSpeed*Time.deltaTime, 0.0F);
			_head.rotation = Quaternion.LookRotation(newDir);
			_head.eulerAngles = new Vector3(0, _head.eulerAngles.y, 0);
		}

		/// <summary>
		/// Stand and relax playing default animation.
		/// </summary>
		public void Idle()
		{
			if (_locked) return;
			_motion.State = MotionState.Idling;
			_motion.Animate(AnimationState.Idling);
			if (!_motion.ClampIdleRotation)
			{
				if (_motion.IdleRotationSpeed != 0) _head.eulerAngles += new Vector3(0, _motion.IdleRotationSpeed, 0);
			}
			else
			{
				if (Mathf.Abs(_head.eulerAngles.y - _motion.ClampedIdleRotStart)%360 <= ClampedRotError
				    || Mathf.Abs(_head.eulerAngles.y - _motion.ClampedIdleRotEnd)%360 <= ClampedRotError) _motion.IdleRotationSpeed *= -1;
				if (_motion.IdleRotationSpeed != 0) _head.eulerAngles += new Vector3(0, _motion.IdleRotationSpeed, 0);
			}
		}

		/// <summary>
		/// Stand still idling, with different animation from idle one.
		/// </summary>
		public void Stand() { RefuseAndIdle(); }

		/// <summary>
		/// Walk to target, playing corresponding animation and audio. Idle if out of energy.
		/// </summary>
		public void WalkToTarget() { RefuseAndIdle(); }

		/// <summary>
		/// Walk from target, playing corresponding animation and audio. Idle if out of energy.
		/// </summary>
		public void WalkFromTarget() { RefuseAndIdle(); }

		/// <summary>
		/// Run to target, playing corresponding animation and audio. Walk to target if out of energy.
		/// </summary>
		public void RunToTarget() { RefuseAndIdle(); }

		/// <summary>
		/// Run from target, playing corresponding animation and audio. Walk from target if out of energy.
		/// </summary>
		public void RunFromTarget() { RefuseAndIdle(); }

		/// <summary>
		/// Get random point on the NavMesh and walk to it. 
		/// </summary>
		public void WanderAround() { RefuseAndIdle(); }
		
		/// <summary>
		/// Get random point on the NavMesh and run to it. 
		/// </summary>
		public void RunAround() { RefuseAndIdle(); }
		
		/// <summary>
		/// Set next waypoint as a target and walk to it. Repeat when Agent got to waypoint.
		/// </summary>
		public void WalkAroundWaypoints() { RefuseAndIdle(); }

		/// <summary>
		/// Set next waypoint as a target and run to it. Repeat when Agent got to waypoint.
		/// </summary>
		public void RunAroundWaypoints() { RefuseAndIdle(); }

		/// <summary>
		/// Get random point on the NavMesh and walk to it. Stand for specified amount of time. Walk again.
		/// </summary>
		public void Patrol() { RefuseAndIdle(); }
		
		/// <summary>
		/// Quick move in certain direction, with a corresponding animation.
		/// </summary>
		/// <param name="direction"></param>
		public void Dodge(string direction){ RefuseAndIdle(); }

		/// <summary>
		/// Let other Agents push this one around.
		/// </summary>
		/// <param name="pusherPos"></param>
		/// <param name="pushPower"></param>
		public void Push(Vector3 pusherPos, float pushPower) { RefuseAndIdle(); }
		
		#endregion
		
		#region UTILITY
		private void RefuseAndIdle()
		{
			Debug.Log("I am a Turret. I cannot do anything except Idle");
			Idle();
		}
		#endregion
	}
}
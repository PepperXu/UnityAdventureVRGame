using System;
using System.Collections;
#if ASTAR_EXISTS
using Pathfinding;
#endif
using UnityEngine;
using UnityEngine.AI;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates the control over Agent's Animation or Animator
	/// components depending on the current state of the Agent.
	/// </summary>
	[Serializable]
	public class AgentAnimation
	{
		/// <summary>
		/// Animation engine.
		/// </summary>
		public AnimationMode AnimationMode
		{
			get { return _animationMode; }
		}

		/// <summary>
		/// Reference to the Legacy Animation component.
		/// </summary>
		public UnityEngine.Animation LegacyAnimation
		{
			set { _animation = value; }
		}

		/// <summary>
		/// Reference to the Animator component.
		/// </summary>
		public Animator Animator
		{
			get { return _animator; }
			set { _animator = value; }
		}

		public LegacyAnimationClips Clips
		{
			get { return _clips; }
		}

		public AnimatorParameters Parameters
		{
			get { return _parameters; }
		}

		public bool ApplyRootMotion
		{
			get { return _applyRootMotion; }
		}

		/// Animation engine.
		[SerializeField] private AnimationMode _animationMode;
		
		#region LEGACY PROPERTIES
		[Header("Legacy")]
		[Tooltip("Reference to the Legacy Animation component. Leave empty if " +
		         "you are not using legacy animations on Agent's graphics.")]
		[SerializeField] private UnityEngine.Animation _animation;
		[SerializeField] private LegacyAnimationClips _clips;
		#endregion
		
		#region ANIMATOR PROPERTIES
		[Header("Animator")]
		[Tooltip("Reference to the Animator component. Leave empty if " +
		         "you are not using Animator on Agent's graphics.")]
		[SerializeField] private Animator _animator;
		[Tooltip("Use this if you want this Agent to utilize Animator's root" +
		         " motion for moving rather than just one of motion engines.")]
		[SerializeField] private bool _applyRootMotion;
		[Tooltip("If you use rootmotion, afent's graphics will be detached from " +
		         "the agent itself in the hiherarchy. Set this toggle to true if" +
		         " you want to change graphics' name to be able to see more clearly" +
		         " which Agent does it belong to.")]
		[SerializeField] private bool _changeGraphicsName;
		[SerializeField] private AnimatorParameters _parameters;
		#endregion
		
		#region ANIMATOR ROOTMOTION DATA

		private Vector3 _groundNormal;
		private float _groundCheckDistance = 1f;
		private float _turnAmount;
		private float _forwardAmount;
		private float _stationaryTurnSpeed = 1f;
		private float _movingTurnSpeed = 10f;
		
		#endregion

		private Agent _agent;

		/// <summary>
		/// Initialize the component
		/// </summary>
		/// <param name="agent"></param>
		public void Init(Agent agent)
		{
			_agent = agent;
			
			// Detach the graphics if Agent uses rootmotion.
			if (AnimationMode == AnimationMode.Mecanim && _applyRootMotion)
			{
				_animator.gameObject.transform.parent = _agent.transform.parent;
				if (_changeGraphicsName)
					_animator.gameObject.name = "__graphics__" + "[" + _agent.gameObject.name + "]";
			}
		}

		/// <summary>
		/// This method gets called every Agent's update. Once every frame by default.
		/// </summary>
		public void Update()
		{
			if (_animation && AnimationMode == AnimationMode.Legacy && !_animation.isPlaying)
					Animate(AnimationState.Idling);
			
			if (_animator && AnimationMode == AnimationMode.Mecanim && _applyRootMotion)
			{
				var remainingDistance = Vector3.Distance(_agent.transform.position, _agent.Target.position);
				var stoppingDistance = 0f;
				var desiredVelocity = Vector3.zero;
#if ASTAR_EXISTS
				desiredVelocity = _agent.Motion.Type == MotionEngine.NavMesh
					? _agent.GetComponent<NavMeshAgent>().desiredVelocity
					: _agent.Motion.Type == MotionEngine.Astar
						? _agent.GetComponent<IAstarAI>().desiredVelocity : Vector3.zero;
				stoppingDistance = _agent.Motion.Type == MotionEngine.NavMesh
					? _agent.GetComponent<NavMeshAgent>().stoppingDistance
					: _agent.Motion.Type == MotionEngine.Astar
						? _agent.GetComponent<AIPath>().endReachedDistance : 0f;
#else
				desiredVelocity = _agent.Motion.Type == MotionEngine.NavMesh 
					? _agent.GetComponent<NavMeshAgent>().desiredVelocity : Vector3.zero;
				stoppingDistance = _agent.Motion.Type == MotionEngine.NavMesh
					? _agent.GetComponent<NavMeshAgent>().stoppingDistance : 0f;
#endif
				if (remainingDistance > stoppingDistance)
					RootmotionMove(desiredVelocity);
				else
					RootmotionMove(Vector3.zero);

				_agent.transform.position =new Vector3( _animator.transform.position.x, _agent.transform.position.y,  _animator.transform.position.z);
				_agent.transform.rotation = _animator.transform.rotation;
				if (_animator.transform.position.y != _agent.transform.position.y)
					_animator.transform.position += new Vector3(0, _agent.transform.position.y-_animator.transform.position.y, 0);
			}
		}

		/// <summary>
		/// Execute legacy animation or trigger Animator depending on the Agent's Animation Mode.
		/// </summary>
		/// <param name="clip">Legacy Animation Clip.</param>
		/// <param name="fadeLength">Period of time over which animation clip will fade.</param>
		/// <param name="animatorTrigger">Sets the value of the given Animator parameter.</param>
		/// <param name="force">Legacy Animation Clip.</param>
		private void AnimateByMode(AnimationClip clip, float fadeLength, string animatorTrigger, bool force = false)
		{
			if (_animationMode == AnimationMode.Legacy)
				Animate(clip, fadeLength, force);
			else if(!string.IsNullOrEmpty(animatorTrigger))
				Animate(animatorTrigger);
		}

		/// <summary>
		/// Execute legacy animation.
		/// </summary>
		/// <param name="clip">Legacy Animation Clip.</param>
		/// <param name="fadeLength">Period of time over which animation clip will fade.</param>
		/// <param name="force">Legacy Animation Clip.</param>
		public void Animate(AnimationClip clip, float fadeLength = 0.3F, bool force = false)
		{
			if (_animation && clip != null)
			{
				if (force) _animation.Stop();
				_animation.CrossFade(clip.name, fadeLength);
			}
		}

		/// <summary>
		/// Execute animation by state using default animations.
		/// </summary>
		/// <param name="state">State which dictates what Animation clip
		/// or what Animator message exactly to use.</param>
		/// <param name="fadeLength">eriod of time over which animation clip will fade.</param>
		/// <param name="force">Legacy Animation Clip.</param>
		public void Animate(AnimationState state, float fadeLength = 0.3F, bool force = false)
		{
			switch (state)
			{
				case AnimationState.Idling:
					AnimateByMode(_clips.Idle, fadeLength, null, force);
					break;
				case AnimationState.Walking:
					AnimateByMode(_clips.Walk, fadeLength, null, force);
					break;
				case AnimationState.Running:
					AnimateByMode(_clips.Run, fadeLength, null, force);
					break;
				case AnimationState.Dodging:
					AnimateByMode(_clips.Dodge, fadeLength, _parameters.DodgeTrigger, force);
					break;
				case AnimationState.TakingDamage:
					AnimateByMode(_clips.TakeDamage, fadeLength, _parameters.TakeDamageTrigger, force);
					break;
				case AnimationState.LoadingSkill:
					AnimateByMode(_clips.LoadSkill, fadeLength, _parameters.LoadSkillTrigger, force);
					break;
				case AnimationState.UsingSkill:
					AnimateByMode(_clips.UseSkill, fadeLength, _parameters.UseSkillTrigger, force);
					break;
			}
		}

		#region ANIMATOR

		/// <summary>
		/// Trigger Animator by message.
		/// </summary>
		/// <param name="triggerMessage">Sets the value of the given Animator parameter.</param>
		/// <param name="duration"></param>
		public void Animate(string triggerMessage, float duration = 0.1f)
		{
			_animator.SetTrigger(triggerMessage);
			_agent.StartCoroutine(ResetTriggerEnum(triggerMessage, duration));
		}
		
		/// <summary>
		/// Make sure Agent's animation does not stuck.
		/// </summary>
		/// <param name="triggerMessage"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		private IEnumerator ResetTriggerEnum(string triggerMessage, float duration)
		{
			yield return new WaitForSeconds(duration);
			_animator.ResetTrigger(triggerMessage);
		}

		/// <summary>
		/// Animate Agent's motion, taking into consideration current Agent's Motion State.
		/// </summary>
		/// <param name="state"></param>
		public void SetSpeed(MotionState state)
		{
			if (_applyRootMotion) return;
			var animatorSpeed = 0f;
			switch (state)
			{
				case MotionState.Walking: case MotionState.WalkingAway:
					animatorSpeed = 0.4f;
					break;
				case MotionState.Running: case MotionState.RunningAway:
					animatorSpeed = 1f;
					break;
			}
			_animator.SetFloat(_parameters.Speed, animatorSpeed);
		}
		
		/// <summary>
		/// Update the Animator parameters based upon Agent's input.
		/// This method is based on Unity Standard Assets' Third person character.
		/// </summary>
		/// <param name="move"></param>
		private void RootmotionMove(Vector3 move)
		{
			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			move = _agent.transform.InverseTransformDirection(move);
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, _groundNormal);
			_turnAmount = Mathf.Atan2(move.x, move.z);
			_forwardAmount = move.z;

			var turnSpeed = Mathf.Lerp(_stationaryTurnSpeed, _movingTurnSpeed, _forwardAmount);
			_agent.transform.Rotate(0, _turnAmount * turnSpeed * Time.deltaTime, 0);

			_animator.SetFloat(_parameters.Speed, _forwardAmount, 0.1f, Time.deltaTime);
			_animator.SetFloat(_parameters.Turn, _turnAmount, 0.1f, Time.deltaTime);
		}
		
		/// <summary>
		/// Update the Animator parameters based upon Agent's input.
		/// This method is based on Unity Standard Assets' Third person character.
		/// </summary>
		private void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(_agent.transform.position + (Vector3.up * 0.1f),
				_agent.transform.position + (Vector3.up * 0.1f) + (Vector3.down * _groundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			if (Physics.Raycast(_agent.transform.position + (Vector3.up * 0.1f),
				Vector3.down, out hitInfo, _groundCheckDistance))
			{
				_groundNormal = hitInfo.normal;
				_animator.applyRootMotion = true;
			}
			else
			{
				_groundNormal = Vector3.up;
				_animator.applyRootMotion = false;
			}
		}
		

		#endregion
	}
}
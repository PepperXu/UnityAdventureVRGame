using UnityEngine;

namespace Eliot.BehaviourEngine
{
	/// <summary>
	/// Transition binds other components together,
	/// transmitting information between them.
	/// </summary>
	public class Transition
	{
		/// Component that needs to be activated.
		private readonly EliotComponent _target;
		/// Wheather this Transition is currently allowed to pass the signal further.
		private bool _active = true;
		/// Minimum times the Transition will skip the Update before activating
		/// its target Component.
		private readonly int _activationRateMin;
		/// Maximum times the Transition will skip the Update before activating
		/// its target Component.
		private readonly int _activationRateMax;
		/// Actual number of times the Transition will skip the Update before
		/// activating its target Component.
		private int _activationRate;
		/// Number of times the Transition already skiped the Update.
		private int _currentRate = 1;
		/// Wheather or not the Transition should skip any Updates based on probability.
		private readonly bool _useProbability = true;
		/// Minimum duration of time the Transition should skip the Update.
		private readonly float _minCooldown;
		/// Maximum duration of time the Transition should skip the Update.
		private readonly float _maxCooldown;
		/// Last time the Transition Updated based on cooldown.
		private float _lastTimeUpdated;
		/// Actual duration of time the Transition should skip the Update.
		private float _cooldown;
		/// Wheather or not the Transition should skip any Updates based on cooldown.
		private readonly bool _useCooldown = true;
		/// Wheather or not the successful Update should prevent all the
		/// other Trannsitions in the same group from being Updated.
		private readonly bool _terminateOriginUpdate;

		/// <summary>
		/// Iitialize the component.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="activationRateMin"></param>
		/// <param name="activationRateMax"></param>
		/// <param name="minCooldown"></param>
		/// <param name="maxCooldown"></param>
		/// <param name="terminateOriginUpdate"></param>
		public Transition(EliotComponent target, int activationRateMin, int activationRateMax,
			float minCooldown, float maxCooldown, bool terminateOriginUpdate)
		{
			_target = target;
			if (activationRateMin <= 0) activationRateMin = 1;
			if (activationRateMax <= activationRateMin) 
				activationRateMax = activationRateMin + 1;
			_activationRateMin = activationRateMin;
			_activationRateMax = activationRateMax;
			
			if (_activationRateMin == 1 && _activationRateMax == 2)
				_useProbability = false;

			if (minCooldown < 0) minCooldown = 0;
			if (maxCooldown < 0) maxCooldown = 0;
			if (minCooldown == 0 && maxCooldown == 0) _useCooldown = false;
			if (_useCooldown)
			{
				if (maxCooldown <= minCooldown)
					maxCooldown = minCooldown + 1;
			}

			_minCooldown = minCooldown;
			_maxCooldown = maxCooldown;

			

			_terminateOriginUpdate = terminateOriginUpdate;
		}

		/// <summary>
		/// Make Transition do its job.
		/// </summary>
		public bool Update()
		{
			if (!_useCooldown)
			{
				if (!_useProbability)
				{
					_target.Update();
					return !_terminateOriginUpdate;
				}

				if (!_active)
				{
					if (_currentRate < _activationRate)
						_currentRate++;
					else _active = true;
				}
				else
				{
					_target.Update();
					_activationRate = Random.Range(_activationRateMin, _activationRateMax);
					_currentRate = 1;
					_active = false;
					return !_terminateOriginUpdate;
				}
			}
			else
			{
				if (!_useProbability)
				{
					if (Time.time >= _cooldown + _lastTimeUpdated)
					{
						_target.Update();
						_lastTimeUpdated = Time.time;
						_cooldown = Random.Range(_minCooldown, _maxCooldown);
						return !_terminateOriginUpdate;
					}
				}
				else
				{
					if (!_active)
					{
						if (_currentRate < _activationRate)
							_currentRate++;
						else _active = true;
					}
					else
					{
						if (Time.time >= _cooldown + _lastTimeUpdated)
						{
							_target.Update();
							_activationRate = Random.Range(_activationRateMin, _activationRateMax);
							_currentRate = 1;
							_active = false;
							_lastTimeUpdated = Time.time;
							_cooldown = Random.Range(_minCooldown, _maxCooldown);
							return !_terminateOriginUpdate;
						}
					}
				}
			}

			return true;
		}
	}
}


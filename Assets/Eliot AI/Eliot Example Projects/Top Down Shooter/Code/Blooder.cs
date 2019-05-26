using System.Collections;
using System.Collections.Generic;
using Eliot.AgentComponents;
using UnityEngine;

public class Blooder : MonoBehaviour
{

	[SerializeField] private GameObject _bloodParticles;
	[SerializeField] private PlayerStats _playerStats;
	[SerializeField] private bool _tryGetAgentOnStart = true;
	[SerializeField] private Agent _agent;

	public void Blood()
	{
		if(_bloodParticles) 
		    Instantiate(_bloodParticles, transform.position, transform.rotation);
	}
	
	public void Damage(int power)
	{
		if(_playerStats)
			_playerStats.Damage(power);
		if(_agent)
			_agent.Resources.Damage(power);
		Blood();
	}

	private void Start()
	{
		if(_tryGetAgentOnStart && GetComponent<Agent>()) _agent = GetComponent<Agent>();
	}
}

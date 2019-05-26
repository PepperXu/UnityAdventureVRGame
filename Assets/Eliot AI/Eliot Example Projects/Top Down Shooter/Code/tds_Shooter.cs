using System.Collections;
using System.Collections.Generic;
using Eliot.AgentComponents;
using UnityEngine;

public class tds_Shooter : MonoBehaviour
{
	[SerializeField] private GameObject projectile;

	[SerializeField] private Transform origin;

	[SerializeField] private int _minPower;
	[SerializeField] private int _maxPower;

	[SerializeField] private float _ping = 0.2f;
	private float _lastShootTime;

	[SerializeField] private Skill _skill;

	public Agent agentComponent;

	private AudioSource _audioSource;

	public AudioClip ShootSound;
	private GameController GC;
	
	// Use this for initialization
	void Start ()
	{
		_audioSource = GetComponent<AudioSource>() ? GetComponent<AudioSource>() : gameObject.AddComponent<AudioSource>();
		_audioSource.clip = ShootSound;
		GC = GameObject.Find("GameController").GetComponent<GameController>();
	}
	
	// Update is called once per frame
	private void Update () {
		if (!GC.Paused && Input.GetKey(KeyCode.Mouse0) && Time.time > _ping + _lastShootTime)
		{
			var pjtl = Instantiate(projectile, origin.position, origin.rotation) as GameObject;
			pjtl.GetComponent<Projectile>().Init(agentComponent, null, _skill, _minPower, _maxPower);
			_lastShootTime = Time.time;
			Skill.MakeNoise(20, origin, 10f);

			_audioSource.Play();
		}
	}
}

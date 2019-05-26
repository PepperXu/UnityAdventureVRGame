#pragma warning disable CS0414
using Eliot.AgentComponents;
using Eliot.BehaviourEditor;
using UnityEngine;

namespace Eliot.Environment
{
	/// <summary>
	/// Represents an item that an Agent can carry in his Inventory.
	/// Items can hold Skills and some Agent's characteristics so that
	/// they can be changed when Agent uses the item. Skills let Items
	/// to be used as potions, and other options let Items to change Agent's
	/// Behaviour model, his graphics components etc.
	/// </summary>
	[RequireComponent(typeof(Unit))]
	public class Item : MonoBehaviour
	{
		/// <summary>
		/// User-defined type of the item. May vary depending on the context of the game.
		/// </summary>
		public ItemType Type
		{
			get { return _itemType; }
		}

		/// <summary>
		/// Measurement of how much the item is valuable.
		/// </summary>
		public int Value
		{
			get { return _value; }
		}

		/// <summary>
		/// How much weight or space does it take inside the Inventory.
		/// </summary>
		public float Weight
		{
			get { return _weight * _amount; }
		}

		/// <summary>
		/// Return the Skill assigned to the Item.
		/// </summary>
		public Skill Skill
		{
			get { return _skill; }
		}

		[Tooltip("Measurement of how much the item is valuable.")]
		[SerializeField] private int _value;
		[Tooltip("How much weight or space does it take inside the Inventory.")]
		[SerializeField] private float _weight;
		[Tooltip("The number of times this Item occurs in Inventory.")]
		[SerializeField] private int _amount = 1;
		/// User-defined type of the item. May vary depending on the context of the game.
		[SerializeField] private ItemType _itemType;
		[Tooltip("The Skill that will be applied to an Agent when he uses the Item.")]
		[SerializeField] private Skill _skill;
		
		[Header("On Wield")]
		[Tooltip("New Behaviour model (if there is one) will be set for Agent when he wields the Item.")]
		[SerializeField] private EliotBehaviour _newBehaviour;
		[Tooltip("New skills will be added to Agent's didposal when he wields the item.")]
		[SerializeField] private Skill[] _addSkills;
		[Tooltip("New graphics (if there is one) will be set for Agent when he wields the Item.")]
		[SerializeField] private GameObject _newGraphics;
		
		[Space]
		[Tooltip("Sound of wielding the item.")]
		[SerializeField] private AudioClip _wieldSound;
		[Tooltip("Sound of unwielding the item.")]
		[SerializeField] private AudioClip _unwieldSound;
		[Tooltip("Sound of using the item as a potion.")]
		[SerializeField] private AudioClip _useSound;

		/// Wheather the Item is currently in an Inventory.
		private bool _isInInventory;
		/// MeshRenderer component of the Item's gameObject;
		private MeshRenderer _renderer;
		/// Collider component of the Item's gameObject;
		private Collider _collider;
		/// Inventory that the Item is currently in. None if the Item is outside the Inventory.
		private Inventory _currentInventory;
		
		/// <summary>
		/// Use this for initialization.
		/// </summary>
		private void Start()
		{
			_renderer = GetComponent<MeshRenderer>();
			_collider = GetComponent<Collider>();
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		private void Update()
		{
			
		}

		/// <summary>
		/// Put Item into cpecified Agent's Inventory.
		/// </summary>
		/// <param name="inventory"></param>
		public void AddToInventory(Inventory inventory)
		{
			_isInInventory = true;
			_currentInventory = inventory;
			inventory.AddItem(GetComponent<Item>());
			gameObject.SetActive(false);
			transform.parent = inventory.ItemsContainer;
			transform.localPosition = Vector3.zero;
		}

		/// <summary>
		/// Drop Item from Inventory if it currently is in one.
		/// </summary>
		public void GetDropped(float dropRadius)
		{
			if (!_isInInventory) return;
			gameObject.SetActive(true);
			_isInInventory = false;
			_currentInventory = null;
			transform.parent = null;
			var offset = Random.insideUnitCircle * dropRadius;
			transform.position += new Vector3(offset.x, 0, offset.y);
		}

		/// <summary>
		/// Apply the skill held by this Item to an Agent.
		/// </summary>
		/// <param name="agent"></param>
		public void Use(Agent agent)
		{
			if (!_skill) return;
			agent.AddEffect(_skill, agent);
			_amount--;
			if (_amount > 0) return;
			GetDropped(agent.Inventory.DropRadius);
			Destroy(gameObject);
		}

		/// <summary>
		/// Make an Agent wield the Item. May lead to changes in Agent's cofiguration.
		/// </summary>
		/// <param name="agent"></param>
		public void Wield(Agent agent)
		{
			if(_currentInventory.WieldedItem)
				_currentInventory.WieldedItem.Unwield(agent);
			
			_currentInventory.WieldedItem = this;
			if(_addSkills.Length > 0)
				foreach (var skill in _addSkills)
					agent.AddSkill(skill);
			if(_newBehaviour) agent.SetBehaviour(_newBehaviour);
			if(_newGraphics) agent.ReplaceGraphics(_newGraphics);
			
			if (_wieldSound)
			{
				agent.GetAudioSource().clip = _wieldSound;
				agent.GetAudioSource().Play();
			}
		}

		/// <summary>
		/// Make Agent unwield the Item. Returns Agent's configuration to default.
		/// </summary>
		/// <param name="agent"></param>
		public void Unwield(Agent agent)
		{
			if(_addSkills.Length > 0)
				foreach (var skill in _addSkills)
					agent.RemoveSkill(skill);
			
			if (_unwieldSound)
			{
				agent.GetAudioSource().clip = _unwieldSound;
				agent.GetAudioSource().Play();
			}
			
			agent.ResetGraphics();
		}
	}
}
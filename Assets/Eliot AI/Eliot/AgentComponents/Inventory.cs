using System;
using System.Collections.Generic;
using Eliot.Environment;
using UnityEngine;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Incapsulates behaviour related to interaction with Items (Keep, use, wield, throw away, etc.).
	/// </summary>
	[Serializable]
	public class Inventory
	{
		/// <summary>
		/// Child of Agent's Transform that is a parent to all Items in the Agent's Inventory.
		/// </summary>
		public Transform ItemsContainer
		{
			get { return _itemsContainer; }
		}

		/// <summary>
		/// Item that is currently being wielded.
		/// </summary>
		public Item WieldedItem
		{
			get { return _wieldedItem; }
			set { _wieldedItem = value; }
		}

		public float DropRadius
		{
			get { return _dropRadius; }
		}

		/// <summary>
		/// List of all items in the Inventory.
		/// </summary>
		public List<Item> Items
		{
			get { return _items; }
		}

		/// Maximum weight that the inventory can handle.
		[SerializeField] private float _maxWeight = 1f;
		/// List of all items in the Inventory.
		[SerializeField] private List<Item> _items = new List<Item>();
		/// Item that is currently being wielded.
		[SerializeField] private Item _wieldedItem;
		[Space]
		[Tooltip("If true, all objects from list of Items will be put" +
		         " into Inventory even if they are outside at the moment.")]
		[SerializeField] private bool _initFromList;
		[Tooltip("If true, all children Items of the ItemsContainer " +
		         "will be set up apprepriately.")]
		[SerializeField] private bool _initFromChildren;
		/// Maximum distance from an Agent in which an Item will be dropped.
		[Space][SerializeField] private float _dropRadius = 1f;

		/// A link to actual controller.
		private Agent _agent;
		/// Child of Agent's Transform that is a parent to all Items in the Agent's Inventory.
		private Transform _itemsContainer;

		/// Action Interface for this Inventory instance.
        private List<InventoryActionInterface> _actionInterfaces = new List<InventoryActionInterface>();
		/// Condition Interface for this Inventory instance.
        private List<InventoryConditionInterface> _conditionInterfaces = new List<InventoryConditionInterface>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="inventoryTransform"></param>
        public Inventory(Agent agent, Transform inventoryTransform)
		{
			Init(agent, inventoryTransform);
		}

		/// <summary>
		/// Initialize Inventory components.
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="inventoryTransform"></param>
		public void Init(Agent agent, Transform inventoryTransform = null)
		{
			_agent = agent;
			_itemsContainer = inventoryTransform != null ? inventoryTransform : agent.FindTransformByName("__inventory__");

			if(_initFromList) InitItemsFromList();
			if (_initFromChildren) InitItemsFromChildren();
		}

		/// <summary>
		/// Put Items from the Items list in inventory.
		/// </summary>
		private void InitItemsFromList()
		{
			if (_items.Count <= 0) return;
			foreach (var item in _items)
				item.AddToInventory(this);
		}
		
		/// <summary>
		/// Make sure children Items of container are sut up properly. 
		/// </summary>
		private void InitItemsFromChildren()
		{
			if (_itemsContainer.childCount <= 0) return;
			foreach (Transform item in _itemsContainer)
				if(item.GetComponent<Item>())
					item.GetComponent<Item>().AddToInventory(this);
		}

        /// <summary>
        /// Calculate weight of all items in the Inventory.
        /// </summary>
        /// <returns>Returns weight of all items in the Inventory.</returns>
        public float CurrentWeight()
		{
			var res = 0f;
			foreach (var item in _items)
				res += item.Weight;
			return res;
		}

		/// <summary>
		/// Add new Item to the Inventory.
		/// </summary>
		/// <param name="item"></param>
		public void AddItem(Item item)
		{
			if (!_items.Contains(item))
			{
				if (CurrentWeight() + item.Weight > _maxWeight)
					DropWorstItem();
				_items.Add(item);
			}
		}

		/// <summary>
		/// Find Item with the least value.
		/// </summary>
		/// <returns>Returns Item with the smallest value.</returns>
		private Item WorstItem()
		{
			var minValue = int.MaxValue;
			var index = 0;
			for (var i = 0; i < _items.Count; i++)
				if (_items[i].Value < minValue)
				{
					minValue = _items[i].Value;
					index = i;
				}

			return _items[index];
		}

		/// <summary>
		/// Find the best Item of type Weapon.
		/// </summary>
		/// <returns>Returns Item of type Weapon with the biggest value.</returns>
		public Item BestWeapon()
		{
			var maxValue = 0;
			var index = 0;
			for(var i = 0; i < _items.Count; i++)
			{
				if (_items[i].Type == ItemType.Weapon && _items[i].Value > maxValue)
				{
					maxValue = _items[i].Value;
					index = i;
				}
			}

			return _items[index];
		}
		
		/// <summary>
		/// Drop Item out of Inventory and unwield it if necessary.
		/// </summary>
		/// <param name="item"></param>
		public void DropItem(Item item)
		{
			_items.Remove(item);
			item.GetDropped(_dropRadius);
			if(WieldedItem == item)
				item.Unwield(_agent);
		}
		
		/// <summary>
		/// Find the worst Item and drop it.
		/// </summary>
		public void DropWorstItem()
		{
			if (_items.Count == 0) return;
			DropItem(WorstItem());
		}

		/// <summary>
		/// Check if an item is better than the currently wielded one.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool ItemIsBetterThanCurrent(Item item)
		{
			if (!_wieldedItem) return true;
			return item.Value > _wieldedItem.Value;
		}

		/// <summary>
		/// Drop all Items from Inventory.
		/// </summary>
		public void DropAllItems()
		{
			if (_items.Count <= 0) return;
			for (var i = _items.Count - 1; i >= 0; i--)
			{
				_items[i].GetDropped(_dropRadius);
				if(WieldedItem == _items[i])
					_items[i].Unwield(_agent);
				_items.RemoveAt(i);
			}
		}

		/// <summary>
		/// Return true if Agent has a better item of type ItemType in the inventory.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool HaveBetterItem(ItemType type)
		{
			foreach (var item in Items)
				if (item.Type == type && ItemIsBetterThanCurrent(item)) 
					return true;

			return false;
		}

        #region ADD_INTARFACES
        public InventoryActionInterface AddActionInterface(string methodName)
        {
	        return AgentFunctions.AddActionInterface(methodName, ref _actionInterfaces, _agent);
        }

        public InventoryConditionInterface AddConditionInterface(string methodName)
        {
	        return AgentFunctions.AddConditionInterface(methodName, ref _conditionInterfaces, _agent);
        }
        #endregion
    }
}
using System;
using System.Linq;
using System.Reflection;
using Eliot.BehaviourEngine;
using Eliot.BehaviourEditor;
using UnityEngine;
using System.Collections.Generic;
using Eliot.Utility;

namespace Eliot.AgentComponents
{
	/// <summary>
	/// Responsible for the construction of delegates by methods names
	/// and for retrieving some reflective information from Agent's components.
	/// </summary>
	public static class AgentFunctions
	{
        #region ReflectionToying

        /// <summary>
        /// Get an array of direct descendents if an arbitrary class T.
        /// </summary>
        /// <typeparam name="T">T can be any class.</typeparam>
        /// <returns>Returns an array of public nonstatic methods of arbitrary class.</returns>
        private static List<Type> GetExtentions<T>(){
            List<Type> types = new List<Type>();
            foreach (Type type in Assembly.GetAssembly(typeof(T)).GetTypes())
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)) && type.BaseType == (typeof(T)))
                    types.Add(type);
            return types;
        }
		
		/// <summary>
		/// Get an array of public nonstatic methods of arbitrary class.
		/// </summary>
		/// <typeparam name="T">T can be any class.</typeparam>
		/// <returns>Returns an array of public nonstatic methods of arbitrary class.</returns>
        public static string[] GetFunctions<T>(bool sorted = true)
        {
            List<Type> types = GetExtentions<T>();
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach(var obj in types)
            {
                var localMethodsList = obj.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach(var method in localMethodsList)
                    methods.Add(method);
            }
            var filtered = from method in methods
                           where method.GetCustomAttributes(typeof(IncludeInBehaviour), false).Any()
                           select method;
            var names = from name in filtered select name.Name;
            if (!sorted)
                return names.ToArray();
            var sortedNames = names.ToArray();
            Array.Sort(sortedNames, StringComparer.InvariantCulture);
            return sortedNames;
        }

        /// <summary>
        /// Get array of methods of a class corresponding to needed Condition Group
        /// </summary>
        /// <param name="conditionGroup"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string[] GetConditionStrings(ConditionGroup conditionGroup)
		{
			switch (conditionGroup)
			{
				case ConditionGroup.Resources: return GetFunctions<ResourcesConditionInterface>();
				case ConditionGroup.Perception: return GetFunctions<PerceptionConditionInterface>();
				case ConditionGroup.Motion: return GetFunctions<MotionConditionInterface>();
				case ConditionGroup.Inventory: return GetFunctions<InventoryConditionInterface>();
				case ConditionGroup.General: return GetFunctions<GeneralConditionInterface>();
                case ConditionGroup.Time: return new []{""};
                default: throw new ArgumentOutOfRangeException("conditionGroup", conditionGroup, null);
			}
		}

		/// <summary>
		/// Get array of methods of a class corresponding to needed index of Condition Group
		/// </summary>
		/// <param name="conditionGroupIndex"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string[] GetConditionStrings(int conditionGroupIndex)
		{
			switch (conditionGroupIndex)
			{
				case 0: return GetConditionStrings(ConditionGroup.Resources);
				case 1: return GetConditionStrings(ConditionGroup.Perception);
				case 2: return GetConditionStrings(ConditionGroup.Motion);
				case 3: return GetConditionStrings(ConditionGroup.Inventory);
				case 4: return GetConditionStrings(ConditionGroup.General);
                case 5: return GetConditionStrings(ConditionGroup.Time);
                default: throw new ArgumentOutOfRangeException("conditionGroupIndex", conditionGroupIndex, null);
			}
		}


        public static object GetTypeByMethodName<T>(string methodName)
        {
            List<Type> types = GetExtentions<T>();
            foreach (var obj in types)
            {
                var localMethodsList = obj.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in localMethodsList)
                    if (method.Name == methodName)
                        return obj;
            }
            return null;
        }

		#endregion

		#region DelegatesBuilder

		/// <summary>
		/// Build a delegate that will be used by one of Invokers of an Agent's
		/// Behaviour Core to invoke specific Agent's action.
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="gameObject"></param>
		/// <param name="actionGroup"></param>
		/// <param name="executeSkill"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static EliotAction CreateAction(string methodName, GameObject gameObject, ActionGroup actionGroup, bool executeSkill)
		{
			MethodInfo metInfo;
			object component;
			switch (actionGroup)
			{
				case ActionGroup.Motion:
                    component = gameObject.GetComponent<Agent>().Motion.AddActionInterface(methodName);
                    break;
				case ActionGroup.Inventory:
                    component = gameObject.GetComponent<Agent>().Inventory.AddActionInterface(methodName);
                    break;
				case ActionGroup.Skill:
					return gameObject.GetComponent<Agent>().Skill(methodName, executeSkill);
				default:
					throw new ArgumentOutOfRangeException(actionGroup.ToString(), actionGroup, null);
			}
			if (component == null)
			{
				Debug.LogWarning("component object is null");
				return null;
			}
			metInfo = component.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
			if (metInfo == null) return null;
			return (EliotAction) Delegate.CreateDelegate(typeof(EliotAction), component, metInfo, true);
		}

		/// <summary>
		/// Build a delegate that will be used by one of Observers or Loops of an Agent's
		/// Behaviour Core to check specific Agent's condition.
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="gameObject"></param>
		/// <param name="conditionGroup"></param>
		/// <returns></returns>
		public static EliotCondition CreateCondition(string methodName, GameObject gameObject, ConditionGroup conditionGroup, int nodeId)
		{
			MethodInfo metInfo = null;
			object component = null;
			switch (conditionGroup)
			{
				case ConditionGroup.Resources:
                    component = gameObject.GetComponent<Agent>().Resources.AddConditionInterface(methodName);
					break;
				case ConditionGroup.Perception:
					component = gameObject.GetComponent<Agent>().Perception.AddConditionInterface(methodName);
					break;
				case ConditionGroup.Motion:
					component = gameObject.GetComponent<Agent>().Motion.AddConditionInterface(methodName);
					break;
				case ConditionGroup.Inventory:
					component = gameObject.GetComponent<Agent>().Inventory.AddConditionInterface(methodName);
					break;
				case ConditionGroup.General:
					component = gameObject.GetComponent<Agent>().GeneralSettings.AddConditionInterface(methodName);
					break;
                case ConditionGroup.Time:
                    var strings = methodName.Split('/');
                    if(strings.Length == 1){
                        return EliotTime.IsTime(float.Parse(strings[0]), gameObject.GetHashCode(), nodeId);
                    }
                    var min = float.Parse(strings[0]);
                    var max = float.Parse(strings[1]);
                    return EliotTime.IsTime(min, max, gameObject.GetHashCode(), nodeId);
            }

			if (component == null)
			{
				Debug.LogWarning("component object is null");
				return null;
			}
			metInfo = component.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
			if (metInfo == null) return null;
			var test = Delegate.CreateDelegate(typeof(EliotCondition), component, metInfo, true);
			var function = (EliotCondition) test;
			return function;
		}

		#endregion
		
		#region ADD_INTARFACES
		
		/// <summary>
		/// Find an action interface that contains the specified method and return it.
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="actionInterfaces"></param>
		/// <param name="agent"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T AddActionInterface<T>(string methodName, ref List<T> actionInterfaces, Agent agent) where T : ActionInterface
		{
			var obj = AgentFunctions.GetTypeByMethodName<T>(methodName);
			var className = obj.ToString();
			if (actionInterfaces == null) actionInterfaces = new List<T>();
			if (actionInterfaces.Count > 0)
				for (int i = 0; i < actionInterfaces.Count; i++)
				{
					if (actionInterfaces[i].GetType().ToString() == className)
						return actionInterfaces[i];
				}
			var type = System.Type.GetType(className);

			var ctor = type.GetConstructor(new[] { typeof(Agent) });
			var actionInterface = ctor.Invoke(new object[] { agent }) as T;

			actionInterfaces.Add(actionInterface);
			return actionInterface;
		}

		/// <summary>
		/// Find a condition interface that contains the specified method and return it.
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="conditionInterfaces"></param>
		/// <param name="agent"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T AddConditionInterface<T>(string methodName, ref List<T> conditionInterfaces, Agent agent) where T : ConditionInterface
		{
			var obj = AgentFunctions.GetTypeByMethodName<T>(methodName);
			var className = obj.ToString();
			if (conditionInterfaces == null) conditionInterfaces = new List<T>();
			if (conditionInterfaces.Count > 0)
				for (int i = 0; i < conditionInterfaces.Count; i++)
				{
					if (conditionInterfaces[i].GetType().ToString() == className)
						return conditionInterfaces[i];
				}
			var type = System.Type.GetType(className);

			var ctor = type.GetConstructor(new[] { typeof(Agent) });
			var conditionInterface = ctor.Invoke(new object[] { agent }) as T;

			conditionInterfaces.Add(conditionInterface);
			return conditionInterface;
		}
		
		#endregion
	}
}
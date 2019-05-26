using System;
using System.Collections.Generic;
using System.Linq;
using Eliot.Repository;
using UnityEngine;

namespace Eliot.BehaviourEditor
{
	/// <summary>
	/// Keeps the information about elements of the behaviour, and their interconnections.
	/// </summary>
	[Serializable][CreateAssetMenu(fileName = "New Behaviour", menuName = "Eliot/Behaviour")]
	public class EliotBehaviour : ScriptableObject
	{
		/// <summary>
		/// String that contains all needed information about the structure of the Behaviour model.
		/// </summary>
		public string Json
		{
			get { return _json; }
			set { _json = value; }
		}
#if UNITY_EDITOR
		/// <summary>
		/// Collection of nodes of the behaviour model.
		/// </summary>
		public List<Node> Nodes
		{
			get { return _nodes;}
			set { _nodes = value; }
		}

		/// <summary>
		/// Collection of transitions of the behaviour model.
		/// </summary>
		public List<NodesTransition> Transitions
		{
			get { return _transitions;}
			set { _transitions = value; }
		}
#endif
		/// String that contains all needed information about the structure of the Behaviour model.
		[HideInInspector][SerializeField] private string _json; 
#if UNITY_EDITOR
		/// Collection of nodes of the behaviour model.
		[HideInInspector][SerializeField] private List<Node> _nodes = new List<Node>();
		/// Collection of transitions of the behaviour model.
		[HideInInspector][SerializeField] private List<NodesTransition> _transitions = new List<NodesTransition>();

		/// <summary>
		/// Initialize components.
		/// </summary>
		/// <param name="nodes"></param>
		public void InitTransitions(IEnumerable<Node> nodes)
		{
			var trans =  from node in nodes from transition in node.Transitions select transition;
			Transitions = trans.ToList();
		}
#endif
		/// <summary>
		/// Return functionID of the node searching by its ID.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public string GetFunctionById(string id)
		{
			var jObj = new JsonObject(Json);
			var jNodes = jObj["nodes"].Objects;
			if (jNodes == null || jNodes.Count <= 0) return null;
			foreach (var node in jNodes)
				if (id == ((JsonObject)node)["ID"].String) {return ((JsonObject)node)["functionName"].String;}
			
			return null;
			
		}
	}
}
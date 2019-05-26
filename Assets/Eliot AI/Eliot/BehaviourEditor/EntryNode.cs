#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Eliot.BehaviourEditor
{
	/// <summary>
	/// Node that represents the Entry Component of the behaviour model.
	/// </summary>
	public class EntryNode : Node
	{
		/// <summary>
		/// Empty constructor.
		/// </summary>
		public EntryNode(){}
		
		/// <summary>
		/// Initialize the EntryNode.
		/// </summary>
		/// <param name="rect"></param>
		public EntryNode(Rect rect) : base(rect, "Entry"){}

		/// <summary>
		/// Update the node's functionality.
		/// </summary>
		public override void Update()
		{
			try
			{
				if (Transitions.Count <= 0) return;
				foreach (var transition in Transitions)
					transition.Draw();
			}
			catch(Exception){/**/}
		}
		
		/// <summary>
		/// Draw the context menu of the node.
		/// </summary>
		public override void DrawMenu()
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Transition"), false, StartTransition, null);
			menu.ShowAsContext();
		}

		/// <summary>
		/// Initialize the transition starting from this node.
		/// </summary>
		/// <param name="obj"></param>
		public void StartTransition(object obj) {
			BehaviourEditorWindow.StartTransition(Rect, this, BehaviourEditorWindow.NeutralColor);
		}

		/// <summary>
		/// Node's window component functionality.
		/// </summary>
		/// <param name="id"></param>
		public override void NodeFunction(int id)
		{
			base.NodeFunction(id);
			var y = Rect.y < 0 ? 0 : Rect.y;
			var x = Rect.x < 0 ? 0 : Rect.x;
			var r = new Rect(x, y, 150, 30);
			Rect = r;
			GUILayout.Label("Entry");
		}
	}
}
#endif
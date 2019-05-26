#if UNITY_EDITOR
using Eliot.AgentComponents;
using UnityEditor;
using UnityEngine;

namespace Eliot.BehaviourEditor
{
	/// <summary>
	/// Node that represents the Invoker Component of the behaviour model.
	/// </summary>
	public class InvokerNode : Node
	{
		/// <summary>
		/// The action group of this node that reflects the node's functionality spectre.
		/// </summary>
		public ActionGroup ActionGroup
		{
			get { return _actionGroup; }
			set { _actionGroup = value; }
		}

		/// <summary>
		/// Should a Skill be executed or just set as current for retrieving information about it?
		/// </summary>
		public bool ExecuteSkill
		{
			get { return _executeSkill; }
			set { _executeSkill = value; }
		}
		
		[Tooltip("The action group of this node that reflects the node's functionality spectre.")]
		[SerializeField] private ActionGroup _actionGroup;

		[Tooltip("Should a Skill be executed or just set as current for retrieving information about it?")]
		[HideInInspector][SerializeField] private bool _executeSkill = true;
		
		/// <summary>
		/// Empty constructor.
		/// </summary>
		public InvokerNode(){}
		
		/// <summary>
		/// Initialize the InvokerNode.
		/// </summary>
		/// <param name="rect"></param>
		public InvokerNode(Rect rect) : base(rect,  "Invoker"){}
		
		/// <summary>
		/// This function is called when the object is loaded.
		/// </summary>
		public override void OnEnable()
		{
			base.OnEnable();
			if (FuncNames == null)
			{
				switch (_actionGroup)
				{
					case ActionGroup.Skill:
						FuncNames = new[] {BehaviourEditorWindow.Behaviour.GetFunctionById(Id) ?? "loading..."};
						break;
					case ActionGroup.Motion:
						FuncNames = AgentFunctions.GetFunctions<MotionActionInterface>();
						break;
					case ActionGroup.Inventory:
						FuncNames = AgentFunctions.GetFunctions<InventoryActionInterface>();
						break;
				}
			}
		}

		/// <summary>
		/// Update the node's functionality.
		/// </summary>
		public override void Update()
		{
			if (Transitions.Count <= 0) return;
			foreach (var transition in Transitions) transition.Draw();
		}
		
		/// <summary>
		/// Draw the context menu of the node.
		/// </summary>
		public override void DrawMenu()
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Transition"), false, StartTransition, null);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Delete"), false, Delete, null);
			menu.ShowAsContext();
		}

		/// <summary>
		/// Initialize the transition starting from this node.
		/// </summary>
		/// <param name="obj"></param>
		private void StartTransition(object obj){
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
			var r = new Rect(x, y, 175, 45);
			Rect = r;
			GUILayout.Label(_actionGroup != ActionGroup.Skill ? Func : (_executeSkill ? "<b>" + Func + "</b>" : "<b><color=#666666>" + FunctionName + "</color></b>"));
		}
	}
}
#endif
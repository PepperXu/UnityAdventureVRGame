#if UNITY_EDITOR
using System;
using Eliot.AgentComponents;
using UnityEditor;
using UnityEngine;

namespace Eliot.BehaviourEditor.Editor
{
	/// <summary>
	/// Editor extention for InvokerNode objects.
	/// </summary>
	[CustomEditor(typeof(InvokerNode))]
	public class InvokerNodeEditor : UnityEditor.Editor
	{
		/// Index of the function from the function group that the node holds information about.
		private SerializedProperty _funcIndex;
		/// The name of the method represented by the Node.
		private SerializedProperty _functionName;
		/// The action group of this node that reflects the node's functionality spectre.
		private SerializedProperty _actionGroup;
        /// A list of names of functions in the current function group of the node.
        private string[] _options;
		/// Wheather the node's action group is 'Skill'.
		private bool _skill;
		/// Current skill name that the node holds.
		private string _skillName = "skill name";
		/// Should a Skill be executed or just set as current for retrieving information about it?
		private SerializedProperty _executeSkill;
		
		/// <summary>
		/// Get the array of method names for appropriate action group.
		/// </summary>
		private void GetFunctions()
		{
            switch (_actionGroup.enumValueIndex)
			{
				case 1:
					_skill = false;
					_options = AgentFunctions.GetFunctions<InventoryActionInterface>();
					break;
				case 2:
					_skill = true;
					_options = new[]{ _skillName };
					_funcIndex.intValue = 0;
					break;
				default:
					_skill = false;
					_options = AgentFunctions.GetFunctions<MotionActionInterface>();
					break;
			}
			
			((InvokerNode) target).FuncNames = _options;
		}

		/// <summary>
		/// This function is called when the object is loaded.
		/// </summary>
		private void OnEnable()
		{
			hideFlags = HideFlags.DontSave;
			_funcIndex = serializedObject.FindProperty("_funcIndex");
			_actionGroup = serializedObject.FindProperty("_actionGroup");
			string id = null;
			try
			{
				id = BehaviourEditorWindow.Behaviour.GetFunctionById(((InvokerNode) target).Id);
			}catch(Exception){/**/}
			_skillName = id ?? "skill name";
			_executeSkill = serializedObject.FindProperty("_executeSkill");
			_functionName = serializedObject.FindProperty("_functionName");
			GetFunctions();
		}
		
		/// <summary>
		/// Draw the new inspector properties.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

            serializedObject.Update();
			EditorGUILayout.BeginHorizontal();

            GetFunctions();
			if (!_skill)
			{
				EditorGUIUtility.fieldWidth = 150;
				_funcIndex.intValue = EditorGUILayout.Popup("Function", _funcIndex.intValue, _options);
				_functionName.stringValue = _options[_funcIndex.intValue];
			}
			else
			{
				EditorGUILayout.BeginVertical();
				EditorGUIUtility.fieldWidth = 150;
				_skillName = EditorGUILayout.TextField("Skill name", _skillName);
				EditorGUIUtility.fieldWidth = 150;
				_executeSkill.boolValue = EditorGUILayout.Toggle("Execute Skill", _executeSkill.boolValue);
				_functionName.stringValue = _skillName;
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.EndHorizontal();
			serializedObject.ApplyModifiedProperties();
		}

		private void OnDisable()
		{
			BehaviourEditorWindow.Save(null);
		}
	}
}
#endif
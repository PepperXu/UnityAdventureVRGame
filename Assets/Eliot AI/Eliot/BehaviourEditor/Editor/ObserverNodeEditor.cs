#if UNITY_EDITOR
using System;
using Eliot.AgentComponents;
using UnityEditor;
using UnityEngine;

namespace Eliot.BehaviourEditor.Editor
{
	/// <summary>
	/// Editor extention for ObserverNode objects.
	/// </summary>
	[CustomEditor(typeof(ObserverNode))]
	public class ObserverNodeEditor : UnityEditor.Editor
	{
		/// Index of the function from the function group that the node holds information about.
		private SerializedProperty _funcIndex;
		/// The conditions group of this node that reflects the node's functionality spectre.
		private SerializedProperty _conditionGroup;
		/// The name of the method represented by the Node.
		private SerializedProperty _functionName;
		/// A list of names of functions in the current function group of the node.
		private string[] _options;

        private SerializedProperty _minTime;
        private SerializedProperty _maxTime;

		private static System.DateTime _lastDateTime = System.DateTime.Now;
		private const int ValuesUpdatePing = 50;

		/// <summary>
        /// Get the array of method names for appropriate action group.
        /// </summary>
        private void GetFunctions()
		{
			_options = AgentFunctions.GetConditionStrings(_conditionGroup.enumValueIndex);
			Array.Sort(_options, StringComparer.InvariantCulture);
			((ObserverNode) target).FuncNames = _options;
		}
		
		/// <summary>
		/// This function is called when the object is loaded.
		/// </summary>
		private void OnEnable()
		{
			_funcIndex = serializedObject.FindProperty("_funcIndex");
			_conditionGroup = serializedObject.FindProperty("_conditionGroup");
			_functionName = serializedObject.FindProperty("_functionName");
            _minTime = serializedObject.FindProperty("_minTime");
            _maxTime = serializedObject.FindProperty("_maxTime");
            GetFunctions();
		}
		
		/// <summary>
		/// Draw the new inspector properties.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			serializedObject.Update();
			GetFunctions();

            if (_conditionGroup.enumValueIndex != (int)ConditionGroup.Time)
            {
                GetFunctions();
                EditorGUIUtility.fieldWidth = 150;
                _funcIndex.intValue = EditorGUILayout.Popup("Condition", _funcIndex.intValue, _options);
                _functionName.stringValue = _options[_funcIndex.intValue];
            }
            else
            {
                var prevMinTime = _minTime.floatValue;
                var prevMaxTime = _maxTime.floatValue;

                _minTime.floatValue = EditorGUILayout.FloatField("Min time", _minTime.floatValue);
                _maxTime.floatValue = EditorGUILayout.FloatField("Max time", _maxTime.floatValue);
	            
	            if (System.DateTime.Now.Subtract(_lastDateTime) >= System.TimeSpan.FromMilliseconds(ValuesUpdatePing))
	            {
		            if (_minTime.floatValue > prevMaxTime)
			            _maxTime.floatValue = _minTime.floatValue;
		            if (_maxTime.floatValue < prevMinTime)
			            _minTime.floatValue = _maxTime.floatValue;
		            
		            _lastDateTime = System.DateTime.Now;
	            }
            }

            serializedObject.ApplyModifiedProperties();
		}

		private void On()
		{
			if (_conditionGroup.enumValueIndex == (int) ConditionGroup.Time)
			{
				var prevMinTime = _minTime.floatValue;
				var prevMaxTime = _maxTime.floatValue;
				
				if (_minTime.floatValue > prevMaxTime)
					_maxTime.floatValue = _minTime.floatValue;
				if (_maxTime.floatValue < prevMinTime)
					_minTime.floatValue = _maxTime.floatValue;
			}
		}
	}
}
#endif
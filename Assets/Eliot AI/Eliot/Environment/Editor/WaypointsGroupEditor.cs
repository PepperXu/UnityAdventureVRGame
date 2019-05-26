#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Eliot.AgentComponents;
using UnityEditor;
using UnityEngine;

namespace Eliot.Environment.Editor
{
	/// <summary>
	/// Editor extention for WaypointsGroup objects.
	/// </summary>
	[CustomEditor(typeof(WaypointsGroup))]
	[CanEditMultipleObjects]
	[Serializable]
	public class WaypointsGroupEditor : UnityEditor.Editor
	{
		/// Specify the radius at which to set waypoints.
		[SerializeField] private float _radius = 1;
		/// Specify the number of waypoints to create.
		[SerializeField] private int _waypointsNum = 5;
		/// Specify the number of Agents to create.
		[SerializeField] private int _agentsNumber;
		/// Wheather or not to set this waypoints group at a created Agents' one.
		[SerializeField] private bool _setThisAsAgentsWaypoints = true;
		/// Wheather or not to put created Agents inside.
		[SerializeField] private bool _putAgentsInside = true;
		/// Specify the Agent prefab to create. If none is specified,
		/// random one from the array of Agents will be picked.
		[SerializeField] private GameObject _agentPrefab;
		/// Keep the record of created Agents. 
		[SerializeField] private List<GameObject> _spawnedAgents = new List<GameObject>();
		/// The object that can be a parent to newly created Agents.
		private GameObject _agentsParent;
		/// Wheather or not the user is currently placing waypoints.
		private bool _waypointsPlacementMode;
		
		/// <summary>
		/// This function is called when the object is loaded.
		/// </summary>
		private void OnEnable()
		{
			hideFlags = HideFlags.DontSave;
		}
		
		/// <summary>
		/// Draw the new inspector properties.
		/// </summary>
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			serializedObject.Update();
			
			if (GUILayout.Button("Project On Plane"))
			{
				RaycastHit hit;
				var transform = Selection.activeTransform;
				var ray = new Ray(transform.position, -transform.up);
				if (Physics.Raycast(ray, out hit))
					Selection.activeTransform.position = hit.point;
				else
				{
					ray = new Ray(transform.position, transform.up);
					if (Physics.Raycast(ray, out hit))
						Selection.activeTransform.position = hit.point;
				}
			}
			
			EditorGUILayout.Space();
			
			_waypointsPlacementMode = GUILayout.Toggle(_waypointsPlacementMode, "Place Waypoints", GUI.skin.button);
			serializedObject.ApplyModifiedProperties();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Set Waypoints Number");
			_waypointsNum = EditorGUILayout.IntField(_waypointsNum);
			if (GUILayout.Button("Set Number"))
				Selection.activeGameObject.GetComponent<WaypointsGroup>().SetWaypointsNumber(_waypointsNum, _radius);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Set On Radius");
			_radius = EditorGUILayout.FloatField(_radius);
			if (GUILayout.Button("Set"))
				Selection.activeGameObject.GetComponent<WaypointsGroup>().SetWaypointsAtRadius(_radius);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			if (GUILayout.Button("Clear Waypoints"))
				Selection.activeGameObject.GetComponent<WaypointsGroup>().Clear();
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
			var labelSkin = GUI.skin.label;
			labelSkin.richText = true;
			EditorGUILayout.TextArea("<size=10>Specify the Agent prefab to create. If none is specified,\n" +
			                         " random one from the array of Agents will be picked.</size>", labelSkin);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Agent Prefab");
			_agentPrefab = (GameObject)EditorGUILayout.ObjectField(_agentPrefab, typeof(GameObject), false);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Agents Number");
			_agentsNumber = EditorGUILayout.IntField(_agentsNumber);
			EditorGUILayout.EndHorizontal();
			_setThisAsAgentsWaypoints = GUILayout.Toggle(_setThisAsAgentsWaypoints, "Set This As Waypoints");
			_putAgentsInside = GUILayout.Toggle(_putAgentsInside, "Put Agents Inside");
			if (GUILayout.Button("Mass Place Agents"))
			{
				if (_putAgentsInside)
					_agentsParent = Selection.activeGameObject.GetComponent<WaypointsGroup>().AgentsParent();
				if(_agentsNumber > 0)
					for (var i = 0; i < _agentsNumber; i++)
					{
						var prefab = _agentPrefab == null ? 
							Selection.activeGameObject.GetComponent<WaypointsGroup>().RandomAgentFromPool() : _agentPrefab;
						var position = Selection.activeGameObject.GetComponent<WaypointsGroup>().RandomPoint();
						var rotation = new Quaternion(0, UnityEngine.Random.Range(0, 2*Mathf.PI), 0, 1);
						var newAgent = Instantiate(prefab, position, rotation) as GameObject;
						var agent = newAgent.GetComponent<Agent>();
						if (agent && _setThisAsAgentsWaypoints)
						{
							agent.Waypoints = Selection.activeGameObject.GetComponent<WaypointsGroup>();
						}
						_spawnedAgents.Add(newAgent);
						
						if (_putAgentsInside)
							newAgent.transform.parent = _agentsParent.transform;
					}
			}
			if (GUILayout.Button("Clear Agents"))
			{
				var agentsParent = Selection.activeTransform.Find("__agents__");
				if (agentsParent)
				{
					for (var i = agentsParent.childCount - 1; i >= 0; i--)
						DestroyImmediate(agentsParent.GetChild(i).gameObject);
				}
				else
				{
					if (_spawnedAgents.Count > 0)
						for (var i = _spawnedAgents.Count - 1; i >= 0; i--)
							DestroyImmediate(_spawnedAgents[i]);
				}
			}
		}
		
		/// <summary>
		/// Enables the Editor to handle an event in the scene view.
		/// </summary>
		private void OnSceneGUI()
		{
			if (_waypointsPlacementMode)
			{
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0
				    && !Event.current.command && !Event.current.control && !Event.current.alt)
				{
					var obj = Selection.activeObject;
					RaycastHit hit;
					var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					if (Physics.Raycast(ray, out hit))
					{
						var position = hit.point;

						var newWaypoint = new GameObject("Waypoint[" 
						                                 + Selection.activeGameObject
							                                 .GetComponent<WaypointsGroup>().WaypointsNumber() + "]");
						newWaypoint.transform.position = position;
						newWaypoint.transform.parent = Selection.activeTransform;
						newWaypoint.AddComponent<Waypoint>();
					}

					Selection.activeObject = obj;
					Event.current.Use();
				}
			}
		}
	}
}
#endif
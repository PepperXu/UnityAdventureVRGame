using System;
using System.Collections.Generic;
using Eliot.AgentComponents;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace Eliot.Environment
{
	/// <summary>
	/// A controller object for a set of Waypoints that can be used by Agents as an area or a path.
	/// </summary>
	public class WaypointsGroup : MonoBehaviour
	{
		/// <summary>
		/// A list of all the waypoints in the group.
		/// </summary>
		public List<Waypoint> Points
		{
			get { return _points; }
		}

		/// <summary>
		/// Distance on which an Agent is concidered to be in the range of a waypoint of this group.
		/// </summary>
		public float ThresholdDistance
		{
			get { return _thresholdDistance; }
		}

		[Header("Customization")]
		[Tooltip("Use colors to recognize the utility of this waypoints group with a single glance.")]
		[SerializeField] private WaypointsColors _colors;
		[Tooltip("Distance on which an Agent is concidered to be in the range of a waypoint of this group.")]
		[SerializeField] private float _thresholdDistance = 1f;

		[Header("Pool")] 
		[Tooltip("Wheather this waypoints group should replenish the number of its population of Agents.")]
		[SerializeField] private bool _poolAgents;
		[Tooltip("Number of Agents at which no new Agents will be instantiated.")]
		[SerializeField] private int _maxAgentsNumber;
		[Tooltip("Defines how often new Agents will be instantiated.")]
		[SerializeField] private float _agentsPoolCoolDown = 30f;
		[Tooltip("Random object from this list will be instantiated as a group's Agent.")]
		[SerializeField] private GameObject[] _pooledAgentsPrefabs;
		
		/// The last time the waypoints grop instantiated an Agent.
		private float _lastTimePooledAgent;
		/// A list of all the waypoints in the group.
		private List<Waypoint> _points = new List<Waypoint>();
		/// Link to the GameObject that holds Agents as its children.
		private GameObject _agentsParent;
		
#if UNITY_EDITOR
		/// <summary>
		/// Instantiate a new Waypoints Group from the Editor.
		/// </summary>
		[MenuItem("Eliot/Create/New Waypoints Group")]
		private static void NewWaypointsGroup()
		{
			var waypointsObj = new GameObject("New Waypoints Group");
			waypointsObj.transform.position = SceneView.lastActiveSceneView.pivot;
			waypointsObj.AddComponent<WaypointsGroup>();
			Selection.activeGameObject = waypointsObj;
		}
#endif
		
		/// <summary>
		/// Initialize the components on the loading of the scene.
		/// </summary>
		private void Start()
		{
			_points = new List<Waypoint>();
			var wpts = GetComponentsInChildren<Waypoint>();
			foreach (var child in wpts)
				_points.Add(child);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Draw helper objects for the group object as well as for each waypoint in the group.
		/// </summary>
		private void OnDrawGizmos()
		{
			// Draw label.
			var style = new GUIStyle();
			try
			{ style.normal.textColor = _colors.OriginColor; }
			catch (Exception){/*Dont worry*/}

			Handles.BeginGUI();
			var pos = transform.position + transform.up * 0.5f;
			var pos2D = HandleUtility.WorldToGUIPoint(pos);
			GUI.Label(new Rect(pos2D.x + 10, pos2D.y - 10, 100, 100), name, style);
			Handles.EndGUI();
			
			// Draw gizmos.
			try{Gizmos.color = _colors.OriginColor;}catch (Exception){/*OK*/}
			Gizmos.DrawWireSphere(transform.position, 0.3f);
			var points = new List<Transform>();
			var index = 0;
			foreach (Transform point in transform)
			{
				if (point.GetComponent<Waypoint>())
				{
					Handles.color = _colors.LineColor;
					Handles.DrawDottedLine(transform.position, point.position, 1f);
					points.Add(point);
					point.GetComponent<Waypoint>().DrawCircle(_colors.WaypointColor);
					point.GetComponent<Waypoint>().DrawIndex(index++, _colors.WaypointColor);
				}
			}

			if (points.Count <= 0) return;
			for (var i = 0; i < points.Count; i++)
			{
				Handles.color = _colors.WaypointColor;
				var i_e = i + 1 == points.Count ? 0 : i + 1;
				Handles.DrawLine(points[i].position, points[i_e].position);
			}
		}
#endif
		
		/// <summary>
		/// Get a collection of all triangles of the mesh that cansists of Waypoints.
		/// </summary>
		/// <returns></returns>
		private Vector3[][] AllTriangles()
		{
			if (!transform) return null;
			var triangles = new List<Vector3[]>();
			var waypoints = new List<Vector3>();
			var number = 0;
			foreach (Transform child in transform)
				if (child.GetComponent<Waypoint>())
				{
					waypoints.Add(child.position);
					number++;
				}
			if (number == 0) return null;

			for (var i = 0; i < number; i++)
			{
				var a = i;
				var b = a + 1 >= number ? 0 : a + 1;
				triangles.Add(new List<Vector3>
				{
					waypoints[a],
					waypoints[b]
				}.ToArray());
			}

			return triangles.ToArray();
		}
		
		/// <summary>
		/// Pick a legit random triple of points, one of which is the group object itself.
		/// </summary>
		/// <returns></returns>
		private Vector3[] RandomTriangle()
		{
			if (!transform) return null;
			var waypoints = new List<Vector3>();
			var number = 0;
			foreach (Transform child in transform)
				if (child.GetComponent<Waypoint>())
				{
					waypoints.Add(child.position);
					number++;
				}
			if (number == 0) return null;
			
			var randomA = Random.Range(0, number);
			var randomB = randomA + 1 >= number ? 0 : randomA + 1;
			return new List<Vector3>
			{
				waypoints[randomA],
				waypoints[randomB]
			}.ToArray();
		}
		
		/// <summary>
		/// Calculate a random point inside a randomly picked triangle.
		/// </summary>
		/// <returns></returns>
		public Vector3 RandomPoint()
		{
			var triangle = RandomTriangle();
			var r1 = Random.Range(0f, 1f);
			var r2 = Random.Range(0f, 1f);
			var a = transform.position;
			var b = triangle[0];
			var c = triangle[1];
			return (1f - Mathf.Sqrt(r1))*a + (Mathf.Sqrt(r1)*(1f - r2))*b + (r2*Mathf.Sqrt(r1))*c;
		}

		/// <summary>
		/// Set all the waypoints uniformly around the group object and set them at specific radius.
		/// </summary>
		/// <param name="radius"></param>
		public void SetWaypointsAtRadius(float radius)
		{
			var number = 0;
			foreach (Transform child in transform)
				if (child.GetComponent<Waypoint>())
					number++;
			if (number == 0) return;
			var delta = 2*Mathf.PI / number;
			var fi = 0f;
			foreach (Transform point in transform)
			{
				if (!point.GetComponent<Waypoint>()) continue;
				fi += delta;
				var x = radius * Mathf.Cos(fi);
				var z = radius * Mathf.Sin(fi);
				point.position = transform.position + new Vector3(x, 0, z);
			}
		}

		/// <summary>
		/// Remove all waypoints from the group.
		/// </summary>
		public void Clear()
		{
			var number = transform.childCount;
			if (number == 0) return;
			for (var i = transform.childCount-1; i >= 0; i--)
			{
				var child = transform.GetChild(i);
				if(child.GetComponent<Waypoint>())
					DestroyImmediate(child.gameObject);
			}
		}

		/// <summary>
		/// Clear the group, create specific number of waypoints and set them at specific radius.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="radius"></param>
		public void SetWaypointsNumber(int number, float radius)
		{
			Clear();
			if (number == 0) return;
			for (var i = 0; i < number; i++)
			{
				var newWaypoint = new GameObject("Waypoint[" + i + "]");
				newWaypoint.transform.position = transform.position;
				newWaypoint.transform.parent = transform;
				newWaypoint.AddComponent<Waypoint>();
			}

			SetWaypointsAtRadius(radius);
		}

		/// <summary>
		/// Get the child that holds all the group's Agents or create one.
		/// </summary>
		/// <returns></returns>
		public GameObject AgentsParent()
		{
			if (!_agentsParent)
			{
				if (transform.Find("__agents__"))
				{
					_agentsParent = transform.Find("__agents__").gameObject;
				}
#if UNITY_EDITOR
				else
				{
					_agentsParent = new GameObject("__agents__");
					_agentsParent.transform.parent = Selection.activeTransform;
					_agentsParent.transform.localPosition = Vector3.zero;
				}
#endif
			}

			return _agentsParent;
		}

		/// <summary>
		/// Return the current number of Agents in the group.
		/// </summary>
		/// <returns></returns>
		public int AgentsNumber()
		{
			return AgentsParent().transform.childCount;
		}

		/// <summary>
		/// Return the current numberof waypoints in the group.
		/// </summary>
		/// <returns></returns>
		public int WaypointsNumber()
		{
			return transform.GetComponentsInChildren<Waypoint>().Length;
		}

		/// <summary>
		/// Pick a random Agent from the array of possible Agents.
		/// </summary>
		/// <returns></returns>
		public GameObject RandomAgentFromPool()
		{
			return _pooledAgentsPrefabs[Random.Range(0, _pooledAgentsPrefabs.Length)];
		}

		/// <summary>
		/// Instantiate a new Agent at a random position inside the group's area.
		/// </summary>
		private void SpawnAgent()
		{
			if (_pooledAgentsPrefabs.Length == 0) return;
			var position = RandomPoint();
			var rotation = new Quaternion(0, Random.Range(0, 2*Mathf.PI), 0, 1);
			var newAgent = Instantiate(RandomAgentFromPool(), position, rotation) as GameObject;
			var agent = newAgent.GetComponent<Agent>();
			if (agent)
			{
				agent.Waypoints = GetComponent<WaypointsGroup>();
			}
			newAgent.transform.parent = AgentsParent().transform;
		}

		#region CheckIfPointIsInside

		private static float Sign (Vector3 p1, Vector3 p2, Vector3 p3)
		{
			return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
		}

		private static bool PointInTriangle (Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
		{
			bool b1, b2, b3;

			b1 = Sign(pt, v1, v2) < 0.0f;
			b2 = Sign(pt, v2, v3) < 0.0f;
			b3 = Sign(pt, v3, v1) < 0.0f;

			return ((b1 == b2) && (b2 == b3));
		}
		
		public bool IsInsidePolygon(Vector3 point)
		{
			foreach (var triangle in AllTriangles())
				if (PointInTriangle(point, transform.position, triangle[0], triangle[1]))
					return true;

			return false;
		}

		#endregion
		
		/// <summary>
		/// Update the object every frame.
		/// </summary>
		private void Update()
		{
			if (_poolAgents && AgentsNumber() == _maxAgentsNumber )
			{
				_lastTimePooledAgent = Time.time;
			}
			if (_poolAgents && AgentsNumber() < _maxAgentsNumber 
			                && Time.time > _lastTimePooledAgent + _agentsPoolCoolDown)
			{
				_lastTimePooledAgent = Time.time;
				SpawnAgent();
			}
		}
		
		/// <summary>
		/// Overload the subscription of the object.
		/// </summary>
		/// <param name="index"></param>
		public Waypoint this[int index]
		{
			get
			{
				if (index + 1 >= Points.Count) index = 0;
				if (index < 0) index = Points.Count + index;
				return transform.GetComponentsInChildren<Waypoint>()[index];
			}
		}
	}
}
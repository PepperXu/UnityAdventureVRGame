using Eliot.BehaviourEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Eliot.Environment
{
	/// <summary>
	/// One of the waypoints in the group. Can define one of the border points
	/// of the group or a target position for the Agent. Can Interact with Agent
	/// changing his parameters if both Agent and Waypoint agree on that.
	/// </summary>
	public class Waypoint : MonoBehaviour
	{
		/// <summary>
		/// Wheather the waypoint will apply its changes to an Agent.
		/// </summary>
		public bool MakeChangesToAgent
		{
			get { return _makeChangesToAgent; }
		}

		/// <summary>
		/// A new Behaviour model that can be applied to an Agent.
		/// </summary>
		public EliotBehaviour NewBehaviour
		{
			get { return _newBehaviour; }
		}

		/// <summary>
		/// A reference to a new Waypoints Group that can be applied to an Agent.
		/// </summary>
		public WaypointsGroup NewWaypoints
		{
			get { return _newWaypoints; }
		}

		[Tooltip("Wheather the waypoint will apply its changes to an Agent.")]
		[SerializeField] private bool _makeChangesToAgent;
		[Space(10)]
		[Tooltip("A new Behaviour model that can be applied to an Agent.")]
		[SerializeField] private EliotBehaviour _newBehaviour;
		[Tooltip("A reference to a new Waypoints Group that can be applied to an Agent.")]
		[SerializeField] private WaypointsGroup _newWaypoints;
		
#if UNITY_EDITOR
		/// <summary>
		/// Draw a circle with Handles with specific color.
		/// </summary>
		/// <param name="color"></param>
		public void DrawCircle(Color color)
		{
			DrawDoubleArc(color, 360, transform, 0.5f);
		}

		/// <summary>
		/// Draw a label specifying the waypoint's index in the group.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="color"></param>
		public void DrawIndex(int index, Color color)
		{
			Drawlabel(index.ToString(), color, 0);
		}
		
		#region UTILITY
		
		/// <summary>
		/// Use Handles to draw a label.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <param name="yOffset"></param>
		private void Drawlabel(string text, Color color, float yOffset) {
			var style = new GUIStyle {normal = {textColor = color}};
			Handles.BeginGUI();
			var pos = transform.position;
			var pos2D = HandleUtility.WorldToGUIPoint(pos);
			GUI.Label(new Rect(pos2D.x, pos2D.y + yOffset, 100, 100), text, style);
			Handles.EndGUI();
		}

		/// <summary>
		/// Use handles to draw to arcs in a row wint slightly differens radiuses.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="fov"></param>
		/// <param name="origin"></param>
		/// <param name="range"></param>
		private static void DrawDoubleArc(Color color, float fov, Transform origin, float range)
		{
			var startColor = Handles.color;
			Handles.color = color;
			Handles.DrawWireArc(origin.position, origin.up, -origin.right, fov, range);
			Handles.DrawWireArc(origin.position, origin.up, -origin.right, fov, range-0.05f);
			Handles.color = startColor;
		}
		#endregion
#endif
	}
}
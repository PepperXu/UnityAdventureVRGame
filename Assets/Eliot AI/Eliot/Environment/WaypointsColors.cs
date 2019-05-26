using System;
using UnityEngine;

namespace Eliot.Environment
{
	/// <summary>
	/// Holds the customization information for each WaypointsGroup.
	/// </summary>
	[Serializable] public class WaypointsColors
	{
		/// <summary>
		/// Color of each waypoint object.
		/// </summary>
		public Color WaypointColor = Color.white;
		
		/// <summary>
		/// Color of the holder object of the waypoints in the group.
		/// </summary>
		public Color OriginColor = Color.white;
		
		/// <summary>
		/// Color of connections between waypoints.
		/// </summary>
		public Color LineColor = Color.white;
	}
}

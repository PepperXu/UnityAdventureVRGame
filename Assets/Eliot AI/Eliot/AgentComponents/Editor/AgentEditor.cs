#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Eliot.AgentComponents.Editor
{
	/// <summary>
	/// Editor extention for Agent objects.
	/// </summary>
	[CustomEditor(typeof(Agent))]
	[CanEditMultipleObjects]
	public class AgentEditor : UnityEditor.Editor
	{
		/// Link to the Agent component.
		private Agent _agent;
		/// Wheather the Agent is inert.
		private SerializedProperty _inert;

		/// <summary>
		/// This function is called when the object is loaded.
		/// </summary>
		private void OnEnable()
		{
			_inert = serializedObject.FindProperty("_inert");
		}

		/// <summary>
		/// Draw the new inspector properties.
		/// </summary>
		public override void OnInspectorGUI()
		{
			if (_inert.boolValue)
			{
				EditorGUILayout.BeginHorizontal();
				_inert.boolValue = EditorGUILayout.Toggle("Inert", _inert.boolValue);
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				DrawDefaultInspector();
			}
			serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Enables the Editor to handle an event in the scene view.
		/// </summary>
		public void OnSceneGUI()
		{
			_agent = target as Agent;
			if (_agent == null) return;

			OnSceneGUI_Resources();
			OnSceneGUI_Perception();
			OnSceneGUI_Motion();
			OnSceneGUI_Settings();
			OnSceneGUI_Skill();
			OnSceneGUI_Waypoints();
			OnSceneGUI_Target();
		}

		#region HELPERS

		/// <summary>
		/// Draw GUI for Resources.
		/// </summary>
		private void OnSceneGUI_Resources()
		{
			var resources = _agent.Resources;
			if (!resources.Debug) return;
			
			var healthLabel = _agent.Resources.UsesHealth ? ("Max health: " + _agent.Resources.MaxHealthPoints) : "";
			var curHealthLabel = _agent.Resources.UsesHealth ? ("Health: " + _agent.Resources.HealthPoints) : "";
			var energyLabel = _agent.Resources.UsesEnergy ? ("Max energy: " + _agent.Resources.MaxEnergyPoints) : "";
			var curEnergyLabel = _agent.Resources.UsesEnergy ? ("Energy: " + _agent.Resources.EnergyPoints) : "";
			Drawlabel(healthLabel, Color.white, -30f);
			Drawlabel(curHealthLabel, Color.white, -15f);
			Drawlabel(energyLabel, Color.white, 0.0f);
			Drawlabel(curEnergyLabel, Color.white, 15f);
		}
		
		/// <summary>
		/// Draw GUI for Perception.
		/// </summary>
		private void OnSceneGUI_Perception()
		{
			var eye = _agent.Perception;
			if (!eye.Debug) return;
			
			Handles.color = new Color(1f,1f,1f,0.3f);
			if (eye.Resolution == 0) return;
			var rotation = eye.Offset;
			var fieldOfView = eye.FieldOfView;
			var resolution = eye.Resolution;
			var range = eye.Range;

			var n = resolution;
			var delta = fieldOfView / (n-1);
			var origin = eye.Origin==null ? _agent.transform : eye.Origin;
			var vectors = new List<Vector3>();
			
			if (resolution % 2 != 0)
			{
				var rot = delta;
				
				vectors = new List<Vector3> {InitRay(range, 0, rotation, origin)};
				for (var i = 0; i < n / 2; i++)
				{
					vectors.Add(InitRay(range, -rot, rotation, origin));
					vectors.Add(InitRay(range, rot, rotation, origin));
					rot += delta;
				}
			}
			else
			{
				for (var i = 0; i < resolution; i++)
				{
					if (i % 2 == 0) continue;
					vectors.Add(InitRay(range, -i * delta / 2, rotation, origin));
					vectors.Add(InitRay(range, i * delta / 2, rotation, origin));
				}
			}
			
			foreach (var vec in vectors)
				Handles.DrawDottedLine(origin.position, vec, 2f);

			Handles.color = Color.white;

			var arcT = new GameObject("__arc_t__");
			arcT.transform.position = origin.position;
			arcT.transform.rotation = origin.rotation;
			arcT.transform.Rotate(arcT.transform.up, 90-fieldOfView/2 + 90-rotation);
			var arcRot = -arcT.transform.right;
			DestroyImmediate(arcT);
			
			Handles.DrawWireArc(origin.position, origin.up, arcRot, fieldOfView, range);
			
			Handles.DrawWireArc(origin.position, origin.up, -origin.right, 360, eye.LookAroundRange);
			Handles.DrawWireArc(origin.position, origin.up, -origin.right, 360, eye.LookAroundRange-0.05f);

			DrawDoubleSphere(Color.white, eye.Origin, 0.1f);
		}

		/// <summary>
		/// Draw GUI for Motion.
		/// </summary>
		private void OnSceneGUI_Motion()
		{
			var motion = _agent.Motion;
			if (!motion.Debug) return;
			
			var stateLabel = "Motion state: " + motion.State;
			Drawlabel(stateLabel, Color.white, 50f);
		}

		/// <summary>
		/// Draw GUI for Settings.
		/// </summary>
		private void OnSceneGUI_Settings()
		{
			var stats = _agent.GeneralSettings;
			if (!stats.Debug) return;
			var origin = _agent.Perception.Origin==null ? _agent.transform : _agent.Perception.Origin;
			
			DrawDoubleArc(Color.red, stats.AimFieldOfView, origin, stats.CloseDistance);
			DrawDoubleArc(Color.yellow, stats.BackFieldOfView, origin, _agent.Perception.LookAroundRange+0.1f, 180);

			Handles.color = new Color(1f, 1f, 1f, 0.5f);
			Handles.DrawWireArc(origin.position, origin.up, -origin.right, 360, stats.MidDistance);
			
			Handles.color = new Color(1f, 1f, 1f, 0.25f);
			Handles.DrawWireArc(origin.position, origin.up, -origin.right, 360, stats.FarDistance);
			
			DrawDoubleSphere(Color.red, stats.ShootOrigin ?? origin, 0.15f);
		}
		
		/// <summary>
		/// Draw GUI for Skill.
		/// </summary>
		private void OnSceneGUI_Skill()
		{
			if (!_agent.DebugSkill) return;
			var skill = _agent.CurrentSkill;
			if (!skill) return;

			var skillLabel = "Skill (" + skill.name + "): " + skill.State;
			Drawlabel(skillLabel, Color.white, 65f);
		}
		
		/// <summary>
		/// Draw GUI for Waypoints Group.
		/// </summary>
		private void OnSceneGUI_Waypoints()
		{
			if (!_agent.DebugWaypoints) return;
			var waypoints = _agent.Waypoints;
			if (!waypoints) return;

			var waypointsLabel = "Area: " + waypoints.name;
			Drawlabel(waypointsLabel, Color.white, 80f);
		}
		
		/// <summary>
		/// Draw GUI for Agent's Target.
		/// </summary>
		private void OnSceneGUI_Target()
		{
			if (!_agent.DebugTarget) return;
			var targ = _agent.Target;
			if (!targ) return;

			var waypointsLabel = "Target: " + targ.name;
			Drawlabel(waypointsLabel, Color.white, 95f);
		}
		
		#endregion
		
		#region UTILITY

		/// <summary>
		/// Return a vector that points to a specific position.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="fi"></param>
		/// <param name="offset"></param>
		/// <param name="center"></param>
		/// <returns></returns>
		private static Vector3 InitRay(float r, float fi, float offset, Transform center)
		{
			var offsetRads = offset * Mathf.PI / 180;
			fi -= center.eulerAngles.y;
			var rads = fi * Mathf.PI / 180;
			rads = rads%(Mathf.PI*2) + offsetRads%(Mathf.PI*2);
			var x = center.position.x + r * Mathf.Cos(rads);
			var z = center.position.z + r * Mathf.Sin(rads);
			return new Vector3(x, center.position.y, z);
		}
		
		/// <summary>
		/// Use handles to draw a label.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <param name="yOffset"></param>
		private void Drawlabel(string text, Color color, float yOffset) {        
			var style = new GUIStyle();
			style.normal.textColor = color;
			Handles.BeginGUI();
			var pos = _agent.transform.position + _agent.transform.up * _agent.transform.localScale.magnitude*1.5f;
			var pos2D = HandleUtility.WorldToGUIPoint(pos);
			GUI.Label(new Rect(pos2D.x + 100, pos2D.y + yOffset, 100, 100), text, style);
			Handles.EndGUI();
		}

		/// <summary>
		/// Use Handles to draw two arcs in a row with slightly different radiuses.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="fov"></param>
		/// <param name="origin"></param>
		/// <param name="range"></param>
		/// <param name="offset"></param>
		private void DrawDoubleArc(Color color, float fov, Transform origin, float range, float offset = 0)
		{
			var startColor = Handles.color;
			
			var arcT = new GameObject("__arc_t__");
			arcT.transform.position = origin.position;
			arcT.transform.rotation = origin.rotation;
			arcT.transform.Rotate(arcT.transform.up, 90-fov/2 + 90-_agent.Perception.Offset + offset);
			var arcRot = -arcT.transform.right;
			DestroyImmediate(arcT);
			
			Handles.color = color;
			Handles.DrawWireArc(origin.position, origin.up, arcRot, fov, range);
			Handles.DrawWireArc(origin.position, origin.up, arcRot, fov, range-0.05f);

			var startAim = InitRay(range, fov/2, _agent.Perception.Offset + offset, origin);
			var endAim = InitRay(range, -fov/2, _agent.Perception.Offset + offset, origin);
			Handles.DrawDottedLine(origin.position, startAim, 1f);
			Handles.DrawDottedLine(origin.position, endAim, 1f);

			Handles.color = startColor;
		}

		/// <summary>
		/// Use handles to draw a wired sphere.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="origin"></param>
		/// <param name="radius"></param>
		private static void DrawSphere(Color color, Transform origin, float radius)
		{
			var startColor = Handles.color;
			Handles.color = color;
			try{
			Handles.DrawWireArc(origin.position, origin.up, -origin.right, 360, radius);
			Handles.DrawWireArc(origin.position, origin.forward, -origin.right, 360, radius);
			Handles.DrawWireArc(origin.position, origin.right, -origin.forward, 360, radius);
			}
			catch(System.Exception){}
			Handles.color = startColor;
		}

		/// <summary>
		/// Draw two wired spheres with slightly different radiuses.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="origin"></param>
		/// <param name="radius"></param>
		private static void DrawDoubleSphere(Color color, Transform origin, float radius)
		{
			DrawSphere(color, origin, radius);
			DrawSphere(color, origin, radius+0.005f);
		}

		#endregion
		
	}
}
#endif
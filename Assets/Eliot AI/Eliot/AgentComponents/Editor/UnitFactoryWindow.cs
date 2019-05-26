#if UNITY_EDITOR
#pragma warning disable CS0414, CS1692
using System.Collections.Generic;
using Eliot.Environment;
using UnityEditor;
using UnityEngine;
using Eliot.BehaviourEditor;

namespace Eliot.AgentComponents.Editor
{
    /// <summary>
    /// Editor window that helps constructing new Agents.
    /// </summary>
    public class UnitFactoryWindow : EditorWindow
    {
        /// Name of an Agent.
        private string _name = "";
        /// Team of an Agent.
        private string _team = "";
        /// Team of an Agent.
        private float _weight = 1;
        /// How far can Agent see.
        private float _seeDistance = 10;
        /// Field of view of Agent's perception.
        private float _seeFOV = 180;
        /// Resolution of Agent's perception.
        private UnitFactoryOptions _seeResolution = UnitFactoryOptions.Medium;
        /// Wheather the Agent can move.
        private bool _canMove = true;

        private MotionEngine _motionEngine;
        /// Speed with which the Agent can move.
        private UnitFactoryOptions _moveSpeed = UnitFactoryOptions.Medium;
        /// Wheather the Agent can run.
        private bool _canRun = true;
        /// Wheather the Agent can dodge.
        private bool _canDodge;
        /// For how long can Agent remember his targets.
        private int _memoryDuration = 60;
        /// Wheather the Agent uses health as a resource.
        private bool _isMortal = true;
        /// Maximum health capacity.
        private int _maxHealth = 10;
        /// Wheather the Agent uses energy as a resource.
        private bool _useEnergy;
        /// Maximum energy capacity.
        private int _maxEnergy = 10;
        /// Agent's graphics.
        private GameObject _graphics;
        /// Agent's ragdoll.
        private GameObject _ragdoll;
        /// Agent's Behaviour model.
        private EliotBehaviour _behaviour;
        /// Agent's waypoints group.
        private WaypointsGroup _waypoints;
        /// Agent's initial skills.
        private List<Skill> _skills = new List<Skill>();
        /// Position of the scroll rect.
        private static Vector2 _scrollPosition = Vector2.zero;
        
        #region INTERNAL ENUMS
        /// <summary>
        /// Enumerates options for some factory parameters.
        /// </summary>
        private enum UnitFactoryOptions {Low, Medium, High}
        #endregion
        
        /// <summary>
        /// Initialize new factory window.
        /// </summary>
        [MenuItem("Eliot/Unit Factory")]
        private static void InitWindow()
        {
            GetWindowWithRect<UnitFactoryWindow>(new Rect(750, 250, 350, 550), true, "Unit Factory");
        }

        /// <summary>
        /// Draw all the window graphics.
        /// </summary>
        private void OnGUI()
        {
            //GUI.DrawTexture(new Rect(0,0,350,100), _unitFactoryLogo);
            //Space(15);
            Tip("Create new Agent here giving some general idea of his \nparameters." +
                "You can always adjust them in the inspector \nof already created Agent.");
            Space();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            Header("General");
            _name = EditorGUILayout.TextField("Agent's name: ", _name);
            _team = EditorGUILayout.TextField("Team: ", _team);
            _weight = EditorGUILayout.FloatField("Weight: ", _weight);
            Space(2);
            Header("Perception");
            _seeDistance = EditorGUILayout.FloatField("Perception range:", _seeDistance);
            _seeFOV = EditorGUILayout.FloatField("Field of view: ", _seeFOV);
            _seeResolution = (UnitFactoryOptions)EditorGUILayout.EnumPopup("Accuracy of perception: ", _seeResolution);
            _memoryDuration = EditorGUILayout.IntField("Memory duration: ", _memoryDuration);
            Space(2);
            Header("Motion");
            _motionEngine = (MotionEngine) EditorGUILayout.EnumPopup("Motion engine: ", _motionEngine);
            //_canMove = EditorGUILayout.Toggle("Should it move?", _canMove);
            _moveSpeed = (UnitFactoryOptions) EditorGUILayout.EnumPopup("Move speed: ", _moveSpeed);
            _canRun = EditorGUILayout.Toggle("Can it run?", _canRun);
            _canDodge = EditorGUILayout.Toggle("Can it dodge?", _canDodge);
            Space(2);
            Header("Resources");
            _isMortal = EditorGUILayout.Toggle("Is it mortal?", _isMortal);
            if(_isMortal)
                _maxHealth = EditorGUILayout.IntField("Health capacity: ", _maxHealth);
            _useEnergy = EditorGUILayout.Toggle("Does it use energy?", _useEnergy);
            if(_useEnergy)
                _maxEnergy = EditorGUILayout.IntField("Energy capacity: ", _maxEnergy);
            Space(2);
            Header("Other");
            _graphics = (GameObject)EditorGUILayout.ObjectField("Graphics: ", _graphics, typeof(GameObject), false);
            _ragdoll = (GameObject)EditorGUILayout.ObjectField("Ragdoll: ", _ragdoll, typeof(GameObject), false);
            _behaviour = (EliotBehaviour)EditorGUILayout.ObjectField("Behaviour: ", _behaviour, typeof(EliotBehaviour), false);
            _waypoints = (WaypointsGroup)EditorGUILayout.ObjectField("Waypoints: ", _waypoints, typeof(WaypointsGroup), true);
            Space(2);
            Header("Skills");
            if(_skills.Count > 0)
            for (var i = _skills.Count-1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                _skills[i] = (Skill) EditorGUILayout.ObjectField(_skills[i], typeof(Skill), false);
                if(GUILayout.Button("Remove"))
                    _skills.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add skill"))
                _skills.Add(null);
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
            {
                //Resources
                var resources = Resources.CreateResources(_isMortal, _maxHealth, _useEnergy, _maxEnergy);
                
                //Memory
                var memory = Memory.CreateMemory(_memoryDuration);
                
                //See
                var resolution = 0;
                switch (_seeResolution)
                {
                    case UnitFactoryOptions.Low: resolution = 7; break;
                    case UnitFactoryOptions.Medium: resolution = 15; break;
                    case UnitFactoryOptions.High: resolution = 35; break;
                }
                var perception = Perception.CreatePerception(_seeDistance, _seeFOV, resolution);
                
                //Move
                var speed = 0f;
                switch (_moveSpeed)
                {
                    case UnitFactoryOptions.Low: speed = 1.5f; break;
                    case UnitFactoryOptions.Medium: speed = 3f; break;
                    case UnitFactoryOptions.High: speed = 5f; break;
                }
                var motion = Motion.CreateMotion(_motionEngine, speed, _canRun, _canDodge);
                motion.Weight = _weight;
                
                //Death
                var death = Death.CreateDeath(_ragdoll);
                
                //Settings
                var stats = Settings.CreateSettings();
                
                var newAgent = Agent.CreateAgent(_name, resources, memory, perception, motion, death, stats, _skills, _graphics);
                newAgent.Behaviour = _behaviour;
                newAgent.GetComponent<Unit>().Team = _team;
                newAgent.Waypoints = _waypoints;
                newAgent.transform.position = SceneView.lastActiveSceneView.pivot;
                
                Selection.activeObject = newAgent;
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Make some space in the editor.
        /// </summary>
        /// <param name="number"></param>
        private static void Space(int number = 1)
        {
            for(var i = 0; i < number; i++) 
                EditorGUILayout.Space();
        }

        /// <summary>
        /// Add text area to the editor with small font size to insert some tips for user.
        /// </summary>
        /// <param name="text"></param>
        private static void Tip(string text)
        {
            var labelSkin = GUI.skin.label;
            labelSkin.richText = true;
            EditorGUILayout.TextArea("<size=10>" + text + "</size>", labelSkin);
        }
        
        /// <summary>
        /// Insert some bold text in the editor.
        /// </summary>
        /// <param name="text"></param>
        private static void Header(string text)
        {
            var labelSkin = GUI.skin.label;
            labelSkin.richText = true;
            EditorGUILayout.TextArea("<b>" + text + "</b>", labelSkin);
        }
    }
}
#endif
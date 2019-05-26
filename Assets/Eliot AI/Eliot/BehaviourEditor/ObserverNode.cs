#if UNITY_EDITOR
using Eliot.AgentComponents;
using Eliot.Repository;
using UnityEditor;
using UnityEngine;

namespace Eliot.BehaviourEditor
{
	/// <summary>
	/// Node that represents the Loop Component of the behaviour model.
	/// </summary>
	public class ObserverNode : Node
	{
		/// <summary>
		/// The conditions group of this node that reflects the node's functionality spectre.
		/// </summary>
		public ConditionGroup ConditionGroup
		{
			get { return _conditionGroup; }
			set { _conditionGroup = value; }
		}

        public float MinTime
        {
            get { return _minTime; }
            set { _minTime = value; }
        }
        public float MaxTime
        {
            get { return _maxTime; }
            set { _maxTime = value; }
        }
        public float RandomTime
        {
            get { return UnityEngine.Random.Range(_minTime, _maxTime); }
        }
        public string FormattedTime
        {
            get { return (_minTime == _maxTime) ? _minTime.ToString() : (_minTime.ToString() + "/" + _maxTime.ToString()); }
        }

        [Tooltip("The conditions group of this node that reflects the node's functionality spectre.")]
		[SerializeField] private ConditionGroup _conditionGroup;

        [HideInInspector] [SerializeField] private float _minTime;
        [HideInInspector] [SerializeField] private float _maxTime;

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ObserverNode(){}
		
		/// <summary>
		/// Initialize the ObserverNode.
		/// </summary>
		/// <param name="rect"></param>
		public ObserverNode(Rect rect) : base(rect, "Observer"){}
		
		/// <summary>
		/// This function is called when the object is loaded.
		/// </summary>
		public new void OnEnable()
		{
			base.OnEnable();
			if(FuncNames == null) FuncNames = AgentFunctions.GetConditionStrings(_conditionGroup);
		}

		/// <summary>
		/// Update the node's functionality.
		/// </summary>
		public override void Update()
		{
			if (Transitions.Count <= 0) return;
			foreach (var transition in Transitions)
				transition.Draw();
		}
		
		/// <summary>
		/// Draw the context menu of the node.
		/// </summary>
		public override void DrawMenu()
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Transition/Yes"), false, StartTransition, "yes");
			menu.AddItem(new GUIContent("Transition/No"), false, StartTransition, "no");
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Delete"), false, Delete, null);
			menu.ShowAsContext();
		}

		/// <summary>
		/// Initialize the transition starting from this node.
		/// </summary>
		/// <param name="obj"></param>
		private void StartTransition(object obj)
		{
			var str = (string) obj;
			switch (str)
			{
				case "yes":
					BehaviourEditorWindow.StartTransition(Rect, this, BehaviourEditorWindow.PositiveColor);
					break;
				case "no":
					BehaviourEditorWindow.StartTransition(Rect, this, BehaviourEditorWindow.NegativeColor, true);
					break;
			}
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
			var col = ColorUtility.ToHtmlStringRGBA(BehaviourEditorWindow.PositiveColor);
            if (_conditionGroup != ConditionGroup.Time)
            {
                GUILayout.Label(Func + "<size=16><b><color=#" + col + ">?</color></b></size>");
            }
            else{
                var avg = (_minTime + _maxTime) / 2f;
                GUILayout.Label("<size=16><b><color=#" + col + ">time<</color></b></size>" 
                         + ((_minTime != _maxTime) ? "~" : "") + avg + "sec"
                         + "<size=16><b><color=#" + col + ">?</color></b></size>");
            }
		}
		
		/// <summary>
		/// Return the GUI skin of the node.
		/// </summary>
		/// <returns></returns>
		public override GUISkin GetSkin()
		{
			return (GUISkin)AssetDatabase.LoadAssetAtPath(
				PathManager.EliotStyles() + "ObserverWindowStyle.guiskin", typeof(GUISkin));
		}
		
		/// <summary>
		/// Return the default texture of the node.
		/// </summary>
		/// <returns></returns>
		protected override Texture2D GetNormalTexture()
		{
			return (Texture2D)AssetDatabase.LoadAssetAtPath(
				PathManager.EliotImages() + "romb.png", typeof(Texture2D));
		}

		/// <summary>
		/// Return the 'selected' texture of the node.
		/// </summary>
		/// <returns></returns>
		protected override Texture2D GetSelectedTexture()
		{
			return (Texture2D)AssetDatabase.LoadAssetAtPath(
				PathManager.EliotImages() + "rombSelected.png", typeof(Texture2D));
		}
	}
}
#endif
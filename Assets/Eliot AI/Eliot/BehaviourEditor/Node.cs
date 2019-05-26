#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Eliot.Repository;
using UnityEditor;
using UnityEngine;

namespace Eliot.BehaviourEditor
{
    /// <summary>
    /// The base class for all the type of nodes in the Behaviour model editor.
    /// </summary>
    [Serializable]
    public class Node : ScriptableObject
    {
        /// <summary>
        /// Check wheather the node already exists.
        /// </summary>
        public bool Exist { get; protected set; }
        
        /// <summary>
        /// Unique ID of the node.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Unique window ID for the editor.
        /// </summary>
        public int EditorId { get; private set; }
        
        /// <summary>
        /// Name of the node. Reflects the node type.
        /// </summary>
        public string NodeName { get; set; }
        
        /// <summary>
        /// The Rect component of the node.
        /// </summary>
        public Rect Rect { get; set; }
        
        /// <summary>
        /// List of node's transitions.
        /// </summary>
        public List<NodesTransition> Transitions { get { return _transitions;} }
        
        /// <summary>
        /// Wheather the node is currently grouped with another nodes.
        /// </summary>
        public bool Grouped { get; set; }
        
        /// <summary>
        /// Index of the function from the function group that the node holds information about.
        /// </summary>
        public int FuncIndex
        {
            get { return _funcIndex;}
            set { _funcIndex = value; }
        }

        public string FunctionName
        {
            get { return _functionName; }
            set { _functionName = value; }
        }
        
        /// <summary>
        /// Name of the function from the function group that the node holds information about.
        /// </summary>
        public string Func{
            get{ return FuncNames.Length > FuncIndex ? FuncNames[FuncIndex] : "NaN"; }
        }
        
        /// List of node's transitions.
        [HideInInspector][SerializeField] private List<NodesTransition> _transitions = new List<NodesTransition>();
        /// Index of the function from the function group that the node holds information about.
        [HideInInspector][SerializeField] private int _funcIndex;
        /// A list of names of functions in the current function group of the node.
        [HideInInspector][SerializeField] public string[] FuncNames = {"loading..."};

        [HideInInspector][SerializeField] private string _functionName = null;
        
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public Node(){}
        
        /// <summary>
        /// Initialize the node.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="nodeName"></param>
        public Node(Rect rect, string nodeName = "Node")
        {
            NodeName = nodeName;
            Rect = rect;
            Exist = true;
        }

        /// <summary>
        /// This function is called when the object is loaded.
        /// </summary>
        public virtual void OnEnable()
        {
            hideFlags = HideFlags.DontSave;
            Exist = true;
        }

        /// <summary>
        /// Update the node's functionality.
        /// </summary>
        public virtual void Update(){}
        
        /// <summary>
        /// Draw the context menu of the node.
        /// </summary>
        public virtual void DrawMenu(){}

        /// <summary>
        /// Node's window component functionality.
        /// </summary>
        /// <param name="id"></param>
        public virtual void NodeFunction(int id)
        {
            EditorId = id;
            if (!Grouped)
            {
                GUI.DragWindow();
            }
        }
        
        /// <summary>
        /// Remove the node from the editor.
        /// </summary>
        /// <param name="obj"></param>
        public void Delete(object obj)
        {
            Exist = false;
            BehaviourEditorWindow.RemoveNode(this);
        }

        /// <summary>
        /// Return the GUI skin of the node.
        /// </summary>
        /// <returns></returns>
        public virtual GUISkin GetSkin()
        {
            return (GUISkin)AssetDatabase.LoadAssetAtPath(
                PathManager.EliotStyles() + "InvokerWindowStyle.guiskin", typeof(GUISkin));
        }

        /// <summary>
        /// Return the default texture of the node.
        /// </summary>
        /// <returns></returns>
        protected virtual Texture2D GetNormalTexture()
        {
            return (Texture2D)AssetDatabase.LoadAssetAtPath(
                PathManager.EliotImages() + "rect.png", typeof(Texture2D));
        }

        /// <summary>
        /// Return the 'selected' texture of the node.
        /// </summary>
        /// <returns></returns>
        protected virtual Texture2D GetSelectedTexture()
        {
            return (Texture2D)AssetDatabase.LoadAssetAtPath(
                PathManager.EliotImages() + "rectSelected.png", typeof(Texture2D));
        }

        /// <summary>
        /// Return the texture concidering the grouped status of the node.
        /// </summary>
        /// <returns></returns>
        public Texture2D GetTexture()
        {
            return Grouped ? GetSelectedTexture() : GetNormalTexture();
        }
    }
}
#endif
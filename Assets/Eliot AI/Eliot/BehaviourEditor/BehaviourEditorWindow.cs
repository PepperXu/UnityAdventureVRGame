#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Eliot.Repository;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eliot.BehaviourEditor
{
	/// <summary>
	/// Editor window for Behaviour models.
	/// </summary>
	[Serializable] public class BehaviourEditorWindow : EditorWindow
	{
		/// <summary>
		/// Instance of this window. Makes sure there is only one instance of it.
		/// </summary>
		private static BehaviourEditorWindow Instance
		{
			get
			{
				if (_instance == null)
					_instance = GetWindow<BehaviourEditorWindow>("Behaviour");
				var icon = (Texture2D) AssetDatabase.LoadAssetAtPath(
					PathManager.EliotStyles() + "gamepad_grey.png", typeof(Texture2D));
				if(icon != null) 
					_instance.titleContent = new GUIContent(" Behaviour", icon);
				return _instance;
			}
		}
		
		/// List of nodes currently present in the window.
		public static List<Node> Nodes = new List<Node>();
		/// Behaviour model currently being updated with the Editor.
		public static EliotBehaviour Behaviour;
		/// Default color of transitions.
		public static Color NeutralColor = Color.white;
		/// Color of 'Yes' transitions.
		public static Color PositiveColor = new Color(0.2f, 0.8f, 0.1f, 1f);
		/// Color of 'No' transitions.
		public static Color NegativeColor = new Color(0.8f, 0.3f, 0.2f, 1f);
		/// Color of transitions that are selected.
		public static Color SelectedColor = new Color(1f, 1f, 0.2f, 1f);
		/// Color of 'While' transitions.
		public static Color LoopColor = new Color(0.7f, 0.1f, 0.9f, 1f);
		/// Color of selection box.
		public static Color SelectionBoxColor = new Color(0, 0.6f, 1, 0.2f);
		
		/// Instance of this window. Can be only single one.
		private static BehaviourEditorWindow _instance;
		/// Link to the Entry of the current Behaviour model.
		private static EntryNode _entry;
		/// Position of the mouse cursor on the screen.
		private static Vector2 _mousePos = Vector2.zero;
		/// Wheather the user is currently making a new transition.
		private static bool _makeTransitionMode;
		/// Origin transform of a new transition.
		private static Rect _newTransitionStart;
		/// Link to the node that is the origin of a new transition.
		private static Node _nodeThatStartedTransition;
		/// Color of a new transition.
		private static Color _transitionColor = NeutralColor;
		/// Wheather a new transition is a negative one.
		private static bool _transitionIsNeg;
		/// Original color of last selected transition.
		private static Color _lastTransSelectedColor;
		/// Link to the last selected transition.
		private static NodesTransition _selectedTransition;
		/// Position of the scroll rect.
		private static Vector2 _scrollPosition = Vector2.zero;
		/// Borders of the window.
		private static float _minX, _maxX, _minY, _maxY;
		/// Space which all the nodes are in.
		private static Rect _spaceRect;
		/// Rect that user can currently see.
		private static Rect _viewRect;
		/// Initial GUI style.
		private static GUISkin m_GUIStyle_Start;
		/// GUI style of this Editor window.
		private static GUISkin m_GUIStyle;
		/// Wheather the user is currently dragging the corner of the selection box.
		private static bool _dragging;
		/// Wheather the user is currently dragging the group of selected nodes.
		private static bool _draggingGroup;
		/// Start position of selection box.
		private static Vector2 _startPos;
		/// Rect of rhe selection box.
		private static Rect _selectionRect;
		/// Context menu of this window.
		private static GenericMenu _genericMenu;
		/// Backgtound texture of this window.
		private static Texture2D _backgroundTexture;
		/// Current event that is updated every OnGUI call.
		private static Event _event;
		///	Last time user used shortcuts for nodes creation.
		private static int _lastInputUpdate;
		/// Distance between border and the closest node to the border.
		private const float Padding  = 1000f;
		/// Path to the current behaviour.
		[SerializeField] private string _pathToBehaviour;
		
		/// <summary>
		/// Initialize the window.
		/// </summary>
		[MenuItem("Eliot/Behaviour")]
		private static void InitWindow() { _instance = Instance; }
		
		/// <summary>
		/// This function is called when the object is loaded.
		/// </summary>
		private void OnEnable ()
		{
			hideFlags = HideFlags.HideAndDontSave;
			
			if(m_GUIStyle == null)
				m_GUIStyle = (GUISkin)AssetDatabase.LoadAssetAtPath(
					PathManager.EliotStyles() + "Style.guiskin", typeof(GUISkin));
			if (_backgroundTexture == null)
			{
				_backgroundTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(
					PathManager.EliotImages() + "grid.png", typeof(Texture2D));
				_backgroundTexture.wrapMode = TextureWrapMode.Repeat;
			}
			var icon = (Texture2D) AssetDatabase.LoadAssetAtPath(
				PathManager.EliotImages() + "gamepad_grey.png", typeof(Texture2D));
			if(icon != null) 
				titleContent = new GUIContent(" Behaviour", icon);

			if (_pathToBehaviour != null) 
				Behaviour = AssetDatabase.LoadAssetAtPath<EliotBehaviour>(_pathToBehaviour);
			
			try{ Reverse(null); }catch (Exception){/**/}
		}

		/// <summary>
		/// Make sure there is exactly one Entry in the Editor.
		/// </summary>
		private static void RemoveExtraEntry()
		{
			var entries = from entry in Nodes where entry is EntryNode select entry;
			var qNodes = entries.ToList();
			if (qNodes.Count <= 1) return;
			foreach (var entry in qNodes) 
				if(entry.Transitions.Count == 0) 
					Nodes.Remove(entry);
		}
		
		/// <summary>
		/// Update the window when mouse is over another one.
		/// </summary>
		private void Update()
		{
			if (_event == null) return;
			if (_event.type == EventType.MouseUp)
			{
				_draggingGroup = false;
				ClearGrouped();
				_dragging = false;

				foreach (var node in Nodes)
				{
					if (!_selectionRect.Contains(node.Rect.center)) continue;
					node.Grouped = true;
				}
				_genericMenu = null;
			}
		}
		
		/// <summary>
		/// Draw GUI and run listeners.
		/// </summary>
		private void OnGUI()
		{
			_event = Event.current;
			
			var width = _maxX - _minX + Padding > position.width ? _maxX - _minX + Padding : position.width;
			var height = _maxY - _minY + Padding > position.height ? _maxY - _minY + Padding : position.height;
			_spaceRect = new Rect(0, 0, width, height);
			_viewRect = new Rect(0, 0, position.width, position.height);
			_scrollPosition = GUI.BeginScrollView(_viewRect, _scrollPosition, _spaceRect, GUIStyle.none, GUIStyle.none);

             GUI.DrawTextureWithTexCoords(_spaceRect, _backgroundTexture, 
	             new Rect(0, 0, _spaceRect.width / _backgroundTexture.width, _spaceRect.height / _backgroundTexture.height));

			GUI.color = Color.white;
			
			m_GUIStyle_Start = GUI.skin;
			GUI.skin = m_GUIStyle;

			UpdateShortcuts();
			
			InitEntry();
			RemoveExtraEntry();
			
			DragMultipleWindows();
			
			_mousePos = Event.current.mousePosition;
			if (!_makeTransitionMode)
			{
				UpdateContextMenu();
				UpdateSelected();
				UpdateNodes();
				UpdateSelectedTransitions();
			}
			else
			{
				if(Event.current.type != EventType.Layout)
					UpdateNewTransition(_transitionColor, _transitionIsNeg);
				Repaint();
				UpdateNodes();
				
			}
			GUI.EndScrollView();
			
			DoDragAndDrop();
			PanScrollView();
			SelectionBox();
			
			GUI.skin = m_GUIStyle_Start;
		}

		private static void UpdateShortcuts()
		{
			try
			{
				if (Event.current.control && Event.current.keyCode == KeyCode.Z)
					UndoOperation(null);
				if (Event.current.control && Event.current.keyCode == KeyCode.Y)
					RedoOperation(null);
				if (Event.current.control && Event.current.keyCode == KeyCode.S)
					Save(null);
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
					Delete(null);
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.I)
					AddNode("Invoker");
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.O)
					AddNode("Observer");
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.L)
					AddNode("Loop");
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Y)
				{
					var node = HoverOnNode();
					if (node != null)
						StartTransition(node.Rect, node, (node is ObserverNode ? PositiveColor : node is LoopNode ? LoopColor : NeutralColor), false);
				}
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.N)
				{
					var node = HoverOnNode();
					if (node != null)
						StartTransition(node.Rect, node, (node is ObserverNode ? NegativeColor : NeutralColor), true);
				}
			}
			catch (Exception)
			{
				/*Its ok*/
			}
		}

		/// <summary>
		/// Update the dragging multiple nodes at once functionality.
		/// </summary>
		private static void DragMultipleWindows()
		{
			if (!_draggingGroup && Selection.objects.Length > 0 && Event.current.button == 0 
			    && Event.current.type == EventType.MouseDown 
			    && Selection.objects.Contains(HoverOnNode(true)))
			{
				_draggingGroup = true;
			}

			if (Event.current.type == EventType.MouseUp)
			{
				_draggingGroup = false;
			}

			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && _draggingGroup)
			{
				var delta = Event.current.delta;
				foreach (var node in Selection.objects)
				{
					if (!(node is Node)) continue;
					if (!(node as Node).Grouped) continue;
					var rect = ((Node) node).Rect;
					rect.position += delta;
					((Node) node).Rect = rect;
				}
			}

			if (Selection.objects.Length < 1)
			{
				ClearGrouped();
				_draggingGroup = false;
			}
			else if (Selection.objects.Length == 1)
			{
				if (!(Selection.objects[0] is Node)) return;
				GUI.FocusWindow(((Node) Selection.objects[0]).EditorId);
			}
			else if (Selection.objects.OfType<Node>().Any(node => !node.Grouped))
			{
				ClearGrouped();
				_draggingGroup = false;
			}
		}

		/// <summary>
		/// Update the pan view functionality.
		/// </summary>
		private static void PanScrollView()
		{
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
			{
				_scrollPosition -= Event.current.delta;
			}
		}

		/// <summary>
		/// Clear the nodes group selection.
		/// </summary>
		private static void ClearGrouped()
		{
			foreach (var node in Nodes)
				if (node.Grouped)
					node.Grouped = false;
		}

		/// <summary>
		/// Update the load Behaviour by dropping it onto window functionality.
		/// </summary>
		private void DoDragAndDrop()
		{
			if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
			{
				// Show a copy icon on the drag
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (Event.current.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();
					if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is EliotBehaviour)
					{
						Behaviour = DragAndDrop.objectReferences[0] as EliotBehaviour;
						_pathToBehaviour = AssetDatabase.GetAssetPath(Behaviour);
						Behaviour.UpdateBehaviour();
						if (string.IsNullOrEmpty(Behaviour.Json))
						{
							Clear(null);
							Save(null);
						}
						Reverse(null);
					}
				}

				Event.current.Use(); 
			}
		}
		
		/// <summary>
		/// Update the selection box.
		/// </summary>
		private static void SelectionBox()
		{
			_mousePos = Event.current.mousePosition;
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && HoverOnNode(true) == null &&
			    HoverOnTransition() == null)
			{
				
				if (!_dragging)
				{
					_dragging = true;
					_startPos = _mousePos;
				}
			}

			if (Event.current.type == EventType.MouseUp)
			{
				_draggingGroup = false;
				ClearGrouped();
				_dragging = false;

				var selectedNodes = new List<Object>();
				foreach (var node in Nodes)
				{
					if (!_selectionRect.Contains(node.Rect.center)) continue;
					selectedNodes.Add(node);
					node.Grouped = true;
				}

				if(!(Selection.activeObject is NodesTransition))
					Selection.objects = selectedNodes.ToArray();
			}

			if (_dragging)
			{
				var c = GUI.color;
				GUI.color = SelectionBoxColor;
				
				var x = _startPos.x > _mousePos.x ? _mousePos.x : _startPos.x;
				var y = _startPos.y > _mousePos.y ? _mousePos.y : _startPos.y;
				var w = Mathf.Abs(_mousePos.x - _startPos.x);
				var h = Mathf.Abs(_mousePos.y - _startPos.y);
				_selectionRect = new Rect(x + _scrollPosition.x, y + _scrollPosition.y, w, h);
				
				GUI.Box(new Rect(x, y, w, h), "");
				GUI.color = c;
			}

			Instance.Repaint();
		}

		/// <summary>
		/// Update each node rendering and functions.
		/// </summary>
		private void UpdateNodes()
		{
			if (Nodes.Count > 0)
			{
				var id = 0;
				BeginWindows();

				_minX = _maxX = _minY = _maxY = 0;
				foreach (var node in Nodes)
				{
					var style = node.GetSkin();
					GUIStyle window;
					try
					{
						window = style.window;
					}
					catch (Exception)
					{
						window = GUI.skin.window;
						
					}
					var copyStyle = new GUIStyle(window) {normal = {background = node.GetTexture()}};
					var normalRect = node.Rect;
					var x = normalRect.center.x;
					var y = normalRect.center.y;
					normalRect.center = new Vector2(Mathf.Floor(x - x % 10), Mathf.Floor(y - y % 10));
					node.Rect = GUI.Window(id++, node.Grouped ? node.Rect : normalRect, node.NodeFunction, node.NodeName, copyStyle);
					node.Update();

					var rX = node.Rect.x;
					if (rX < _minX) _minX = rX;
					else if (rX > _maxX)
						_maxX = rX;

					var rY = node.Rect.y;
					if (rY < _minY) _minY = rY;
					else if (rY > _maxY)
						_maxY = rY;
				}

				EndWindows();
			}
		}

		/// <summary>
		/// Initialize a new transition.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="node"></param>
		/// <param name="color"></param>
		/// <param name="isNegative"></param>
		public static void StartTransition(Rect start, Node node, Color color, bool isNegative = false)
		{
			_transitionColor = color;
			_transitionIsNeg = isNegative;
			_makeTransitionMode = true;
			_newTransitionStart = start;
			_nodeThatStartedTransition = node;
			
			ClearGrouped();
			_draggingGroup = false;
		}
		
		/// <summary>
		/// Update parameters of new transition while it is not a part of model yet.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="isNegative"></param>
		private static void UpdateNewTransition(Color? color = null, bool isNegative = false)
		{
			if (color == null) color = NeutralColor;
			if (ClickedMouse())
			{
				var hover = HoverOnNode();
				if (hover == null){
					_makeTransitionMode = false;
					return;
				}
				if (_nodeThatStartedTransition.Equals(hover) || _entry.Equals(hover)) {
					_makeTransitionMode = false;
					return;
				}
				var transition = CreateInstance<NodesTransition>();
				EditorUtility.SetDirty(transition);
				transition.Start = _nodeThatStartedTransition;
				transition.End = hover;
				transition.Color = _transitionColor;
				transition.IsNegative = isNegative;
				_nodeThatStartedTransition.Transitions.Add(transition);
				_makeTransitionMode = false;
				_transitionColor = NeutralColor;
				_transitionIsNeg = false;
				
				ClearGrouped();
				_draggingGroup = false;
			}
			
			var startPos = new Vector3(_newTransitionStart.x + _newTransitionStart.width / 2, 
				_newTransitionStart.y + _newTransitionStart.height / 2, 0);
			var endPos = new Vector3(Event.current.mousePosition.x, Event.current.mousePosition.y, 0);
			
			Handles.color = color.Value;
			Handles.DrawAAPolyLine(4, startPos, endPos);
			DrawArrow(startPos, endPos);

			if (Event.current.type != EventType.Repaint)
				Event.current.Use();
		}

		/// <summary>
		/// Draw the arrow on a transition.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		private static void DrawArrow(Vector3 start, Vector3 end)
		{
			var arrowHead = new Vector3[3];
			
			var forward = (end - start).normalized;
			var right = Vector3.Cross(Vector3.forward, forward).normalized;
			var size = HandleUtility.GetHandleSize(end);
			var width = size * 0.3f;
			var height = size * 0.5f;

			var len = (end - start).magnitude;
			var cen = start + (len*0.5f+height/2f) * forward;
			arrowHead[0] = cen;
			arrowHead[1] = cen - forward * height + right*width;
			arrowHead[2] = cen - forward * height - right*width;
			
			Handles.DrawAAConvexPolygon(arrowHead);
		}
		
		/// <summary>
		/// Update the editor's context menu.
		/// </summary>
		private static void UpdateContextMenu()
		{
			if (ClickedMouseRight())
			{
				if (HoverOnNode() != null)
					HoverOnNode().DrawMenu();
				else if (HoverOnTransition() != null)
					HoverOnTransition().DrawMenu();
				else
					DrawContextMenu();
				
				Event.current.Use(); 
			}
		}

		/// <summary>
		/// Remove specific node from the editor.
		/// </summary>
		/// <param name="node"></param>
		public static void RemoveNode(Node node)
		{
			try
			{
				if (Nodes.Contains(node))
					Nodes.Remove(node);
				SetIDs();
			}
			catch(Exception){/**/}
		}

		/// <summary>
		/// Recalculate the ID of each node.
		/// </summary>
		private static void SetIDs()
		{
			var id = 0;
			foreach (var node in Nodes)
			{
				node.Id = id.ToString();
				id++;
			}
		}

		/// <summary>
		/// Update the functionality of selection of a transition.
		/// </summary>
		private static void UpdateSelectedTransitions()
		{
			if (!ClickedMouse()) return;
			if (Nodes == null || Nodes.Count <= 0) return;
			var hover = HoverOnTransition();
			if (hover != null)
			{
				_selectedTransition = hover;
				hover.IsSelected = true;
				Selection.activeObject = hover;
			}
			else if(Selection.objects.Length == 0) 
				Selection.activeObject = null;
			_genericMenu = null;
		}
		
		/// <summary>
		/// Update functionality of selection of a node.
		/// </summary>
		private static void UpdateSelected()
		{
			if (!ClickedMouseLeft() || _draggingGroup) return;
			var hover = HoverOnNode();
			if (hover != null){
				Selection.activeObject = hover;
				if(_selectedTransition != null)
					_selectedTransition.IsSelected = false;
			}
		}

		/// <summary>
		/// Whaether any mouse button has been clicked.
		/// </summary>
		/// <returns></returns>
		private static bool ClickedMouse()
		{
			return Event.current.type == EventType.MouseDown;
		}
		
		/// <summary>
		/// Wheather the right mouse button has been clicked.
		/// </summary>
		/// <returns></returns>
		private static bool ClickedMouseRight()
		{
			return Event.current.type == EventType.MouseDown && Event.current.button == 1;
		}
		
		/// <summary>
		/// Wheather the left mouse button has been clicked.
		/// </summary>
		/// <returns></returns>
		private static bool ClickedMouseLeft()
		{
			return Event.current.type == EventType.MouseDown && Event.current.button == 0;
		}

		/// <summary>
		/// Return the node which the mouse cursor is currently over or null if there is no such node.
		/// </summary>
		/// <returns></returns>
		private static Node HoverOnNode(bool grouped = false)
		{
			if (Nodes == null || Nodes.Count <= 0) return null;
			foreach (var node in Nodes)
				try
				{
					if (!grouped)
					{
						if (node.Rect.Contains(_mousePos))
							return node;
					}
					else
					{
						var nodeRect = node.Rect;
						nodeRect.center -= _scrollPosition;
						if (nodeRect.Contains(_mousePos))
							return node;
					}
				}catch(Exception){return null;}

			return null;
		}
		
		/// <summary>
		/// Return the transition which the mouse cursor is currently over or null if there is no such transition.
		/// </summary>
		/// <returns></returns>
		private static NodesTransition HoverOnTransition()
		{
			NodesTransition res = null;
			try
			{
				foreach (var node in Nodes)
				foreach (var trans in node.Transitions)
					if (trans.Contains(_mousePos))
						res = trans;
					else
						trans.IsSelected = false;
			}
			catch(Exception) {res = null;}
			return res;
		}

		/// <summary>
		/// Draw the context menu.
		/// </summary>
		private static void DrawContextMenu()
		{
			_genericMenu = new GenericMenu();
			_genericMenu.AddItem(new GUIContent("Add Node/Invoker"), false, AddNode, "Invoker");
			_genericMenu.AddItem(new GUIContent("Add Node/Observer"), false, AddNode, "Observer");
			_genericMenu.AddItem(new GUIContent("Add Node/Loop"), false, AddNode, "Loop");
			_genericMenu.AddSeparator("");
			_genericMenu.AddItem(new GUIContent("Save"), false, Save, null);
			_genericMenu.AddItem(new GUIContent("Reverse"), false, Reverse, null);
			_genericMenu.AddItem(new GUIContent("Select file"), false, SelectFile, null);
			_genericMenu.AddSeparator("");
			_genericMenu.AddItem(new GUIContent("Clear"), false, Clear, null);
			_genericMenu.ShowAsContext();
			
			Event.current.Use();
		}

		/// <summary>
		/// Undo the last operation and update the model.
		/// </summary>
		/// <param name="obj"></param>
		private static void UndoOperation(object obj)
		{
			try
			{
				Undo.PerformUndo();
				Reverse(null);
			}
			catch (Exception){/*Alright*/}
		}

		/// <summary>
		/// Redo the last operation and update the model.
		/// </summary>
		/// <param name="obj"></param>
		private static void RedoOperation(object obj)
		{
			try
			{
				Undo.PerformRedo();
				Reverse(null);
			}
			catch (Exception){/*Alright*/}
		}

		/// <summary>
		/// Set the current Behaviour as an active object of the Selection.
		/// </summary>
		/// <param name="obj"></param>
		private static void SelectFile(object obj)
		{
			Selection.activeObject = Behaviour;
			_genericMenu = null;
		}

		/// <summary>
		/// Save the changes made in the editor to the Behaviour.
		/// </summary>
		/// <param name="obj"></param>
		public static void Save(object obj)
		{
			if (Behaviour != null)
			{
				Behaviour.Nodes = Nodes;
				Behaviour.InitTransitions(Nodes);
				Behaviour.ToJson();
				EditorUtility.SetDirty(Behaviour);
				SetIDs();
			}

			_genericMenu = null;
		}

		/// <summary>
		/// Load the last saved version of the Behaviour.
		/// </summary>
		/// <param name="obj"></param>
		private static void Reverse(object obj)
		{
			if (Behaviour == null) return;
			Behaviour.Serialize();
			foreach (var node in Behaviour.Nodes)
			{
				var entry = node as EntryNode;
				if (entry != null) _entry = entry;
				break;
			}
			_genericMenu = null;
		}
		
		/// <summary>
		/// Remove selected nodes from the editor.
		/// </summary>
		/// <param name="obj"></param>
		private static void Delete(object obj)
		{
			var selectedObjects = Selection.objects;
			if (selectedObjects.Length > 0)
			{
				foreach (var node in selectedObjects)
				{
					if(node is Node && !(node is EntryNode))
						(node as Node).Delete(null);
					if(node is NodesTransition)
						(node as NodesTransition).Delete(null);
				}
			}
			_genericMenu = null;
		}
		
		/// <summary>
		/// Add a new node to the Editor.
		/// </summary>
		/// <param name="obj"></param>
		private static void AddNode(object obj)
		{
			var str = (string) obj;
			switch (str)
			{
				case "Invoker":
				{
					var rect = new Rect(0, 0, 175, 40) 
						{center = new Vector2(_mousePos.x, _mousePos.y+40) + _scrollPosition};
					var node = CreateInstance<InvokerNode>();
					EditorUtility.SetDirty(node);
					node.NodeName = "Invoker";
					node.Rect = rect;
					Nodes.Add(node);
					Selection.activeObject = node;
					break;
				}
				case "Observer":
				{
					var rect = new Rect(0, 0, 175, 40) 
						{center = new Vector2(_mousePos.x, _mousePos.y+40) + _scrollPosition};
					var node = CreateInstance<ObserverNode>();
					EditorUtility.SetDirty(node);
					node.NodeName = "Observer";
					node.Rect = rect;
					Nodes.Add(node);
					Selection.activeObject = node;
					break;
				}
				case "Loop":
				{
					var rect = new Rect(0, 0, 175, 40) 
						{center = new Vector2(_mousePos.x, _mousePos.y+40) + _scrollPosition};
					var node = CreateInstance<LoopNode>();
					EditorUtility.SetDirty(node);
					node.NodeName = "Loop";
					node.Rect = rect;
					Nodes.Add(node);
					Selection.activeObject = node;
					break;
				}
			}

			SetIDs();
			_genericMenu = null;
		}

		/// <summary>
		/// Initialize Entry's parameters.
		/// </summary>
		private static void InitEntry()
		{
			if (_entry != null || Nodes.Contains(_entry))
			{
				return;
			}
			var rect = new Rect(_viewRect.width/2-75, _viewRect.height/2-15, 150, 30);
			_entry = CreateInstance<EntryNode>();
			_entry.NodeName = "Entry";
			_entry.Rect = rect;
			_entry.Id = "0";
			Nodes.Add(_entry);
		}
		
		/// <summary>
		/// Clear the editor and initialize the entry.
		/// </summary>
		/// <param name="obj"></param>
		public static void Clear(object obj)
		{
			Nodes = new List<Node>();
			_entry = null;
			InitEntry();
		}

		/// <summary>
		/// Create a new empty Node of certain type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Node TemplateNode(string type)
		{
			Node node = null;
			switch (type)
			{
				case "Entry":
					node = CreateInstance<EntryNode>();
					break;
				case "Invoker":
					node = CreateInstance<InvokerNode>();
					break;
				case "Observer":
					node = CreateInstance<ObserverNode>();
					break;
				case "Loop":
					node = CreateInstance<LoopNode>();
					break;
			}
			return node;
		}

		/// <summary>
		/// Create a new empry transition.
		/// </summary>
		/// <returns></returns>
		public static NodesTransition TemplateTransition()
		{
			var transition = CreateInstance<NodesTransition>();
			return transition;
		}
	}
}
#endif
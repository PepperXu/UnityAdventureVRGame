using System;
using System.Collections.Generic;
using System.Linq;
using Eliot.AgentComponents;
using Eliot.BehaviourEditor;
using UnityEngine;

namespace Eliot.Repository
{
    /// <summary>
    /// Constructs JsonObject and valid Json strings.
    /// </summary>
    public static class JsonFactory
    {
        #region BASE
        /// <summary>
        /// Create a new pair of name and value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Field(string name, object value)
        {
            return "\"" + name + "\":\"" + value + "\"";
        }
        
        /// <summary>
        /// Create new object that has a name and can hold multiple fields which can be another objects.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="wrap"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string Object(string name, bool wrap, params string[] fields)
        {
            var res = (wrap?"{":"") + (name!=null ? "\"" + name + "\":{" : "");
            var len = fields.Length;
            if(len > 0)
            for(var i = 0; i < len; i++)
                res += fields[i] + (i < len-1 ? "," : "");
            return res + (name!=null ? "}" : "") + (wrap?"}":"");
        }

        /// <summary>
        /// Create new object that has a name and can hold multiple fields which can NOT be another objects.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string Array(string name, List<string> fields)
        {
            var res = "\"" + name + "\":[";
            var len = fields.Count;
            if(len > 0)
                for(var i = 0; i < len; i++)
                    res += fields[i] + (i < len-1 ? "," : "");
            return res + "]";
        }
        
        #endregion
        
        #region CONVERT ELIOT OBJECTS TO JSON
        
        #if UNITY_EDITOR
        /// <summary>
        /// Create json string from Rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static string ToJson(this Rect rect){
            return Object("rect", false,
                Field("posX", rect.x), 
                Field("posY", rect.y), 
                Field("width", rect.width), 
                Field("height", rect.height));
        }
        
        /// <summary>
        /// Create json string from EntryNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string ToJson(this EntryNode node){
            return Object(null, true,
                Field("type", node.NodeName), 
                Field("ID", node.Id), 
                node.Rect.ToJson());
        }
        
        /// <summary>
        /// Create json string from InvokerNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string ToJson(this InvokerNode node){
            return Object(null, true,
                Field("type", node.NodeName),
                Field("ID", node.Id),
                Field("actionGroup", node.ActionGroup),
                Field("functionID", node.ActionGroup.Equals(ActionGroup.Skill) ? node.FuncNames[0] : node.FuncIndex.ToString()),
                //Field("functionName", node.ActionGroup.Equals(ActionGroup.Skill) ? node.FuncNames[0] :  node.FuncNames[node.FuncIndex]),
                Field("functionName", node.FunctionName),
                Field("executeSkill", node.ExecuteSkill),
                node.Rect.ToJson());
        }
        
        /// <summary>
        /// Create json string from ObserverNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string ToJson(this ObserverNode node){
            return Object(null, true,
                Field("type", node.NodeName), 
                Field("ID", node.Id), 
                Field("functionID", node.FuncIndex),
                //Field("functionName", node.FuncNames[node.FuncIndex]),
                Field("functionName", node.ConditionGroup != ConditionGroup.Time ? node.FunctionName : node.FormattedTime),
                Field("conditionGroup", node.ConditionGroup),
                Field("minTime", node.MinTime),
                Field("maxTime", node.MaxTime),
                node.Rect.ToJson());
        }
        
        /// <summary>
        /// Create json string from LoopNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string ToJson(this LoopNode node){
            return Object(null, true,
                Field("type", node.NodeName), 
                Field("ID", node.Id), 
                Field("functionID", node.FuncIndex),
                //Field("functionName", node.FuncNames[node.FuncIndex]),
                Field("functionName", node.ConditionGroup != ConditionGroup.Time ? node.FunctionName : node.FormattedTime),
                Field("conditionGroup", node.ConditionGroup),
                Field("reverse", node.Reverse),
                Field("minTime", node.MinTime),
                Field("maxTime", node.MaxTime),
                node.Rect.ToJson());
        }
        
        /// <summary>
        /// Create json string from Node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string ToJson(this Node node)
        {
            if (node is EntryNode) return ((EntryNode) node).ToJson();
            if (node is InvokerNode) return ((InvokerNode) node).ToJson();
            if (node is ObserverNode) return ((ObserverNode) node).ToJson();
            if (node is LoopNode) return ((LoopNode) node).ToJson();
            return null;
        }

        /// <summary>
        /// Create json string from NodesTransition.
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static string ToJson(this NodesTransition trans)
        {
            return Object(null, true,
                Field("startID", trans.Start.Id), 
                Field("endID", trans.End.Id), 
                Field("type", trans.IsNegative?"negative":"positive"),
                Field("minRate", trans.MinRate),
                Field("maxRate", trans.MaxRate),
                Field("minCooldown", trans.MinCooldown),
                Field("maxCooldown", trans.MaxCooldown),
                Field("terminate", trans.Terminate),
                Object("color", false,
                    Field("r", trans.Color.r),
                    Field("g", trans.Color.g),
                    Field("b", trans.Color.b)
                    )
                );
        }

        /// <summary>
        /// Create json string from Behaviour.
        /// </summary>
        /// <param name="core"></param>
        public static void ToJson(this EliotBehaviour core)
        {
            var res = Object(null, true, 
                Field("version", 1.2F),
                Array("nodes", (from node in core.Nodes select node.ToJson()).ToList()),
                Array("transitions", (from trans in core.Transitions select trans.ToJson()).ToList()));
            core.Json = res;
        }
        #endif
        #endregion
        
        #region CONVERT JSON TO ELIOT OBJECTS
#if UNITY_EDITOR
        /// <summary>
        /// Create EntryNode from json string.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        private static Node Serialize(this EntryNode node, JsonObject jObj)
        {
            node.NodeName = jObj["type"].String;
            node.Id = jObj["ID"].String;
            var x = jObj["rect"]["posX"].Float;
            var y = jObj["rect"]["posY"].Float;
            var w = jObj["rect"]["width"].Float;
            var h = jObj["rect"]["height"].Float;
            node.Rect = new Rect(x, y, w, h);
			
            return node;
        }
        
        /// <summary>
        /// Create InvokerNode from json string.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        private static Node Serialize(this InvokerNode node, JsonObject jObj)
        {
            node.NodeName = jObj["type"].String;
            node.Id = jObj["ID"].String;
            node.ActionGroup = jObj["actionGroup"].ActionGroup;
            if (node.ActionGroup == ActionGroup.Skill)
            {
                node.FuncIndex = 0;
                node.FuncNames = new[] { jObj["functionID"].String };
                try
                { node.FunctionName = jObj["functionName"].String; }
                catch (Exception){/*Debug.Log("Your behaviour has a deprecated format");*/}
                
                if(jObj["executeSkill"] != null)
                    node.ExecuteSkill = jObj["executeSkill"].Bool;
            }
            else if (node.ActionGroup == ActionGroup.Inventory)
            {
                //node.FuncIndex = jObj["functionID"].Int;
                node.FuncNames = AgentFunctions.GetFunctions<InventoryActionInterface>();
                try
                { node.FunctionName = jObj["functionName"].String; }
                catch (Exception){/*Debug.Log("Your behaviour has a deprecated format");*/}
            }
            else
            {
                //node.FuncIndex = jObj["functionID"].Int;
                node.FuncNames = AgentFunctions.GetFunctions<MotionActionInterface>();
                try
                { node.FunctionName = jObj["functionName"].String; }
                catch (Exception){/*Debug.Log("Your behaviour has a deprecated format");*/}
            }
            
            var sortedFunctions = node.FuncNames;
            System.Array.Sort(sortedFunctions, StringComparer.InvariantCulture);

            if (node.FunctionName != null)
                    for (var i = 0; i < node.FuncNames.Length; i++)
                        if (node.FuncNames[i] == node.FunctionName)
                            node.FuncIndex = i;

            if (node.FuncIndex == -1)
                node.FuncIndex =  jObj["functionID"].Int;
            
            var x = jObj["rect"]["posX"].Float;
            var y = jObj["rect"]["posY"].Float;
            var w = jObj["rect"]["width"].Float;
            var h = jObj["rect"]["height"].Float;
            node.Rect = new Rect(x, y, w, h);
			
            return node;
        }
        
        /// <summary>
        /// Create ObserverNode from json string.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        private static Node Serialize(this ObserverNode node, JsonObject jObj)
        {
            node.NodeName = jObj["type"].String;
            node.Id = jObj["ID"].String;
            //node.FuncIndex = jObj["functionID"].Int;
            node.ConditionGroup = jObj["conditionGroup"].ConditionGroup;
            node.FuncNames = AgentFunctions.GetConditionStrings(node.ConditionGroup);
            try
            { node.FunctionName = jObj["functionName"].String; }
            catch (Exception){/*Debug.Log("Your behaviour has a deprecated format");*/}

            try
            {
                node.MinTime = jObj["minTime"].Float;
                node.MaxTime = jObj["maxTime"].Float;
            }
            catch (Exception) {/*Debug.Log("Your behaviour has a deprecated format");*/}

            var sortedFunctions = node.FuncNames;
            System.Array.Sort(sortedFunctions, StringComparer.InvariantCulture);
            
            if (node.FunctionName != null)
                    for (var i = 0; i < node.FuncNames.Length; i++)
                        if (node.FuncNames[i] == node.FunctionName)
                            node.FuncIndex = i;
            
            if (node.FuncIndex == -1)
                node.FuncIndex =  jObj["functionID"].Int;
            
            var x = jObj["rect"]["posX"].Float;
            var y = jObj["rect"]["posY"].Float;
            var w = jObj["rect"]["width"].Float;
            var h = jObj["rect"]["height"].Float;
            node.Rect = new Rect(x, y, w, h);
			
            return node;
        }
        
        /// <summary>
        /// Create LoopNode from json string.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        private static Node Serialize(this LoopNode node, JsonObject jObj)
        {
            node.NodeName = jObj["type"].String;
            node.Id = jObj["ID"].String;
            //node.FuncIndex = jObj["functionID"].Int;
            node.ConditionGroup = jObj["conditionGroup"].ConditionGroup;
            node.FuncNames = AgentFunctions.GetConditionStrings(node.ConditionGroup);
            try
            { node.FunctionName = jObj["functionName"].String; }
            catch (Exception){/*Debug.Log("Your behaviour has a deprecated format");*/}

            try
            { 
                node.MinTime = jObj["minTime"].Float;
                node.MaxTime = jObj["maxTime"].Float;
            }
            catch (Exception) {/*Debug.Log("Your behaviour has a deprecated format");*/}

            var sortedFunctions = node.FuncNames;
            System.Array.Sort(sortedFunctions, StringComparer.InvariantCulture);
            
            if (node.FunctionName != null)
                    for (var i = 0; i < node.FuncNames.Length; i++)
                        if (node.FuncNames[i] == node.FunctionName)
                            node.FuncIndex = i;
            
            if (node.FuncIndex == -1)
                node.FuncIndex =  jObj["functionID"].Int;
            
            
            node.Reverse = jObj["reverse"]!=null && jObj["reverse"].Bool;
            var x = jObj["rect"]["posX"].Float;
            var y = jObj["rect"]["posY"].Float;
            var w = jObj["rect"]["width"].Float;
            var h = jObj["rect"]["height"].Float;
            node.Rect = new Rect(x, y, w, h);
			
            return node;
        }
        
        /// <summary>
        /// Create Node from json string.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        public static Node Serialize(this Node node, JsonObject jObj)
        {
            if (jObj["type"].String == "Entry") return ((EntryNode) node).Serialize(jObj);
            if (jObj["type"].String == "Invoker") return ((InvokerNode) node).Serialize(jObj);
            if (jObj["type"].String == "Observer") return ((ObserverNode) node).Serialize(jObj);
            if (jObj["type"].String == "Loop") return ((LoopNode) node).Serialize(jObj);
            return null;
        }

        /// <summary>
        /// Create all the Behaviour objects from its json string.
        /// </summary>
        /// <param name="behaviour"></param>
        public static void Serialize(this EliotBehaviour behaviour)
        {
            behaviour.Nodes = new List<Node>();
            BehaviourEditorWindow.Nodes = new List<Node>();
            var jObj = new JsonObject(behaviour.Json);
            var jNodes = jObj["nodes"].Objects;
            
            if (jNodes == null)
            {
                behaviour.Nodes = new List<Node>();
                behaviour.Transitions = new List<NodesTransition>();
                BehaviourEditorWindow.Nodes = behaviour.Nodes;
                BehaviourEditorWindow.Clear(null);
            }
            else
            {
                if (jNodes.Count > 0)
                    foreach (var node in jNodes)
                        behaviour.Nodes.Add(BehaviourEditorWindow.TemplateNode(((JsonObject)node)["type"].String).Serialize((JsonObject)node));

                behaviour.Transitions = new List<NodesTransition>();
                var jTransitions = jObj["transitions"].Objects;
                foreach (var trans in jTransitions)
                {
                    var transition = (JsonObject) trans;
                    if (transition["startID"] == null) break;
                    var startId = transition["startID"].Int;
                    var endId = transition["endID"].Int;
                    var isNeg = transition["type"].String.Equals("negative");
                    int minRate, maxRate;
                    float minCooldown, maxCooldown;
                    bool terminate;
                    try
                    {
                        minRate = transition["minRate"].Int;
                        maxRate = transition["maxRate"].Int;
                        minCooldown = transition["minCooldown"].Float;
                        maxCooldown = transition["maxCooldown"].Float;
                        terminate = transition["terminate"].Bool;
                    }
                    catch (Exception)
                    {
                        minRate = 1;
                        maxRate = 1;
                        minCooldown = 0f;
                        maxCooldown = 0f;
                        terminate = false;
                    }

                    var color = BehaviourEditorWindow.NeutralColor;
                    var tColor = transition["color"];
                    if (tColor != null)
                    {
                        var r = transition["color"]["r"].Float;
                        var g = transition["color"]["g"].Float;
                        var b = transition["color"]["b"].Float;
                        var a = Mathf.Clamp(1f / (maxRate * 0.2f)+0.2f, 0.5f, 2f);
                        color = new Color(r, g, b, a);
                    }

                    var t = BehaviourEditorWindow.TemplateTransition();
                    t.Start = behaviour.Nodes[startId];
                    t.End = behaviour.Nodes[endId];
                    t.IsNegative = isNeg;
                    t.MinRate = minRate;
                    t.MaxRate = maxRate;
                    t.MinCooldown = minCooldown;
                    t.MaxCooldown = maxCooldown;
                    t.Terminate = terminate;
                    t.Color = color;
                    behaviour.Nodes[startId].Transitions.Add(t);
                    behaviour.Transitions.Add(t);
                }

                BehaviourEditorWindow.Nodes = behaviour.Nodes;
            }
        }
#endif
        #endregion
    }
}
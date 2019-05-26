#pragma warning disable CS0219, CS1692
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Eliot.BehaviourEngine;
using Eliot.AgentComponents;
using Eliot.BehaviourEditor;

namespace Eliot.Repository
{
	/// <summary>
	/// Responsible for turning json string into behaviour algorithm.
	/// </summary>
    public static class CoresPool
    {
	    /// <summary>
	    /// Decipher the file and create methods and relations between them using reflection.
	    /// </summary>
	    /// <param name="behaviour"></param>
	    /// <param name="gameObject"></param>
	    /// <returns></returns>
	    public static BehaviourCore GetCore(EliotBehaviour behaviour, GameObject gameObject)
	    {
		    var core = new BehaviourCore();
		    behaviour.Json = BehaviourVersionManager.UpdateJson(behaviour.Json);
		    // See if there are any nodes in file.
		    var jObj = new JsonObject(behaviour.Json);
		    var jNodes = jObj["nodes"].Objects;
		    if (jNodes == null) return null;
		    
		    Entry entry = null;
		    var particles = new List<EliotComponent>();
		    
		    // If there are, create Behaviour Components from each piece of data.
		    if (jNodes.Count > 0)
			    foreach (var node in jNodes)
			    {
				    var nodeObj = (JsonObject) node;
				    var type = nodeObj["type"].String;
				    switch (type)
				    {
					    case "Entry":
					    {
						    entry = new Entry(nodeObj["ID"].String);
						    particles.Add(entry);
						    break;
					    }
					    case "Invoker":
					    {
						    var actionGroup = nodeObj["actionGroup"].ActionGroup;
						    string[] funcNames;
						    string functionName;
						    int funcIndex = -1;
						    var execute = true;
						    if (actionGroup == ActionGroup.Skill)
						    {
							    funcIndex = 0;
							    funcNames = new[] { nodeObj["functionID"].String };
							    functionName = nodeObj["functionName"].String;
							    if(nodeObj["executeSkill"] != null)
								    execute = nodeObj["executeSkill"].Bool;
						    }
						    else if (actionGroup == ActionGroup.Inventory)
						    {
							    //funcIndex = nodeObj["functionID"].Int;
							    funcNames = AgentFunctions.GetFunctions<InventoryActionInterface>();
							    functionName = nodeObj["functionName"].String;
						    }
						    else
						    {
							    //funcIndex = nodeObj["functionID"].Int;
							    funcNames = AgentFunctions.GetFunctions<MotionActionInterface>();
							    functionName = nodeObj["functionName"].String;
						    }
						    
//_____________________________________CAUTION WORK IN PROGRESS________________________
						    //Array.Sort(funcNames, StringComparer.InvariantCulture);
						    for(var i = 0; i < funcNames.Length; i++)
							    if (funcNames[i] == functionName)
								    funcIndex = i;
						    if (funcIndex == -1)
							    funcIndex =  nodeObj["functionID"].Int;
//_____________________________________________________________________________________
						    
						    var inv = new Invoker(AgentFunctions.CreateAction(funcNames[funcIndex], gameObject, 
								    actionGroup, execute), nodeObj["ID"].String);
						    particles.Add(inv);
						    break;
					    }
					    case "Observer":
					    {
						    var funcIndex = nodeObj["functionID"].Int;
						    var conditionGroup = nodeObj["conditionGroup"].ConditionGroup;
						    var funcNames = AgentFunctions.GetConditionStrings(conditionGroup);
                            var functionName = nodeObj["functionName"].String;
                            var obs = new Observer(AgentFunctions.CreateCondition(functionName, gameObject, 
							    conditionGroup, nodeObj["ID"].Int), nodeObj["ID"].String);
						    particles.Add(obs);
						    break;
					    }
					    case "Loop":
					    {
						    var funcIndex = nodeObj["functionID"].Int;
						    var conditionGroup = nodeObj["conditionGroup"].ConditionGroup;
						    var funcNames = AgentFunctions.GetConditionStrings(conditionGroup);
                            var functionName = nodeObj["functionName"].String;
						    var reverse = nodeObj["reverse"] != null && nodeObj["reverse"].Bool;
                            var loop = new Loop(core, reverse, 
							    AgentFunctions.CreateCondition(functionName, gameObject, 
								    conditionGroup, nodeObj["ID"].Int), nodeObj["ID"].String);
						    particles.Add(loop);
						    break;
					    }
				    }
			    }
		    
		    // Now we need to connect the Components.
		    var jTransitions = jObj["transitions"].Objects;
		    foreach (var trans in jTransitions)
		    {
			    // Retrieve the needed information from data.
			    var transition = (JsonObject) trans;
				
			    var startId = transition["startID"].String;
			    var endId = transition["endID"].String;
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
			    
			    // Connect proper Components by their IDs.
			    try
			    {
				    var type = ComponentTypeById(particles, startId);
				    if (type == "Observer")
				    {
					    if (isNeg)
						    ((Observer) particles.FirstOrDefault(particle => particle.Id == startId))
							    .ConnectWith_Else(particles.FirstOrDefault(particle => particle.Id == endId),
								    minRate, maxRate, minCooldown, maxCooldown, terminate);
					    else
						    particles.FirstOrDefault(particle => particle.Id == startId)
							    .ConnectWith(particles.FirstOrDefault(particle => particle.Id == endId), 
								    minRate, maxRate, minCooldown, maxCooldown, terminate);
				    }
				    else if (type == "Loop")
				    {
					    if (isNeg)
						    ((Loop) particles.FirstOrDefault(particle => particle.Id == startId))
							    .ConnectWith_End(particles.FirstOrDefault(particle => particle.Id == endId), 
								    minRate, maxRate, minCooldown, maxCooldown, terminate);
					    else
						    particles.FirstOrDefault(particle => particle.Id == startId)
							    .ConnectWith(particles.FirstOrDefault(particle => particle.Id == endId), 
								    minRate, maxRate, minCooldown, maxCooldown, terminate);
				    }
				    else
					    particles.FirstOrDefault(particle => particle.Id == startId)
						    .ConnectWith(particles.FirstOrDefault(particle => particle.Id == endId), 
							    minRate, maxRate, minCooldown, maxCooldown, terminate);
			    }
			    catch(NullReferenceException){Debug.Log("Something is wrong with data.");}
		    }
		    
		    // Set the Entry.
		    core.Entry = entry;
		    return core;
	    }

	    /// <summary>
	    /// Find component in the list by its ID and return its type as a string.
	    /// </summary>
	    /// <param name="list"></param>
	    /// <param name="id"></param>
	    /// <returns></returns>
	    private static string ComponentTypeById(List<EliotComponent> list, string id)
	    {
		    foreach (var element in list)
		    {
			    if (element.Id == id)
			    {
				    if (element is Entry) return "Entry";
				    if (element is Invoker) return "Invoker";
				    if (element is Observer) return "Observer";
				    if (element is Loop) return "Loop";
			    }
		    }
		    return null;
	    }
    }
}
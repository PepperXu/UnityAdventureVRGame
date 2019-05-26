using System.Collections.Generic;

namespace Eliot.BehaviourEngine
{
	/// <summary>
	/// Keeps information about whatever method needs to be invoked.
	/// </summary>
	public delegate void EliotAction();
	
	/// <summary>
	/// Invoker is responsible for running the method it holds on the proper object instance.
	/// </summary>
	public class Invoker : EliotComponent
	{
		/// Hold the method to be invoked.
		private readonly EliotAction _action;
		/// List of transitions that link this component to the other ones in the model.
		private readonly List<Transition> _transitions = new List<Transition>();
		
		/// <summary>
		/// Empty constructor.
		/// </summary>
		public Invoker(){}
		
		/// <summary>
		/// Initialize the component.
		/// </summary>
		/// <param name="qa"></param>
		/// <param name="id"></param>
		public Invoker(EliotAction qa, string id = "I") : base(id)
		{
			if (qa != null) _action += qa;
		}

		/// <summary>
		/// Make Invoker do its job.
		/// </summary>
		public override void Update()
		{
			if (_action != null)
				_action();
			
			if (_transitions.Count == 0) return;
			foreach(var transition in _transitions)
				if (!transition.Update()) break;
		}

		/// <summary>
		/// Create transition between Invoker and other component.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="minRate"></param>
		/// <param name="maxRate"></param>
		/// <param name="minCooldown"></param>
		/// <param name="maxCooldown"></param>
		/// <param name="terminate"></param>
		public override void ConnectWith(EliotComponent p, int minRate, int maxRate, 
			float minCooldown, float maxCooldown, bool terminate)
		{
			if (!p) return;
			_transitions.Add(new Transition(p, minRate, maxRate, minCooldown, maxCooldown, terminate));
		}
	}
}

using System.Collections.Generic;

namespace Eliot.BehaviourEngine
{
	/// <summary>
	/// Entry point of the behaviour. By default, the first element in the model to get updated.
	/// </summary>
	public class Entry : EliotComponent
	{
		/// List of transitions that link this component to the other ones in the model.
		private readonly List<Transition> _transitions = new List<Transition>();
		
		/// <summary>
		/// Initialize the component.
		/// </summary>
		/// <param name="id"></param>
		public Entry(string id = "E"):base(id){}

		/// <summary>
		/// Make Entry do its job.
		/// </summary>
		public override void Update()
		{
			if (_transitions.Count == 0) return;
			foreach(var transition in _transitions)
				if (!transition.Update()) break;
		}

		/// <summary>
		/// Create transition between Entry and other component.
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

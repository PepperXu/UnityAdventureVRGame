using System.Collections.Generic;

namespace Eliot.BehaviourEngine
{
	/// <summary>
	/// Keeps information about whatever condition needs to be checked.
	/// </summary>
	public delegate bool EliotCondition();
	
	/// <summary>
	/// Observer is responsible for checking the specific condition
	/// and take appropriate action as far as the result goes.
	/// </summary>
	public class Observer : EliotComponent
	{
		/// Hold the boolean method and target instance to check the condition whenever needed.
		private readonly EliotCondition _condition;
		/// Links to other components that get activated when the result of checking condition is true.
		private readonly List<Transition> _transitionsIf = new List<Transition>();
		/// Links to other components that get activated when the result of checking condition is false.
		private readonly List<Transition> _transitionsElse = new List<Transition>();
		
		/// <summary>
		/// Empty constructor.
		/// </summary>
		public Observer(){}
		
		/// <summary>
		/// Initialize the component.
		/// </summary>
		/// <param name="qc"></param>
		/// <param name="id"></param>
		public Observer(EliotCondition qc, string id = "O") : base(id)
		{
			if(qc != null) _condition = qc;
		}

		/// <summary>
		/// Make Observer do its job.
		/// </summary>
		public override void Update()
		{
			if (_condition == null) return;
			
			switch (_condition())
			{
				case true:
				{
					if (_transitionsIf.Count == 0) return;
					foreach(var transition in _transitionsIf)
						if (!transition.Update()) break;
					break;
				}
				case false:
				{
					if (_transitionsElse.Count == 0) return;
					foreach(var transition in _transitionsElse)
						if (!transition.Update()) break;
					break;
				}
			}
		}
		
		/// <summary>
		/// Create transition in 'True' group between Observer and other component.
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
			_transitionsIf.Add(new Transition(p, minRate, maxRate, minCooldown, maxCooldown, terminate));
		}
		
		/// <summary>
		/// Create transition in 'False' group between Observer and other component.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="minRate"></param>
		/// <param name="maxRate"></param>
		/// <param name="minCooldown"></param>
		/// <param name="maxCooldown"></param>
		/// <param name="terminate"></param>
		public void ConnectWith_Else(EliotComponent p, int minRate, int maxRate,
			float minCooldown, float maxCooldown, bool terminate)
		{
			if (!p) return;
			_transitionsElse.Add(new Transition(p, minRate, maxRate, minCooldown, maxCooldown, terminate));
		}
	}
}

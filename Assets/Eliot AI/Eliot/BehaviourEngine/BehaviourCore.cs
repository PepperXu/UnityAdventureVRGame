namespace Eliot.BehaviourEngine
{
	/// <summary>
	/// Driver of the Behavoiur model. Activates cand controls active model component.
	/// </summary>
	public class BehaviourCore
	{
		/// <summary>
		/// Entry point of the Behaviour. By default
		/// activates connected elements every update.
		/// </summary>
		public Entry Entry
		{
			get { return _entry;}
			set
			{
				_entry = value;
				ActiveComponent = value;
			}
		}
		
		/// <summary>
		/// Current active element of the model. Can be of type Entry or Loop.
		/// </summary>
		public EliotComponent ActiveComponent { get; set; }
		
		/// Entry point of the Behaviour. 
		private Entry _entry;

		/// <summary>
		/// Update current active component of the model.
		/// </summary>
		public void Update(){
			if (ActiveComponent != null)
				ActiveComponent.Update();
		}
	}
}

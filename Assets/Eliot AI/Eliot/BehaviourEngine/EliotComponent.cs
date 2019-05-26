namespace Eliot.BehaviourEngine
{
	/// <summary>
	/// Component is a base class for all objects that
	/// interract with each other in the process
	/// of the behaviour lifetime.
	/// </summary>
	public abstract class EliotComponent
	{
		/// <summary>
		/// Defines a unique name of the component in the model.
		/// </summary>
		public string Id { get { return _id; } }
		
		/// Defines a unique name of the component in the model.
		private readonly string _id;
		/// Returns if the object already exists.
		private readonly bool _iExist;
		
		/// <summary>
		/// Initialize component.
		/// </summary>
		/// <param name="id"></param>
		protected EliotComponent(string id = "P")
		{
			_iExist = true;
			_id = id;
		}

		/// <summary>
		/// Make component do its job.
		/// </summary>
		public abstract void Update();
		
		/// <summary>
		/// Add a transition between this component and the other one.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="minRate"></param>
		/// <param name="maxRate"></param>
		/// <param name="minCooldown"></param>
		/// <param name="maxCooldown"></param>
		/// <param name="terminate"></param>
		public abstract void ConnectWith(EliotComponent p, int minRate, int maxRate, 
			float minCooldown, float maxCooldown, bool terminate);
		
		/// <summary>
		/// Overload 'true' operator for this component.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static bool operator true(EliotComponent p){return p._iExist;}
		
		/// <summary>
		/// Overload 'false' operator for this component.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static bool operator false(EliotComponent p){return p._iExist == false;}
		
		/// <summary>
		/// Overload '!' operator for this component.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static bool operator !(EliotComponent p){return p._iExist == false;}
	}
}

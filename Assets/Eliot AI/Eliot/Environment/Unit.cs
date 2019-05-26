using System.Collections.Generic;
using UnityEngine;

namespace Eliot.Environment
{
	/// <summary>
	/// The buffer of object's characteristics that Agent can easily understand.
	/// Add Unit component to all the objects that you want Agent to interract well with.
	/// </summary>
	public class Unit : MonoBehaviour
	{
		/// <summary>
		/// User-defined type of the unit. Depends highly on the context of the game.
		/// </summary>
		public UnitType Type
		{
			get { return _type;}
			set { _type = value;} 
		}
		
		/// <summary>
		/// Team to which the unit belongs.
		/// </summary>
		public string Team
		{
			get { return _team;}
			set { _team = value;} 
		}

#if PIXELCRUSHERS_LOVEHATE
		/// <summary>
		/// Whether to use the Pixelcrushers LoveHate system to check if another Unit is a friend.
		/// </summary>
		public bool UseLoveHate
		{
			get { return _useLoveHate; }
		}
#endif
		
		/// User-defined type of the unit. Depends highly on the context of the game.
		[SerializeField] private UnitType _type;
		/// Team to which the unit belongs.
		[SerializeField] private string _team;
		/// Names of teams that are concidered friendly to this unit.
		[SerializeField] private List<string> _friendTeams = new List<string>();
#if PIXELCRUSHERS_LOVEHATE
		/// Whether to use the Pixelcrushers LoveHate system to check if another Unit is a friend.
		[SerializeField] private bool _useLoveHate = true;
#endif

		/// <summary>
		/// Check wheather the given unit belongs to the same team or to one of the friendly teams.
		/// </summary>
		/// <param name="unit"></param>
		/// <returns></returns>
		public bool IsFriend(Unit unit)
		{
#if PIXELCRUSHERS_LOVEHATE
			if (!_useLoveHate)
				return _team == unit._team || _friendTeams.Contains(unit._team);
			else
			{
				var myFactionMember = GetComponent<PixelCrushers.LoveHate.FactionMember>();
				if (!myFactionMember)
					return _team == unit._team || _friendTeams.Contains(unit._team);
				var otherFactionMember = unit.GetComponent<PixelCrushers.LoveHate.FactionMember>();
				if (!otherFactionMember)
					return _team == unit._team || _friendTeams.Contains(unit._team);
				return myFactionMember.GetAffinity(otherFactionMember) >= 0;
			}
#else
			return _team == unit._team || _friendTeams.Contains(unit._team);
#endif
		}
	}
}
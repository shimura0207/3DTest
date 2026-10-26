using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity_Data : ScriptableObject
{	
	public List<Sheet> sheets = new List<Sheet> ();

	[System.SerializableAttribute]
	public class Sheet
	{
		public string name = string.Empty;
		public List<Param> list = new List<Param>();
	}

	[System.SerializableAttribute]
	public class Param
	{
		
		public double cardID;
		public bool name;
		public bool rarityID;
		public bool themeID;
		public bool suportRoleID;
		public bool FirstRaceID;
		public bool SecondRaceID;
		public bool ThildRaceID;
		public bool SIZE;
		public bool ATK;
		public bool HP;
		public bool CardType;
		public bool ability;
	}
}


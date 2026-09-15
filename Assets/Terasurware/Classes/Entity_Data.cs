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
		
		public int CardID;
		public int RarityID;
		public int ThemeID;
		public int RaceID;
		public int ATK;
		public int HP;
		public int SIZE;
	}
}


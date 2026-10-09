using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity_CardData : ScriptableObject
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
		
		public int cardID;
		public string name;
		public int rarityID;
		public int themeID;
		public int KoyakuID;
		public int size;
		public int atk;
		public int hp;
		public int Type;
		public string ability;
	}
}


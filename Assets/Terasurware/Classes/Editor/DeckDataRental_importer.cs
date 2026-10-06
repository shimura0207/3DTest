using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class DeckDataRental_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/Resources/MasterData/DeckDataRental.xlsx";
	private static readonly string exportPath = "Assets/Resources/MasterData/DeckDataRental.asset";
	private static readonly string[] sheetNames = { "rental", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			Entity_RentalDeck data = (Entity_RentalDeck)AssetDatabase.LoadAssetAtPath (exportPath, typeof(Entity_RentalDeck));
			if (data == null) {
				data = ScriptableObject.CreateInstance<Entity_RentalDeck> ();
				AssetDatabase.CreateAsset ((ScriptableObject)data, exportPath);
				data.hideFlags = HideFlags.NotEditable;
			}
			
			data.sheets.Clear ();
			using (FileStream stream = File.Open (filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
				IWorkbook book = null;
				if (Path.GetExtension (filePath) == ".xls") {
					book = new HSSFWorkbook(stream);
				} else {
					book = new XSSFWorkbook(stream);
				}
				
				foreach(string sheetName in sheetNames) {
					ISheet sheet = book.GetSheet(sheetName);
					if( sheet == null ) {
						Debug.LogError("[QuestData] sheet not found:" + sheetName);
						continue;
					}

					Entity_RentalDeck.Sheet s = new Entity_RentalDeck.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i<= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						Entity_RentalDeck.Param p = new Entity_RentalDeck.Param ();
						
					cell = row.GetCell(0); p.DeckID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.CardID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(2); p.Count = (int)(cell == null ? 0 : cell.NumericCellValue);
						s.list.Add (p);
					}
					data.sheets.Add(s);
				}
			}

			ScriptableObject obj = AssetDatabase.LoadAssetAtPath (exportPath, typeof(ScriptableObject)) as ScriptableObject;
			EditorUtility.SetDirty (obj);
		}
	}
}

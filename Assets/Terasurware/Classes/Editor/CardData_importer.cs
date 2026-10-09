using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

public class CardData_importer : AssetPostprocessor {
	private static readonly string filePath = "Assets/Resources/MasterData/CardData.xlsx";
	private static readonly string exportPath = "Assets/Resources/MasterData/CardData.asset";
	private static readonly string[] sheetNames = { "Data", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			Entity_CardData data = (Entity_CardData)AssetDatabase.LoadAssetAtPath (exportPath, typeof(Entity_CardData));
			if (data == null) {
				data = ScriptableObject.CreateInstance<Entity_CardData> ();
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

					Entity_CardData.Sheet s = new Entity_CardData.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i<= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						Entity_CardData.Param p = new Entity_CardData.Param ();
						
					cell = row.GetCell(0); p.cardID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.name = (cell == null ? "" : cell.StringCellValue);
					cell = row.GetCell(3); p.rarityID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(5); p.themeID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(7); p.KoyakuID = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(8); p.size = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(9); p.atk = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(10); p.hp = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(12); p.Type = (int)(cell == null ? 0 : cell.NumericCellValue);
					cell = row.GetCell(13); p.ability = (cell == null ? "" : cell.StringCellValue);
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

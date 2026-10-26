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
	private static readonly string[] sheetNames = { "Data","Type","Name","Rarity","Theme","Ability","Status","Race","Koyaku", };
	
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string asset in importedAssets) {
			if (!filePath.Equals (asset))
				continue;
				
			Entity_Data data = (Entity_Data)AssetDatabase.LoadAssetAtPath (exportPath, typeof(Entity_Data));
			if (data == null) {
				data = ScriptableObject.CreateInstance<Entity_Data> ();
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

					Entity_Data.Sheet s = new Entity_Data.Sheet ();
					s.name = sheetName;
				
					for (int i=1; i<= sheet.LastRowNum; i++) {
						IRow row = sheet.GetRow (i);
						ICell cell = null;
						
						Entity_Data.Param p = new Entity_Data.Param ();
						
					cell = row.GetCell(0); p.cardID = (cell == null ? 0.0 : cell.NumericCellValue);
					cell = row.GetCell(1); p.name = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(3); p.rarityID = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(5); p.themeID = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(7); p.suportRoleID = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(9); p.FirstRaceID = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(11); p.SecondRaceID = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(13); p.ThildRaceID = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(14); p.SIZE = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(15); p.ATK = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(16); p.HP = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(18); p.CardType = (cell == null ? false : cell.BooleanCellValue);
					cell = row.GetCell(19); p.ability = (cell == null ? false : cell.BooleanCellValue);
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

/*
 *  @file   ErrorWordMasterUtility
 *  @author oorui
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// すべての禁止ワードを取得
/// </summary>
public class ErrorWordMasterUtility {  
    
    /// <summary>
    /// すべての禁止ワードを取得する
    /// </summary>
    /// <returns>禁止ワードのリスト</returns>
    public static List<Entity_ErrorWord.Param> GetErrorWord() {

        // マスターデータから禁止ワード一覧を取得する
        var errorWordList = MasterDataManager.errorWord[0];

        // 取得した一覧をそのまま返す
        return errorWordList;
    }

}

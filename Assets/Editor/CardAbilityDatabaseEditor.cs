/*
 * @file    CardAbilityDatabaseEditor
 * @author  Riku
 */

using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;


/// <summary>
/// CardAbilityDatabaseのエディター拡張
/// </summary>
[CustomEditor(typeof(CardAbilityDatabase))]
public class CardAbilityDatabaseEditor : Editor {
    // 入力されたカードID
    private int inputCardID = -1;
    // 入力された効果番号
    private int inputAbilityID = -1;
    // 入力されたカード効果の種類
    private AbilityType inputAbilityType = AbilityType.None;
    // 入力された特殊効果
    private SpecialAbility inputSpecialAbility = SpecialAbility.None;
    // 入力されたキーワード効果
    private KeywordAbility inputKeywordAbility = KeywordAbility.None;
    // 入力された効果の発動タイミング
    private AbilityTiming inputAbilityTiming = AbilityTiming.None;
    // 入力された効果の発動対象
    private AbilityTarget inputAbilityTarget = AbilityTarget.None;
    // 入力された効果の発動条件
    private AbilityCondition inputAbilityCondition = AbilityCondition.None;
    // 入力された効果の発動内容
    private AbilityProcess inputAbilityProcess = AbilityProcess.None;

    public override void OnInspectorGUI() {
        CardAbilityDatabase database = (CardAbilityDatabase)target;
        
        // カードID設定
        inputCardID = EditorGUILayout.IntField("CardID", inputCardID);
        // 効果番号設定
        inputAbilityID = EditorGUILayout.IntField("AbilityID", inputAbilityID);
        // 効果の種類設定
        inputAbilityType = (AbilityType)UseEnumPopup(
            "AbilityType", inputAbilityType);

        // 効果の種類によって表示項目を変更
        switch (inputAbilityType) {
            case AbilityType.Normal:
                // 効果の発動タイミングの設定
                inputAbilityTiming = (AbilityTiming)UseEnumPopup(
                    "AbilityTiming", inputAbilityTiming);
                // 効果の発動対象の設定
                inputAbilityTarget = (AbilityTarget)UseEnumPopup(
                    "AbilityTarget", inputAbilityTarget);
                // 効果の発動条件の設定
                inputAbilityCondition = (AbilityCondition)UseEnumPopup(
                    "AbilityCondition", inputAbilityCondition);
                // 効果の発動内容の設定
                inputAbilityProcess = (AbilityProcess)UseEnumPopup(
                    "AbilityProcess", inputAbilityProcess);
                break;
            case AbilityType.Special:
                // 特殊効果の設定
                inputSpecialAbility = (SpecialAbility)UseEnumPopup(
                    "SpecialAbility", inputSpecialAbility);
                break;
            case AbilityType.Keyword:
                // キーワード効果の設定
                inputKeywordAbility = (KeywordAbility)UseEnumPopup(
                    "KeywordAbility", inputKeywordAbility);
                break;
        }

        // 追加ボタン
        if (GUILayout.Button("AddList")) {
            if (inputCardID != -1 && inputAbilityID != -1) {
                // 設定した内容を纏める
                CardAbilityDatabase.AbilityData inputData = new CardAbilityDatabase.AbilityData() {
                    abilityID = inputCardID,
                    abilityType = inputAbilityType,
                    abilityTiming = inputAbilityTiming,
                    abilityTarget = inputAbilityTarget,
                    abilityCondition = inputAbilityCondition,
                    abilityProcess = inputAbilityProcess,
                    specialAbility = inputSpecialAbility,
                    keywordAbility = inputKeywordAbility
                };

                // 設定したデータを追加
                database.AddCardAbilityDataList(inputCardID, inputAbilityID, inputData);

                // 変更をアセットに保存
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();

                // 設定した値をリセット
                inputCardID = -1;
                inputAbilityID = -1;
                inputAbilityType = AbilityType.None;
                inputSpecialAbility = SpecialAbility.None;
                inputKeywordAbility = KeywordAbility.None;
                inputAbilityTiming = AbilityTiming.None;
                inputAbilityTarget = AbilityTarget.None;
                inputAbilityCondition = AbilityCondition.None;
                inputAbilityProcess = AbilityProcess.None;
            }
            else {
                // カードIDか効果番号が入っていなければ警告
                Debug.LogWarning("入力データが十分ではありません。");
            }
        }

        // 追加ボタンとリスト表示を分けるバー表示
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        int cardIndex = -1;
        int abilityIndex = -1;
        bool delete = false;
        foreach (var data in database.CardAbilityDataList) {
            abilityIndex = -1;
            cardIndex++;
            EditorGUILayout.BeginVertical("box");
            // カードID表示
            EditorGUILayout.LabelField($"CardID: {data.cardID}", EditorStyles.boldLabel);
            foreach (var ability in data.abilityList) {
                abilityIndex++;
                // 効果表示
                switch (ability.abilityType) {
                    case AbilityType.Normal:
                        EditorGUILayout.LabelField("AbilityTiming", ability.abilityTiming.ToString());
                        EditorGUILayout.LabelField("AbilityTarget", ability.abilityTarget.ToString());
                        EditorGUILayout.LabelField("AbilityCondition", ability.abilityCondition.ToString());
                        EditorGUILayout.LabelField("AbilityProcess", ability.abilityProcess.ToString());
                        break;
                    case AbilityType.Special:
                        EditorGUILayout.LabelField("SpecialAbility", ability.specialAbility.ToString());
                        break;
                    case AbilityType.Keyword:
                        EditorGUILayout.LabelField("KeywordAbility", ability.keywordAbility.ToString());
                        break;
                }
                // 登録されているデータを削除するボタン
                if (GUILayout.Button("DeleteData")) {
                    // データ削除予約
                    delete = true;
                    break;
                }
            }
            EditorGUILayout.EndVertical();
            if (delete) break;
        }
        // 削除
        if (delete) {
            database.DeleteCardAbilityData(cardIndex, abilityIndex);
            // 変更をアセットに保存
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// NoneとMaxを無視したEnumPopup
    /// </summary>
    /// <param name="label">ラベル</param>
    /// <param name="useEnum">使うEnum</param>
    /// <returns></returns>
    private System.Enum UseEnumPopup(string label, System.Enum useEnum) {
        return EditorGUILayout.EnumPopup(
            new GUIContent(label),
            useEnum,
            // NoneとMaxを無視する
            enumValue => 
                enumValue.ToString() != "None" &&
                enumValue.ToString() != "Max", 
            false);
    }
}

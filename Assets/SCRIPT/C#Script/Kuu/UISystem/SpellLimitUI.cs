/*
 * @file    SpellLimitUI.cs
 * @author  kuu
 */
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpellLimitUI : MonoBehaviour
{
    // 自分のスペルカウントテキスト
    [SerializeField] private TMP_Text spellCount = null;

    // スペルを唱えられる最大数
    private const int SPELL_MAX = 3;

    // 省略
    private BattleSystemManager battleSystemManager = BattleSystemManager.instance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        var spellLimit = 0;

        // カウントするのは自分のターンのみ
        if (battleSystemManager.turnPlayer == PlayerType.Self) {
            spellLimit = battleSystemManager.spellCastCount;
        }
        else {
            spellLimit = 0;
        }
        // スペルの回数をテキストとして表示する
        spellCount.text = spellLimit + "/" + SPELL_MAX;
    }
}

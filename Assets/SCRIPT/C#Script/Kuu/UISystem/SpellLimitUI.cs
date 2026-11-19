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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var spellLimit = BattleSystemManager.instance.spellCastCount + 1;
        // スペルの回数をテキストとして表示する
        spellCount.text = "Spell:" + spellLimit + "/" + SPELL_MAX;
    }
}

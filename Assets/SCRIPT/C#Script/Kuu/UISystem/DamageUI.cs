/*
 * @file    DamageUI.cs
 * @author  kuu
 */
using IO.Swagger.Model;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    // ダメージテキスト
    [SerializeField] private TMP_Text damageText = null;

    // アルファ値の上げ幅の定数
    private const float ALPHA_UPPER = 0.005f;

    // position.yの上げ幅の定数
    private const float YPOS_UPPER = 0.0003f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ダメージを参照する
        damageText.text = "" + UIManager.instance.damageValue;
        // 徐々に透明にする
        damageText.alpha -= ALPHA_UPPER;
        // 徐々に上げる
        damageText.transform.position += new Vector3(0.0f, YPOS_UPPER, 0.0f);
        // アルファ値が0以下なら削除
        if (damageText.alpha < 0.0f) {
            Destroy(this);
        }
    }
}

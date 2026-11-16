/*
 * @file    DamageUI.cs
 * @author  kuu
 */
using TMPro;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    // ダメージテキスト
    [SerializeField] private TMP_Text damageText = null;

    // アルファ値の上げ幅の定数
    private const float ALPHA_UPPER = 0.01f;

    // position.yの上げ幅の定数
    private const float YPOS_UPPER = 0.003f;

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
        //damageText.alpha -= ALPHA_UPPER;
        // 徐々に上げる
        damageText.transform.position += new Vector3(0.0f, YPOS_UPPER, 0.0f);
        Debug.Log(damageText.transform.position);
        // アルファ値が0以下なら削除
        if (damageText.alpha < 0.0f) {
            Destroy(this);
        }
    }
}

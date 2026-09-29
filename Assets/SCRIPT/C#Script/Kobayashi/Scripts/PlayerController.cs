/*
 * @brief  PlayerController
 * @author Kobayashi
 */

using Unity.Netcode;        // Netcode for GameObjects を使用するためのもの
using UnityEngine;


public class PlayerController : NetworkBehaviour
{
    // プレイヤーの移動速度
    [SerializeField]
    private float speed = 5f;

    private void Update()
    {
        // 自分がプレイヤーを操作する権利がなければ
        if (!IsOwner)
            // 何もしない
            return;

        // 左右移動
        float x = Input.GetAxisRaw("Horizontal");
        // 前後移動
        float z = Input.GetAxisRaw("Vertical");

        // どの方向に移動するか
        Vector3 move =
            new Vector3(
                x,
                0,
                z
            );

        // 実際に操作通りに移動を行う
        transform.position +=
            move *
            speed *
            Time.deltaTime;
    }
}
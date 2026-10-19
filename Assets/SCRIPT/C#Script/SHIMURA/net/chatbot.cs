using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chatbot : MonoBehaviour
{
    public int sharedValue;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 送信者 → サーバー
    public struct SendValueRequest : NetworkMessage {
        public int value;
    }

    // サーバー → 相手
    public struct OpponentValueMessage : NetworkMessage {
        public int value;
    }
}

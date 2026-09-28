/*
 * @brief  MainThreadDispatcher
 * @author Kobayashi
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    // MainThreadDispatcherのインスタンス化
    public static MainThreadDispatcher Instance;
    // 新しいAction用のQueueを作る
    private Queue<Action> actions = new Queue<Action>();

    /// <summary>
    /// このGameObjectが生成された時に自動的に呼ぶ
    /// </summary>
    private void Awake()
    {
        // 自信をInstanceに保存
        Instance = this;
    }

    /// <summary>
    /// 処理をQueueに追加するための関数
    /// </summary>
    /// <param name="action"></param>
    public void Enqueue(Action action)
    {
        // 複数のスレッドが同時にQueueに触れないようにする
        lock (actions)
        {
            // 受け取った処理をQueueの最後に追加する
            actions.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (actions)
        {
            // Queueに処理が残っている間、繰り返す
            while (actions.Count > 0)
            {
                // Queueの先頭にある処理を取り出す
                actions.Dequeue().Invoke();
            }
        }
    }
}
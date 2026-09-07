/*
 *  @file   SystemObject
 *  @author oorui
 */

using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ゲーム全体で使用する機能の基底
/// </summary>
public abstract class SystemObject : MonoBehaviour {
    public abstract UniTask Initialize();
}

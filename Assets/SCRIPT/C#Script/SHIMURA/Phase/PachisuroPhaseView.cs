using UnityEngine;

/// <summary>
/// シーン上の常時アクティブな管理オブジェクトにアタッチする。
/// 実行時生成のPachisuroPhaseから、登録した表示物をまとめて切り替える。
/// </summary>
[DisallowMultipleComponent]
public class PachisuroPhaseView : MonoBehaviour
{
    public static PachisuroPhaseView Instance { get; private set; }

    [Header("背景（複数登録可）")]
    [SerializeField] private GameObject[] backgroundObjects;
    [Header("役カウント画像（複数登録可）")]
    [SerializeField] private GameObject[] roleCountImageObjects;
    [Header("役テキスト（TMPも通常Textも登録可）")]
    [SerializeField] private GameObject[] roleTextObjects;
    [Header("役オーブ画像オブジェクト（複数登録可）")]
    [SerializeField] private GameObject[] roleOrbImageObjects;
    [Header("初期状態")]
    [SerializeField] private bool hideOnAwake = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("PachisuroPhaseViewはシーンに1つだけ配置してください。", this);
            return;
        }
        Instance = this;
        if (hideOnAwake) Hide();
    }

    public void Show() { SetVisible(true); }
    public void Hide() { SetVisible(false); }

    public void SetVisible(bool visible)
    {
        SetObjectsActive(backgroundObjects, visible);
        SetObjectsActive(roleCountImageObjects, visible);
        SetObjectsActive(roleTextObjects, visible);
        SetObjectsActive(roleOrbImageObjects, visible);
    }

    private void SetObjectsActive(GameObject[] objects, bool visible)
    {
        if (objects == null) return;
        foreach (GameObject target in objects)
        {
            if (target == null) continue;
            // 管理オブジェクト自身やその親を消すと、次回の管理が難しくなるため除外。
            if (transform == target.transform || transform.IsChildOf(target.transform))
            {
                Debug.LogWarning("表示管理オブジェクト自身・親は登録できません。管理オブジェクトを非表示対象の外に置いてください。", this);
                continue;
            }
            target.SetActive(visible);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

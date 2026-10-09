using UnityEngine;
using System.Collections;
/// <summary>
/// リール前のガラスの色・透明度を管理します。
/// </summary>
public class grassFlash : MonoBehaviour {
    [Header("右側のガラス")]
    [SerializeField] private GameObject rightTopGlass;
    [SerializeField] private GameObject rightGlass;
    [SerializeField] private GameObject rightBottomGlass;

    [Header("中央のガラス")]
    [SerializeField] private GameObject centerTopGlass;
    [SerializeField] private GameObject centerGlass;
    [SerializeField] private GameObject centerBottomGlass;

    [Header("左側のガラス")]
    [SerializeField] private GameObject leftTopGlass;
    [SerializeField] private GameObject leftGlass;
    [SerializeField] private GameObject leftBottomGlass;

    /// <summary>
    /// 操作するガラスの位置。
    /// </summary>
    public enum GlassPosition {
        RightTop,
        Right,
        RightBottom,

        CenterTop,
        Center,
        CenterBottom,

        LeftTop,
        Left,
        LeftBottom
    }

    // ガラスごとに複製したマテリアル。
    private Material[] glassMaterials;

    // マテリアルの色プロパティID。
    private int[] colorPropertyIds;

    // 初期状態へ戻すために保存する色。
    private Color[] initialColors;

    private void Awake() {
        // GlassPositionと同じ順番に並べます。
        GameObject[] glassObjects =
        {
            rightTopGlass,
            rightGlass,
            rightBottomGlass,

            centerTopGlass,
            centerGlass,
            centerBottomGlass,

            leftTopGlass,
            leftGlass,
            leftBottomGlass
        };

        int count = glassObjects.Length;

        glassMaterials = new Material[count];
        colorPropertyIds = new int[count];
        initialColors = new Color[count];

        for (int i = 0; i < count; i++) {
            GameObject glass = glassObjects[i];

            // 未登録のガラスはスキップ。
            if (glass == null)
                continue;

            // 自身、または子オブジェクトのRendererを取得。
            Renderer glassRenderer =
                glass.GetComponentInChildren<Renderer>(true);

            if (glassRenderer == null ||
                glassRenderer.sharedMaterial == null) {
                Debug.LogWarning(
                    $"{glass.name}にRendererまたはマテリアルがありません。",
                    glass
                );

                continue;
            }

            Material source = glassRenderer.sharedMaterial;
            int colorId;

            // URPなどの色プロパティ。
            if (source.HasProperty("_BaseColor")) {
                colorId = Shader.PropertyToID("_BaseColor");
            }
            // Standardなどの色プロパティ。
            else if (source.HasProperty("_Color")) {
                colorId = Shader.PropertyToID("_Color");
            }
            else {
                Debug.LogWarning(
                    $"{glass.name}のマテリアルに対応する色プロパティがありません。",
                    glass
                );

                continue;
            }

            // このガラス専用のマテリアルを作成。
            Material instance = new Material(source);
            glassRenderer.sharedMaterial = instance;

            glassMaterials[i] = instance;
            colorPropertyIds[i] = colorId;
            initialColors[i] = instance.GetColor(colorId);
        }
    }

    /// <summary>
    /// 指定したガラスの色と透明度を変更します。
    /// opacity：0で透明、1で不透明。
    /// </summary>
    public void SetGlassColor(
        GlassPosition position,
        Color color,
        float opacity = 1f) {
        int index = (int)position;

        if (glassMaterials == null ||
            index < 0 ||
            index >= glassMaterials.Length ||
            glassMaterials[index] == null) {
            return;
        }

        color.a = Mathf.Clamp01(opacity);

        glassMaterials[index].SetColor(
            colorPropertyIds[index],
            color
        );
    }

    /// <summary>
    /// 全ガラスの色と透明度をまとめて変更します。
    /// </summary>
    public void SetAllGlassColor(Color color, float opacity = 1f) {
        if (glassMaterials == null)
            return;

        for (int i = 0; i < glassMaterials.Length; i++) {
            SetGlassColor((GlassPosition)i, color, opacity);
        }
    }

    /// <summary>
    /// 指定したガラスを初期の色・透明度へ戻します。
    /// </summary>
    public void ResetGlass(GlassPosition position) {
        int index = (int)position;

        if (glassMaterials == null ||
            index < 0 ||
            index >= glassMaterials.Length ||
            glassMaterials[index] == null) {
            return;
        }

        glassMaterials[index].SetColor(
            colorPropertyIds[index],
            initialColors[index]
        );
    }

    /// <summary>
    /// 全ガラスを初期の色・透明度へ戻します。
    /// </summary>
    public void ResetAllGlass() {
        if (glassMaterials == null)
            return;

        for (int i = 0; i < glassMaterials.Length; i++) {
            ResetGlass((GlassPosition)i);
        }
    }

    private void OnDestroy() {
        if (glassMaterials == null)
            return;

        // 実行中に作成した専用マテリアルを破棄。
        foreach (Material material in glassMaterials) {
            if (material != null)
                Destroy(material);
        }
    }


    public IEnumerator FlashTopRow(
    Color flashColor,
    int flashCount = 5,
    float interval = 0.15f,
    float opacity = 1f) {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0f, interval));

        for (int i = 0; i < flashCount; i++) {
            // 上段3か所を同時に点灯。
            SetGlassColor(GlassPosition.LeftTop, Color.black, 0.9f);
            SetGlassColor(GlassPosition.CenterTop, Color.black, 0.9f);
            SetGlassColor(GlassPosition.RightTop, Color.black,0.9f);

            yield return wait;

            // 元の色・透明度に戻して消灯。
            ResetGlass(GlassPosition.LeftTop);
            ResetGlass(GlassPosition.CenterTop);
            ResetGlass(GlassPosition.RightTop);

            yield return wait;
        }
    }

    public IEnumerator FlashChary(
   Color flashColor,
   int flashCount = 5,
   float interval = 0.15f,
   float opacity = 1f) {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0f, interval));

        for (int i = 0; i < flashCount; i++) {
            // 上段3か所を同時に点灯。
            SetGlassColor(GlassPosition.LeftBottom, Color.black, 0.9f);
            

            yield return wait;

            // 元の色・透明度に戻して消灯。
            ResetGlass(GlassPosition.LeftBottom);
            

            yield return wait;
        }
    }
}
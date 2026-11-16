using TMPro;
using UnityEngine;

/// <summary>
/// SlotReelControllerが役数字のTMPへ自動追加する表示専用コンポーネント。
/// 通常数値の右側にSeven由来の虹+N、チャンス目由来の紫+Nを並べる。
/// </summary>
[DisallowMultipleComponent]
public class RoleStockBonusText : MonoBehaviour
{
    private TMP_Text label;
    private int rainbowStart;
    private int rainbowLength;

    public void SetCounts(TMP_Text target, int stock, int seven, int chance)
    {
        label = target;
        string number = stock.ToString();
        string rainbow = seven > 0 ? "+" + seven : string.Empty;
        rainbowStart = number.Length + 1;
        rainbowLength = rainbow.Length;
        label.richText = true;
        label.text = number
            + (seven > 0 ? " <color=#FFFFFF>" + rainbow + "</color>" : string.Empty)
            + (chance > 0 ? " <color=#C060FF>+" + chance + "</color>" : string.Empty);
    }

    private void LateUpdate()
    {
        if (label == null || !label.isActiveAndEnabled || rainbowLength == 0) return;
        // TMPが生成した元の文字色を毎回取得し、通常数値・紫部分は変更しない。
        label.ForceMeshUpdate();
        TMP_TextInfo info = label.textInfo;
        int end = Mathf.Min(rainbowStart + rainbowLength, info.characterCount);
        for (int i = rainbowStart; i < end; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];
            if (!character.isVisible) continue;
            Color32[] colors = info.meshInfo[character.materialReferenceIndex].colors32;
            int vertex = character.vertexIndex;
            for (int corner = 0; corner < 4; corner++)
            {
                // 文字の左右・上下にも色差をつけ、+1でも虹のグラデーションを出す。
                float horizontal = (corner >= 2 ? 1f : 0f);
                float vertical = (corner == 1 || corner == 2) ? 0.22f : 0f;
                float hue = Mathf.Repeat(Time.unscaledTime * 0.25f
                    + (i - rainbowStart + horizontal) / Mathf.Max(1, rainbowLength)
                    * 0.75f + vertical, 1f);
                Color32 color = Color.HSVToRGB(hue, 0.8f, 1f);
                color.a = colors[vertex + corner].a;
                colors[vertex + corner] = color;
            }
        }
        label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}

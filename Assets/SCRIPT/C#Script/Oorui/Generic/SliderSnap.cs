using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Sliderの操作終了時に指定位置へ吸着させるクラス
/// </summary>
[RequireComponent(typeof(Slider))]
public class SliderSnap : MonoBehaviour, IPointerUpHandler {
    // 操作対象のSlider
    private Slider slider = null;

    // 吸着させる位置（0～1の割合）
    [SerializeField]
    private float[] snapPoints = { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f,
                                   0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };

    private void Awake() {
        // 同じGameObjectにあるSliderを取得する
        slider = GetComponent<Slider>();
    }

    /// <summary>
    /// マウスを離したときに最も近い位置へ吸着させる
    /// </summary>
    public void OnPointerUp(PointerEventData eventData) {
        // 現在のSlider値を0～1の割合に変換する
        float normalizedValue = Mathf.InverseLerp(
            slider.minValue,
            slider.maxValue,
            slider.value
        );

        // 最も近い吸着位置を探す
        float nearestPoint = normalizedValue;
        float minDistance = float.MaxValue;

        foreach (float point in snapPoints) {
            // 現在値と吸着位置の距離を比較する
            float distance = Mathf.Abs(normalizedValue - point);

            if (distance < minDistance) {
                minDistance = distance;
                nearestPoint = point;
            }
        }

        // 吸着位置をSliderの実際の値へ変換する
        float snappedValue = Mathf.Lerp(
            slider.minValue,
            slider.maxValue,
            nearestPoint
        );

        // Sliderの値を更新してOptionMenu側にも変更を通知する
        slider.value = snappedValue;
    }
}
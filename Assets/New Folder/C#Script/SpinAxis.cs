/// <summary>
/// リールをどの軸で回転させるかを選ぶための enum。
/// 
/// Inspector の SlotReelController から選択できます。
/// リールが横向きに回ってしまう場合は、SlotReelController 側の rotationAxis を X / Y / Z に切り替えて調整してください。
/// </summary>
public enum SpinAxis
{
    X,
    Y,
    Z
}

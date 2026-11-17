using UnityEngine;

/// <summary>
/// スロットに関係するカード効果。手動アタッチ不要。
/// Upper/GOverは場にいる間だけ有効。疲労状態には依存しない。
/// 降臨・テーブル上昇は外部のカード使用処理から明示的に呼び出す。
/// </summary>
public partial class SlotReelController {
    private PachisuroSymbolKoyakuEnum? cardFirstGameRole;
    private bool cardFirstGameStarted;

    public int CardExtraGames => GetFieldCardExtraGames(PlayerType.Self);
    public bool CardRoleUpperEnabled => HasFieldRoleUpper(PlayerType.Self);

    private int GetFieldCardExtraGames(PlayerType player) {
        CardObjectManager manager = CardObjectManager.instance;
        if (manager == null || manager.fieldObjectList == null) return 0;
        if (!manager.fieldObjectList.TryGetValue(player, out var field) || field == null) return 0;
        long total = 0;
        foreach (var entry in field) {
            CardObject card = entry.Value;
            if (card == null || card.playerType != player || card.abilityList == null) continue;
            foreach (var ability in card.abilityList) {
                if (ability.abilityType != AbilityType.Passive) continue;
                if (ability.passiveAbility != PassiveAbility.GOver) continue;
                // 他の効果関数と同様、未指定なら1として扱う。
                total += ability.abilityValue > 0 ? ability.abilityValue : 1;
                if (total >= int.MaxValue - 5) return int.MaxValue - 5;
            }
        }
        return (int)total;
    }

    private bool HasFieldRoleUpper(PlayerType player) {
        CardObjectManager manager = CardObjectManager.instance;
        if (manager == null || manager.fieldObjectList == null) return false;
        if (!manager.fieldObjectList.TryGetValue(player, out var field) || field == null) return false;
        foreach (var entry in field) {
            CardObject card = entry.Value;
            if (card == null || card.playerType != player || card.abilityList == null) continue;
            foreach (var ability in card.abilityList) {
                if (ability.abilityType == AbilityType.Passive &&
                    ability.passiveAbility == PassiveAbility.Upper) return true;
            }
        }
        return false;
    }

    public bool CanUseSlotEffectCard() {
        return !networkRemoteControl && !cardFirstGameStarted &&
            !IsAnyReelRotating();
    }

    private bool CheckSlotEffectCard(string cardName) {
        if (CanUseSlotEffectCard()) return true;
        Debug.LogWarning("[" + cardName + "] 自分の1G目のレバーオン前に使用してください。", this);
        return false;
    }

    private bool ReserveFirstGameRole(PachisuroSymbolKoyakuEnum role, string cardName) {
        if (!CheckSlotEffectCard(cardName)) return false;
        cardFirstGameRole = role;
        return true;
    }

    public bool CardBlankAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Miss, "ブランク降臨");
    public bool CardReplayAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Replay, "リプレイ降臨");
    public bool CardBellAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Bell, "ベル降臨");
    public bool CardSuikaAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Suika, "スイカ降臨");
    public bool CardCherryAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.WeakCherry, "チェリー降臨");

    public bool CardUoooo() {
        if (!CheckSlotEffectCard("うおおおお") || currentTable == int.MaxValue) return false;
        currentTable++;
        return true;
    }

    // 旧APIとの互換用。常在効果は手動で加算・予約せず、場の状態を返す。
    // オーヴァ/ロール・アッパー使用時にこれらを呼ぶ必要はない。
    public bool CardOver() => CardExtraGames > 0;
    public bool CardRoleUpper() => CardRoleUpperEnabled;

    private void BeginSlotCardEffects() {
        // 開始前に使用した降臨の予約を保持する。
        cardFirstGameStarted = false;
    }

    private PachisuroSymbolKoyakuEnum ApplySlotCardEffects(PachisuroSymbolKoyakuEnum drawnRole) {
        // 受信した役は送信側で昇格済み。再変換しない。
        if (networkRemoteControl) return drawnRole;

        bool first = !cardFirstGameStarted && slotIndex == 0;
        cardFirstGameStarted = true;
        if (first && cardFirstGameRole.HasValue) drawnRole = cardFirstGameRole.Value;
        cardFirstGameRole = null;

        if (HasFieldRoleUpper(PlayerType.Self)) {
            if (drawnRole == PachisuroSymbolKoyakuEnum.WeakCherry)
                return PachisuroSymbolKoyakuEnum.StrongCherry;
            if (drawnRole == PachisuroSymbolKoyakuEnum.Suika)
                return PachisuroSymbolKoyakuEnum.Seven;
        }
        return drawnRole;
    }

    public void ClearSlotCardEffects() {
        cardFirstGameRole = null;
        cardFirstGameStarted = false;
        // 常在効果は場から取得するため解除用のフラグを持たない。
    }
}

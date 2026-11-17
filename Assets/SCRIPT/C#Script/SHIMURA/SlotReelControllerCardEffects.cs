using UnityEngine;

/// <summary>
/// スロットに関係するカード効果。手動アタッチ不要。
/// Upper/GOverは場にいる間だけ有効。疲労状態には依存しない。
/// 降臨・テーブル上昇は外部のカード使用処理から明示的に呼び出す。
/// 呼び出し形式：SlotReelController.Instance.CardBellAdvent();
/// Instanceの存在を確認し、自分のスペル使用が確定した箇所で1回だけ呼ぶ。
/// 相手のスペル再生では自分用の予約APIを呼ばない。
/// このクラス自体はスペル消費・コスト支払い・召喚の処理を行わない。
/// partialなので本体と同じクラスとして動作し、本体のInstanceを共有する。
/// </summary>
public partial class SlotReelController {
    // 降臨スペルで予約した1G目の役。nullなら通常の抽選結果を使用する。
    // 複数の降臨を使った場合は、最後に成功した予約で上書きされる。
    private PachisuroSymbolKoyakuEnum? cardFirstGameRole;
    // 1G目の抽選結果にカード効果を適用した時点でtrueになる。
    // 以後の降臨・テーブル上昇の受付を止めるためのフラグ。
    private bool cardFirstGameStarted;

    /// <summary>
    /// 自分の場のGOverによる追加G数。参照するたびに現在の場を調べる。
    /// 基本の5Gは含まない。相手の追加G数は本体から専用の内部関数で取得する。
    /// </summary>
    public int CardExtraGames => GetFieldCardExtraGames(PlayerType.Self);
    /// <summary>自分の場に有効なUpperがあるか。疲労していても有効。</summary>
    public bool CardRoleUpperEnabled => HasFieldRoleUpper(PlayerType.Self);

    /// <summary>
    /// 指定プレイヤーの場にあるGOverの値を合計する。
    /// 毎回取得するため、場に出たカード・離れたカードを次の確認から反映する。
    /// </summary>
    /// <param name="player">追加G数を確認する場の所有者。</param>
    /// <returns>追加G数。対象カードがない場合や初期化前は0。</returns>
    private int GetFieldCardExtraGames(PlayerType player) {
        // カード管理がまだ生成されていない場合にも対応する。
        CardObjectManager manager = CardObjectManager.instance;
        if (manager == null || manager.fieldObjectList == null) return 0;
        // 指定プレイヤーの場が登録済みか確認し、存在するときだけ走査する。
        if (!manager.fieldObjectList.TryGetValue(player, out var field) || field == null) return 0;
        // 加算途中のintオーバーフローを防ぐため、合計にはlongを使う。
        long total = 0;
        // 各レーンに置かれたカードを順に調べる。
        foreach (var entry in field) {
            // 空きレーン・所有者の不一致・効果リスト未設定は対象外。
            // isFatigueは調べないので、疲労中のカードも常在効果を発揮する。
            CardObject card = entry.Value;
            if (card == null || card.playerType != player || card.abilityList == null) continue;
            // 1枚に複数の効果が登録されている場合も、それぞれを確認する。
            foreach (var ability in card.abilityList) {
                // 通常効果は加算せず、常在効果として登録されたGOverのみ対象にする。
                if (ability.abilityType != AbilityType.Passive) continue;
                if (ability.passiveAbility != PassiveAbility.GOver) continue;
                // abilityValueが正数ならその値、0以下なら1Gとして加算する。
                // 複数カード・複数GOverの値はすべて合算する。
                total += ability.abilityValue > 0 ? ability.abilityValue : 1;
                // 本体で基本5Gを足してもintの上限を超えないように制限する。
                if (total >= int.MaxValue - 5) return int.MaxValue - 5;
            }
        }
        // 上限を確認済みの合計を、最大G数計算に使うintで返す。
        return (int)total;
    }

    /// <summary>
    /// 指定プレイヤーの場にPassive/Upperが1つでもあるか確認する。
    /// 複数あっても昇格を繰り返すことはなく、有効・無効だけを返す。
    /// </summary>
    private bool HasFieldRoleUpper(PlayerType player) {
        // カード管理がまだ生成されていない場合にも対応する。
        CardObjectManager manager = CardObjectManager.instance;
        if (manager == null || manager.fieldObjectList == null) return false;
        // 指定プレイヤーの場が登録済みか確認し、存在するときだけ走査する。
        if (!manager.fieldObjectList.TryGetValue(player, out var field) || field == null) return false;
        // 各レーンに置かれたカードを順に調べる。
        foreach (var entry in field) {
            // 空きレーン・所有者の不一致・効果リスト未設定は対象外。
            // isFatigueは調べないので、疲労中のカードも常在効果を発揮する。
            CardObject card = entry.Value;
            if (card == null || card.playerType != player || card.abilityList == null) continue;
            // 1枚に複数の効果が登録されている場合も、それぞれを確認する。
            foreach (var ability in card.abilityList) {
                // Upperを見つけた時点で有効と確定できるため、走査を終了する。
                if (ability.abilityType == AbilityType.Passive &&
                    ability.passiveAbility == PassiveAbility.Upper) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 降臨・テーブル上昇を受け付けられるスロットの状態か確認する。
    /// カードの所持・コスト・現在のターンやフェイズはこの関数では判定しない。
    /// 呼び出し側で自分のスペル使用が正当か確認すること。
    /// </summary>
    public bool CanUseSlotEffectCard() {
        // 相手の再生中ではない、1G目の適用前、全リール停止中の3条件が必要。
        return !networkRemoteControl && !cardFirstGameStarted &&
            !IsAnyReelRotating();
    }

    /// <summary>受付条件を共通で確認し、失敗時はカード名付きの警告を出す。</summary>
    private bool CheckSlotEffectCard(string cardName) {
        if (CanUseSlotEffectCard()) return true;
        Debug.LogWarning("[" + cardName + "] 自分の1G目のレバーオン前に使用してください。", this);
        return false;
    }

    /// <summary>
    /// 1G目の役を予約する。ここでは回転・演出・通信送信を開始しない。
    /// 実際の役変更はレバーオン時のApplySlotCardEffectsで行う。
    /// </summary>
    /// <returns>予約できたらtrue。受付不可なら既存の予約を変えずfalse。</returns>
    private bool ReserveFirstGameRole(PachisuroSymbolKoyakuEnum role, string cardName) {
        if (!CheckSlotEffectCard(cardName)) return false;
        // 既存の予約があれば置き換える。降臨の優先順位は最後に使ったもの。
        cardFirstGameRole = role;
        return true;
    }

    /// <summary>
    /// ブランク降臨：1G目をハズレに予約する。
    /// 使用例：SlotReelController.Instance.CardBlankAdvent();
    /// 自分のスペル使用確定時に1回呼ぶ。trueは予約成功を表す。
    /// </summary>
    public bool CardBlankAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Miss, "ブランク降臨");
    /// <summary>リプレイ降臨：1G目をリプレイに予約。成功時true。</summary>
    public bool CardReplayAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Replay, "リプレイ降臨");
    /// <summary>ベル降臨：1G目をベルに予約。成功時true。</summary>
    public bool CardBellAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Bell, "ベル降臨");
    /// <summary>スイカ降臨：1G目をスイカに予約。Upperが有効なら適用時にSevenになる。</summary>
    public bool CardSuikaAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.Suika, "スイカ降臨");
    /// <summary>チェリー降臨：1G目を弱チェリーに予約。Upperが有効なら適用時に強チェリーになる。</summary>
    public bool CardCherryAdvent() => ReserveFirstGameRole(PachisuroSymbolKoyakuEnum.WeakCherry, "チェリー降臨");

    /// <summary>
    /// うおおおお：現在のテーブル番号をその場で1増やす。
    /// SlotReelController.Instance.CardUoooo()で呼ぶ。成功時true。
    /// 呼ぶたびに増えるため、Updateなどから繰り返し呼ばないこと。
    /// </summary>
    public bool CardUoooo() {
        // 受付条件と整数の上限を確認する。失敗した場合は変更しない。
        if (!CheckSlotEffectCard("うおおおお") || currentTable == int.MaxValue) return false;
        // ここで確認する上限はintの上限のみ。
        // 抽選テーブルとして有効な範囲の扱いは既存の抽選処理に従う。
        currentTable++;
        return true;
    }

    // 旧APIとの互換用。常在効果は手動で加算・予約せず、場の状態を返す。
    // オーヴァ/ロール・アッパー使用時にこれらを呼ぶ必要はない。
    /// <summary>互換用の確認API。追加Gがあるか返すだけで、G数は加算しない。</summary>
    public bool CardOver() => CardExtraGames > 0;
    /// <summary>互換用の確認API。Upperが有効か返すだけで、効果を予約しない。</summary>
    public bool CardRoleUpper() => CardRoleUpperEnabled;

    /// <summary>
    /// 本体のBeginLocalPlaybackから、自分のパチスロ開始時に呼ばれる。
    /// スペル使用時には呼ばない。開始前に予約された降臨は消さない。
    /// </summary>
    private void BeginSlotCardEffects() {
        // 開始前に使用した降臨の予約を保持する。
        cardFirstGameStarted = false;
    }

    /// <summary>
    /// 通常・RUSH共通で、抽選した役に「降臨→Upper」の順に効果を適用する。
    /// 本体で演出・停止位置・役記録・送信内容を決める前に呼び出す。
    /// </summary>
    /// <param name="drawnRole">抽選結果、または通信で受信した確定役。</param>
    /// <returns>このゲームで実際に使用する役。</returns>
    private PachisuroSymbolKoyakuEnum ApplySlotCardEffects(PachisuroSymbolKoyakuEnum drawnRole) {
        // 受信した役は送信側で昇格済み。再変換しない。
        if (networkRemoteControl) return drawnRole;

        // slotIndexが増える前の0で、まだ適用していないときだけ1G目とみなす。
        bool first = !cardFirstGameStarted && slotIndex == 0;
        // ここから先は降臨・テーブル上昇の追加受付を止める。
        cardFirstGameStarted = true;
        // 1G目かつ予約ありなら、抽選結果を予約された役で置き換える。
        if (first && cardFirstGameRole.HasValue) drawnRole = cardFirstGameRole.Value;
        // 予約を消し、後続のゲームや次のターンへ持ち越さない。
        cardFirstGameRole = null;

        // 現在の場を確認して昇格する。降臨で置き換えた役も昇格対象。
        if (HasFieldRoleUpper(PlayerType.Self)) {
            // 弱チェリーは強チェリーへ。既に強チェリーなら変更しない。
            if (drawnRole == PachisuroSymbolKoyakuEnum.WeakCherry)
                return PachisuroSymbolKoyakuEnum.StrongCherry;
            // スイカはRG揃いとして扱うSevenへ昇格する。
            if (drawnRole == PachisuroSymbolKoyakuEnum.Suika)
                return PachisuroSymbolKoyakuEnum.Seven;
        }
        // 昇格対象でなければ、ここまでで決まった役をそのまま採用する。
        return drawnRole;
    }

    /// <summary>
    /// 自分のパチスロ終了時や無効化時に、降臨予約と開始フラグを解除する。
    /// 上昇させたcurrentTableは戻さず、場にあるカードも変更しない。
    /// </summary>
    public void ClearSlotCardEffects() {
        // 予約を消し、後続のゲームや次のターンへ持ち越さない。
        cardFirstGameRole = null;
        cardFirstGameStarted = false;
        // 常在効果は場から取得するため解除用のフラグを持たない。
    }
}

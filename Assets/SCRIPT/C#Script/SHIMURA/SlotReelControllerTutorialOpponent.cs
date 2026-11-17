using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 通信なしのチュートリアルで、相手のスロットを自動再生する。
/// SlotReelController本体と同じクラスとして動作する。
/// </summary>
public partial class SlotReelController {
    // 同じ自動再生が重複して開始されるのを防ぐ。
    private bool tutorialOpponentPlaying;

    /// <summary>
    /// 指定された役を順番に自動再生する。
    /// 各Gは左→中→右の順で停止し、第3停止後に次のGへ進む。
    /// 今回の相手は通常状態として再生する。
    ///
    /// 呼び出す前に、相手のパチスロフェイズのSetupを済ませること。
    /// この関数と同時にOpponentExecuteやEnemySlotUpdateを呼ばないこと。
    /// </summary>
    public async UniTask PlayTutorialOpponentAsync( 
        params PachisuroSymbolKoyakuEnum[] roles) {
        // 再生条件を確認する。失敗時にフェイズを先へ進めない。
        if (tutorialOpponentPlaying)
            throw new InvalidOperationException("相手の自動再生は実行中です。");

        if (!NetworkReady || PachisuroPhase.Instance == null)
            throw new InvalidOperationException(
                "リールの初期化とパチスロフェイズのSetupが必要です。"
            );

        if (IsAnyReelRotating() || NetworkReelLockRunning)
            throw new InvalidOperationException("リール停止後に開始してください。");

        if (roles == null || roles.Length == 0)
            throw new ArgumentException("再生する役を指定してください。");

        // 呼び出し元が配列を書き換えても、再生順が変わらないようにコピーする。
        var playbackRoles = (PachisuroSymbolKoyakuEnum[])roles.Clone();

        foreach (var role in playbackRoles) {
            if (role == PachisuroSymbolKoyakuEnum.NONE ||
                !Enum.IsDefined(typeof(PachisuroSymbolKoyakuEnum), role)) {
                throw new ArgumentException("無効な役が指定されています。");
            }
        }

        // この演出では3本のリールを左から順に止める。
        if (reels == null || reels.Length != 3 ||
            reels[0] == null || reels[1] == null || reels[2] == null) {
            throw new InvalidOperationException("3本のリールを設定してください。");
        }

        tutorialOpponentPlaying = true;

        // 終了後に、呼び出し前のチュートリアル設定へ戻す。
        bool previousTutorialMode = IsTutorialMode;

        // オブジェクトが破棄された場合は、非同期待機も中断する。
        var token = this.GetCancellationTokenOnDestroy();

        // 自分のRUSH・テーブルなどを退避し、相手再生に切り替える。
        SetTutorialMode(true);
        BeginRemotePlayback();

        try {
            for (int game = 0; game < playbackRoles.Length; game++) {
                // 受信再生用の入口を利用し、指定役で回転を開始する。
                // 通信送信や再抽選は行わない。
                // 相手再生なので、自分のカードによる役昇格も適用されない。
                bool started = StartRemoteSpin(
                    game + 1,
                    playbackRoles[game],
                    false, // 相手は通常状態
                    false, // RUSH当選待ちなし
                    1      // 演出に使用する相手のテーブル
                );

                if (!started)
                    throw new InvalidOperationException("相手の回転開始に失敗しました。");

                // 回転開始から0.8秒見せる。
                float elapsed = 0f;
                int nextReel = 0;

                while (true) {
                    token.ThrowIfCancellationRequested();

                    if (!isActiveAndEnabled)
                        throw new OperationCanceledException("スロットが無効化されました。");

                    // リールロック中は停止までの時間を進めない。
                    if (!NetworkReelLockRunning) {
                        elapsed += Time.deltaTime;

                        // 左は0.8秒、中は1.25秒、右は1.7秒で停止予約する。
                        float stopTime = 0.8f + nextReel * 0.45f;

                        if (nextReel < 3 && elapsed >= stopTime) {
                            // 現在位置から、指定役に対応する停止角度を計算する。
                            float angle = GetNextStopAngle(
                                nextReel,
                                state.ReelAngles[nextReel],
                                currentSlotSymbolRole
                            );

                            // 相手再生用の停止処理へ渡す。通信は行わない。
                            if (!ReserveRemoteStop(
                                nextReel, angle, reelSpeeds[nextReel])) {
                                throw new InvalidOperationException(
                                    "相手の停止予約に失敗しました。"
                                );
                            }

                            nextReel++;
                        }
                    }

                    // 入力受付や通信受信をせず、リールの動きだけ更新する。
                    UpdateReelRotation();
                    SyncInspectorAngles();

                    // 第3停止を予約しただけでは完了にしない。
                    // 最後のリールが実際の停止角度に到着するまで待つ。
                    if (nextReel == 3 &&
                        !IsAnyReelRotating() &&
                        !NetworkReelLockRunning) {
                        break;
                    }

                    await UniTask.NextFrame(cancellationToken: token);
                }

                // オーラなどを見せてから次のGへ進む。
                // 最終Gは3秒待ってからフェイズ終了の片付けに進む。
                int waitMilliseconds =
                    game == playbackRoles.Length - 1 ? 3000 : 1200;

                await UniTask.Delay(
                    waitMilliseconds,
                    cancellationToken: token
                );
            }
        }
        finally {
            tutorialOpponentPlaying = false;

            if (this != null) {
                // 相手の攻撃判定用RUSH情報を保存し、自分の状態を復元する。
                // 相手の再生なので、自分のテーブルを進めるDebugAAAは呼ばない。
                EndNetworkPlayback();

                if (efectManager != null)
                    efectManager.HideTrumps();

                HideStopEffectImage();
                SetTutorialMode(previousTutorialMode);
            }
        }
    }
}
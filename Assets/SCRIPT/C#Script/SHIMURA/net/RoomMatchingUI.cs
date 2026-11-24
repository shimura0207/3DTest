using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// MatchingPartから開く、Prefab不要のROOM画面。
/// true=マッチ成立、false=戻る。GameObjectへのアタッチは不要。
/// </summary>
public static class RoomMatchingUI {
    private static bool opening;

    public static async UniTask<bool> OpenAsync(TMP_FontAsset font, CancellationToken token) {
        if (opening) return false;
        var match = MatchingNetMatch.instance;
        if (match == null || !match.isActiveAndEnabled) {
            Debug.LogError("[ROOM] 有効なMatchingNetMatchがありません。既存の通信管理を有効にしてください。");
            return false;
        }
        // 別の進行中の通信を画面表示だけで切断しない。
        if (NetworkClient.active || NetworkServer.active || match.IsRandomMatching) {
            Debug.LogWarning("[ROOM] 既に通信中です。退出後にROOM画面を開いてください。");
            return false;
        }
        opening = true;
        GameObject root = null;
        GameObject createdEventSystem = null;
        GameObject previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        TMP_InputField previousInput = null;
        bool bound = false;
        bool succeeded = false;
        try {
            token.ThrowIfCancellationRequested();
            // 前回の検索失敗などで残った画面状態をメニューへ戻す。
            match.OnReturnButtonClicked();
            if (font == null) font = TMP_Settings.defaultFontAsset;
            if (font == null) {
                Debug.LogError("[ROOM] TMPフォントがありません。MatchingPartのRoom UI Fontを設定してください。");
                return false;
            }

            // 通常は既存EventSystemを利用。存在しないときだけ作成する。
            if (EventSystem.current == null) {
                createdEventSystem = new GameObject("ROOM_EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                createdEventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                createdEventSystem.AddComponent<StandaloneInputModule>();
#endif
            }

            // パートのパネルが切り替わっても消えない独立Canvasを作成する。
            root = new GameObject("ROOM_RuntimeCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            RectTransform veil = Rect("Background", root.transform, Vector2.zero, Vector2.zero);
            Stretch(veil, 0f);
            veil.gameObject.AddComponent<Image>().color = new Color(0.015f, 0.025f, 0.06f, 0.96f);
            RectTransform panel = Rect("Panel", veil, Vector2.zero, new Vector2(860, 680));
            panel.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.09f, 0.15f, 1f);
            Label("Title", panel, "ROOM MATCH", font, 52, new Vector2(0, 260), new Vector2(780, 70));
            Label("Guide", panel, "同じLAN内の相手と、4桁の番号で対戦", font, 28,
                new Vector2(0, 190), new Vector2(780, 60));

            // InputFieldは非アクティブの間にText/Viewportを組み立ててから有効化する。
            RectTransform inputRect = Rect("RoomCodeInput", panel, new Vector2(0, 90), new Vector2(460, 96));
            inputRect.gameObject.SetActive(false);
            Image inputImage = inputRect.gameObject.AddComponent<Image>();
            inputImage.color = new Color(0.13f, 0.18f, 0.27f);
            TMP_InputField input = inputRect.gameObject.AddComponent<TMP_InputField>();
            RectTransform viewport = Rect("Text Area", inputRect, Vector2.zero, Vector2.zero);
            Stretch(viewport, 16f);
            viewport.gameObject.AddComponent<RectMask2D>();
            TMP_Text valueText = Label("Text", viewport, "", font, 48, Vector2.zero, Vector2.zero);
            Stretch(valueText.rectTransform, 0f);
            TMP_Text placeholder = Label("Placeholder", viewport, "0000", font, 48, Vector2.zero, Vector2.zero);
            Stretch(placeholder.rectTransform, 0f);
            placeholder.color = new Color(0.6f, 0.65f, 0.75f, 0.6f);
            input.targetGraphic = inputImage;
            input.textViewport = viewport;
            input.textComponent = valueText;
            input.placeholder = placeholder;
            input.contentType = TMP_InputField.ContentType.Standard;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterValidation = TMP_InputField.CharacterValidation.Digit;
            input.characterLimit = 4;
            input.richText = false;
            input.customCaretColor = true;
            input.caretColor = Color.white;
            inputRect.gameObject.SetActive(true);
            previousInput = match.BindRoomInput(input);
            bound = true;

            Button create = MakeButton("Create", panel, "部屋を作成", font, new Vector2(-185, -40));
            Button join = MakeButton("Join", panel, "部屋に参加", font, new Vector2(185, -40));
            TMP_Text status = Label("Status", panel, "番号を入力し、作成か参加を選んでください", font, 26,
                new Vector2(0, -160), new Vector2(790, 120));
            Button back = MakeButton("Back", panel, "戻る", font, new Vector2(0, -275));
            bool cancelled = false;
            create.onClick.AddListener(match.OnCreateRoomButton);
            join.onClick.AddListener(match.OnJoinRoomButton);
            back.onClick.AddListener(() => cancelled = true);
            input.ActivateInputField();

            while (!cancelled) {
                token.ThrowIfCancellationRequested();
                if (match == null || !match.isActiveAndEnabled) return false;
                var network = CustomNetworkManager.instance;
                // サーバー内部の開始判定ではなく、この端末が開始通知を受けたことを待つ。
                if (network != null && network.IsMatchStarted) {
                    succeeded = true;
                    return true;
                }
                bool editable = match.CanStartRoomMatch;
                create.interactable = editable;
                join.interactable = editable;
                input.interactable = editable;
                status.text = string.IsNullOrEmpty(match.RoomMatchStatus)
                    ? "番号を入力し、作成か参加を選んでください" : match.RoomMatchStatus;
                await UniTask.Yield();
            }
            return false;
        }
        finally {
            // 成立時は通信を保持。戻る・中断・例外時だけ募集と接続を終了する。
            if (match != null) {
                if (bound) match.BindRoomInput(previousInput);
                if (!succeeded) match.OnLeaveRoomButton();
            }
            if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            if (createdEventSystem != null) UnityEngine.Object.Destroy(createdEventSystem);
            else if (EventSystem.current != null && previousSelection != null && previousSelection.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousSelection);
            opening = false;
        }
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size) {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }
    private static void Stretch(RectTransform rect, float inset) {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }
    private static TMP_Text Label(string name, Transform parent, string text, TMP_FontAsset font,
        float size, Vector2 position, Vector2 bounds) {
        var rect = Rect(name, parent, position, bounds);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.text = text;
        return label;
    }
    private static Button MakeButton(string name, Transform parent, string text, TMP_FontAsset font, Vector2 position) {
        RectTransform rect = Rect(name, parent, position, new Vector2(330, 76));
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.13f, 0.33f, 0.57f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        TMP_Text label = Label("Label", rect, text, font, 30, Vector2.zero, Vector2.zero);
        Stretch(label.rectTransform, 4f);
        return button;
    }
}

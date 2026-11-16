using System.Collections.Generic;
using UnityEngine;

/// <summary>役別3Dランプの点滅。共有マテリアルを変更せず、終了時に元の表示へ戻します。</summary>
[DisallowMultipleComponent]
public class RoleLampFlashController : MonoBehaviour {
    [System.Serializable]
    public class RoleLamp {
        public PachisuroSymbolKoyakuEnum role;
        [Tooltip("ランプ部分の3Dオブジェクト。子のRendererも対象です。筐体全体は登録しないでください。")]
        public GameObject lampObject;
        public Color flashColor = Color.white;
        public RoleLamp(PachisuroSymbolKoyakuEnum role, Color color) { this.role = role; flashColor = color; }
    }

    [Header("役ごとの3Dランプ")]
    [SerializeField]
    private RoleLamp[] lamps = {
        new RoleLamp(PachisuroSymbolKoyakuEnum.Miss, Color.white),
        new RoleLamp(PachisuroSymbolKoyakuEnum.Replay, new Color(0.1f, 0.4f, 1f)),
        new RoleLamp(PachisuroSymbolKoyakuEnum.Bell, Color.yellow),
        new RoleLamp(PachisuroSymbolKoyakuEnum.Suika, Color.green),
        new RoleLamp(PachisuroSymbolKoyakuEnum.WeakCherry, Color.red),
        new RoleLamp(PachisuroSymbolKoyakuEnum.StrongCherry, Color.red),
        new RoleLamp(PachisuroSymbolKoyakuEnum.Seven, Color.white)
    };

    [Header("点滅")]
    [Tooltip("1秒間の明→暗の回数")]
    [SerializeField, Range(1f, 30f)] private float flashesPerSecond = 12f;
    [SerializeField, Range(0.1f, 0.9f)] private float onRatio = 0.5f;
    [SerializeField, Min(0f)] private float emissionIntensity = 6f;
    [SerializeField, Range(0f, 1f)] private float darkBrightness = 0.02f;
    [SerializeField] private bool useUnscaledTime;

    private class Target { public Renderer renderer; public Color color; }
    private class Flash {
        public int id;
        public float age;
        public readonly List<Target> targets = new List<Target>();
    }
    private class Slot {
        public MaterialPropertyBlock original;
        public MaterialPropertyBlock working;
        public int baseId;
        public bool hasBase, hasEmission;
        public float alpha;
    }
    private class RendererState {
        public Renderer renderer;
        public Slot[] slots;
    }
    private struct Appearance { public Color color; public bool on; }

    private readonly List<Flash> flashes = new List<Flash>();
    private readonly Dictionary<Renderer, RendererState> states = new Dictionary<Renderer, RendererState>();
    private readonly Dictionary<Renderer, Appearance> desired = new Dictionary<Renderer, Appearance>();
    private readonly List<Renderer> remove = new List<Renderer>();
    private int nextId;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    /// <summary>Sevenは全登録ランプ。返り値をStopFlashに渡すと、その演出だけ終了。</summary>
    public int BeginFlash(PachisuroSymbolKoyakuEnum role) {
        if (!isActiveAndEnabled || lamps == null) return 0;
        Flash flash = new Flash { id = ++nextId };
        foreach (RoleLamp lamp in lamps) {
            if (lamp == null || lamp.lampObject == null) continue;
            bool selected = role == PachisuroSymbolKoyakuEnum.Seven
                || (role == PachisuroSymbolKoyakuEnum.Chance
                    ? lamp.role == PachisuroSymbolKoyakuEnum.Suika || lamp.role == PachisuroSymbolKoyakuEnum.WeakCherry
                    : lamp.role == role);
            if (!selected) continue;
            foreach (Renderer renderer in lamp.lampObject.GetComponentsInChildren<Renderer>(true))
                flash.targets.Add(new Target { renderer = renderer, color = lamp.flashColor });
        }
        if (flash.targets.Count == 0) return 0;
        flashes.Add(flash);
        RenderFlashes(); // 最初は必ず点灯側から始める。
        return flash.id;
    }

    public void StopFlash(int id) {
        if (id == 0) return;
        for (int i = flashes.Count - 1; i >= 0; i--)
            if (flashes[i].id == id) flashes.RemoveAt(i);
        RenderFlashes();
    }

    private void Update() {
        if (flashes.Count == 0) return;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        foreach (Flash flash in flashes) flash.age += dt;
        RenderFlashes();
    }

    private void RenderFlashes() {
        desired.Clear();
        foreach (Flash flash in flashes) {
            bool on = Mathf.Repeat(flash.age * Mathf.Max(1f, flashesPerSecond), 1f) < onRatio;
            foreach (Target target in flash.targets)
                if (target.renderer != null)
                    desired[target.renderer] = new Appearance { color = target.color, on = on };
        }
        remove.Clear();
        foreach (var pair in states) {
            if (pair.Key == null || !desired.ContainsKey(pair.Key)) {
                Restore(pair.Value);
                remove.Add(pair.Key);
            }
        }
        foreach (Renderer renderer in remove) states.Remove(renderer);
        foreach (var pair in desired) {
            RendererState state;
            if (!states.TryGetValue(pair.Key, out state)) {
                state = Capture(pair.Key);
                states.Add(pair.Key, state);
            }
            for (int i = 0; i < state.slots.Length; i++) {
                Slot slot = state.slots[i];
                if (slot == null) continue;
                Color color = pair.Value.color * (pair.Value.on ? 1f : darkBrightness);
                color.a = slot.alpha;
                if (slot.hasBase) slot.working.SetColor(slot.baseId, color);
                if (slot.hasEmission) {
                    Color emission = pair.Value.on ? pair.Value.color * emissionIntensity : Color.black;
                    emission.a = 1f;
                    slot.working.SetColor(EmissionId, emission);
                }
                state.renderer.SetPropertyBlock(slot.working, i);
            }
        }
    }

    private RendererState Capture(Renderer renderer) {
        Material[] materials = renderer.sharedMaterials;
        RendererState state = new RendererState { renderer = renderer, slots = new Slot[materials.Length] };
        for (int i = 0; i < materials.Length; i++) {
            Material material = materials[i];
            if (material == null) continue;
            Slot slot = new Slot {
                original = new MaterialPropertyBlock(),
                working = new MaterialPropertyBlock(),
                baseId = material.HasProperty(BaseColorId) ? BaseColorId : ColorId,
                hasEmission = material.HasProperty(EmissionId)
            };
            slot.hasBase = material.HasProperty(slot.baseId);
            renderer.GetPropertyBlock(slot.original, i);
            renderer.GetPropertyBlock(slot.working, i);
            // マテリアル別の上書きがなければRenderer全体の設定を引き継ぐ。
            if (slot.working.isEmpty) renderer.GetPropertyBlock(slot.working);
            slot.alpha = slot.hasBase ? material.GetColor(slot.baseId).a : 1f;
            if (!slot.hasBase && !slot.hasEmission)
                Debug.LogWarning(renderer.name + "：ランプShaderに_Color/_BaseColor/_EmissionColorがありません。StandardまたはURP/Litを使用してください。", this);
            state.slots[i] = slot;
        }
        return state;
    }

    private static void Restore(RendererState state) {
        if (state.renderer == null) return;
        int count = Mathf.Min(state.slots.Length, state.renderer.sharedMaterials.Length);
        for (int i = 0; i < count; i++) {
            Slot slot = state.slots[i];
            if (slot != null)
                state.renderer.SetPropertyBlock(slot.original.isEmpty ? null : slot.original, i);
        }
    }

    public void StopAllFlashes() {
        flashes.Clear();
        foreach (RendererState state in states.Values) Restore(state);
        states.Clear();
        desired.Clear();
    }

    private void OnDisable() { StopAllFlashes(); }
    private void OnDestroy() { StopAllFlashes(); }
}

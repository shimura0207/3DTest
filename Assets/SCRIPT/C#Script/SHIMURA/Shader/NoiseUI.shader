Shader "UI/NoiseGlitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _NoiseStrength ("Noise Strength", Range(0,1)) = 1
        _NoiseScale ("Noise Scale", Float) = 25
        _NoiseSpeed ("Noise Speed", Float) = 10
        _GlitchSize ("Glitch Size", Range(0,0.2)) = 0.05

        // Unity UI用
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            fixed4 _Color;

            float4 _ClipRect;

            float _NoiseStrength;
            float _NoiseScale;
            float _NoiseSpeed;
            float _GlitchSize;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;

                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;

                return o;
            }

            float RandomNoise(float2 p)
            {
                return frac(
                    sin(dot(p, float2(12.9898, 78.233)))
                    * 43758.5453
                );
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float time = _Time.y * _NoiseSpeed;

                // ============================================
                // 荒い横グリッチ
                // ============================================

                float yLine = floor(uv.y * _NoiseScale);

                float randomLine =
                    RandomNoise(
                        float2(
                            yLine,
                            floor(time)
                        )
                    );

                float glitch =
                    (randomLine - 0.5)
                    * _GlitchSize
                    * _NoiseStrength;

                uv.x += glitch;

                // ============================================
                // 元画像
                // ============================================

                fixed4 col =
                    tex2D(_MainTex, uv)
                    * i.color;

                // ============================================
                // 大きめのノイズ
                // ============================================

                float2 noiseUV =
                    floor(uv * _NoiseScale);

                float noise =
                    RandomNoise(
                        noiseUV +
                        floor(time)
                    );

                // 白黒の荒いノイズ
                float roughNoise =
                    step(0.45, noise);

                // 元画像を壊す
                col.rgb =
                    lerp(
                        col.rgb,
                        col.rgb * roughNoise,
                        _NoiseStrength
                    );

                // ============================================
                // UI Clip対応
                // ============================================

                #ifdef UNITY_UI_CLIP_RECT

                col.a *= UnityGet2DClipping(
                    i.worldPosition.xy,
                    _ClipRect
                );

                #endif

                // ============================================
                // Alpha Clip
                // ============================================

                #ifdef UNITY_UI_ALPHACLIP

                clip(col.a - 0.001);

                #endif

                return col;
            }

            ENDCG
        }
    }
}
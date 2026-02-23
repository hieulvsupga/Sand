Shader "Custom/SandEffect2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SandColor ("Sand Color", Color) = (0.9, 0.8, 0.4, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _NoiseScale ("Noise Scale", Float) = 50.0
        _NoiseStrength ("Noise Edge Strength", Range(0, 0.2)) = 0.05
        _GrainStrength ("Grain Strength", Range(0, 0.5)) = 0.1
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveFrequency ("Wave Frequency", Float) = 10.0
        _Turbulence ("Sand Turbulence", Float) = 1.0
        _FlowSpread ("Flow Spread", Range(0, 1)) = 0.3
        _FillVelocity ("Fill Velocity", Range(0, 1)) = 0.0
        _PeakX ("Peak X Position", Range(0, 1)) = 0.5
        _PeakHeight ("Peak Height", Range(0, 0.5)) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float normalizedWorldY : TEXCOORD1;
                float normalizedWorldX : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _SandColor;
            float _FillAmount;
            float _NoiseScale;
            float _NoiseStrength;
            float _GrainStrength;
            float _WaveSpeed;
            float _WaveFrequency;
            float _Turbulence;
            float _FlowSpread;
            float _FillVelocity;
            float _PeakX;
            float _PeakHeight;

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float signedRandom(float2 uv)
            {
                return random(uv) * 2.0 - 1.0;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                float3 bl = mul(unity_ObjectToWorld, float4(-0.5, -0.5, 0, 1)).xyz;
                float3 br = mul(unity_ObjectToWorld, float4( 0.5, -0.5, 0, 1)).xyz;
                float3 tl = mul(unity_ObjectToWorld, float4(-0.5,  0.5, 0, 1)).xyz;
                float3 tr = mul(unity_ObjectToWorld, float4( 0.5,  0.5, 0, 1)).xyz;

                float minY = min(min(bl.y, br.y), min(tl.y, tr.y));
                float maxY = max(max(bl.y, br.y), max(tl.y, tr.y));
                float minX = min(min(bl.x, br.x), min(tl.x, tr.x));
                float maxX = max(max(bl.x, br.x), max(tl.x, tr.x));

                o.normalizedWorldY = (worldPos.y - minY) / (maxY - minY);
                o.normalizedWorldX = (worldPos.x - minX) / (maxX - minX);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float uvY = i.normalizedWorldY;
                float uvX = i.normalizedWorldX;

                float waveDamping = (1.0 - smoothstep(0.9, 1.0, _FillAmount))
                                  * smoothstep(0.0, 0.1, _FillAmount);

                // ── SÓNG TĨNH (khi đứng yên) ──────────────────────────────
                float staticPhase = _Time.y * 0.3;
                float wave1 = sin(uvX * _WaveFrequency + staticPhase);
                float wave2 = sin(uvX * _WaveFrequency * 1.5 + staticPhase * 1.3 + 1.5);
                float staticWave = (wave1 + wave2 * 0.5) * _NoiseStrength * waveDamping;

                // ── TAM GIÁC CÂN THẲNG (khi đang đổ cát) ─────────────────
                float mountainShape;
                if (uvX < _PeakX)
                    mountainShape = uvX / _PeakX;
                else
                    mountainShape = (1.0 - uvX) / (1.0 - _PeakX);

                float mountainWave = mountainShape * _PeakHeight * waveDamping;

                // ── BLEND theo velocity (phản ứng nhanh) ──────────────────
                float blendT = smoothstep(0.0, 0.05, _FillVelocity);
                float surfaceOffset = lerp(staticWave, mountainWave, blendT);

                float threshold = _FillAmount + surfaceOffset;
                if (_FillAmount >= 1.0) threshold = 1.1;
                if (_FillAmount <= 0.001) threshold = -0.1;

                if (uvY > threshold)
                    return fixed4(0, 0, 0, 0);

                // ── GRAIN ──────────────────────────────────────────────────
                float2 baseCell = floor(float2(uvX, uvY) * _NoiseScale);
                float2 flowCell = floor(float2(uvX, uvY) * _NoiseScale * 0.2);

                float rawFlowX = signedRandom(flowCell + float2(0.1, 0.2)) * _FlowSpread;
                float rawFlowY = (random(flowCell + float2(0.3, 0.4)) * 0.4 + 0.6);

                float offsetX = round(rawFlowX * _FillAmount * _Turbulence * 8.0) * _FlowSpread;
                float offsetY = round(rawFlowY * _FillAmount * _Turbulence * 20.0);

                float2 grainCell = baseCell + float2(offsetX, offsetY);
                float grain = random(grainCell);

                fixed4 col = _SandColor;
                col.rgb -= grain * _GrainStrength;

                return col;
            }
            ENDCG
        }
    }
}
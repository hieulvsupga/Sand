Shader "Custom/SimpleSandFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SandColor ("Sand Color", Color) = (0.9, 0.8, 0.4, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _NoiseScale ("Noise Scale", Float) = 50.0
        _NoiseStrength ("Noise Edge Strength", Range(0, 0.2)) = 0.05
        _GrainStrength ("Grain Strength", Range(0, 0.5)) = 0.1
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _SandColor;
            float _FillAmount;
            float _NoiseScale;
            float _NoiseStrength;
            float _GrainStrength;

            // Simple pseudo-random function
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Simple value noise function
            float noise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                        (c - a)* u.y * (1.0 - u.x) +
                        (d - b) * u.x * u.y;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate noise for the top edge
                float edgeNoise = noise(i.uv * _NoiseScale * 0.5); 
                
                // Calculate fills threshold with noise
                float threshold = _FillAmount + (edgeNoise - 0.5) * _NoiseStrength;
                
                // Determine if this pixel is filled with sand
                if (i.uv.y > threshold)
                {
                    // Empty space (transparent)
                    return fixed4(0,0,0,0);
                }

                // Add grain noise inside the sand area
                float grain = random(i.uv * _NoiseScale);
                fixed4 col = _SandColor;
                
                // Darken slightly based on grain for texture
                col.rgb -= grain * _GrainStrength;
                
                return col;
            }
            ENDCG
        }
    }
}

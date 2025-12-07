Shader "Hidden/URP/LensGlare"
{
    Properties
    {
        _Intensity ("Intensity", Float) = 1.2
        _Threshold ("Threshold", Float) = 0.8
        _Streaks ("Streaks", Int) = 4
        _StreakLength ("StreakLength", Float) = 0.6
        _Falloff ("Falloff", Float) = 0.9
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = UnityObjectToClipPos(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Intensity;
            float _Threshold;
            int _Streaks;
            float _StreakLength;
            float _Falloff;

            float3 Luma(float3 c) { return max(c - _Threshold, 0); }

            fixed4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texel = _MainTex_TexelSize.xy;

                float3 src = tex2D(_MainTex, uv).rgb;

                // extract bright
                float3 bright = max(src - _Threshold, 0);

                // accumulate streaks
                float3 streakAccum = 0;
                int sCount = max(1, _Streaks);
                // use fixed max loop to be HLSL friendly
                for (int s = 0; s < 8; ++s)
                {
                    if (s >= sCount) break;
                    // direction angle
                    float angle = (3.14159 * 2.0f * s) / sCount;
                    float2 dir = float2(cos(angle), sin(angle));

                    // sample along direction for streak effect
                    float weight = 1.0;
                    float decay = 1.0;
                    int samples = 6;
                    for (int k = 1; k <= samples; ++k)
                    {
                        float t = k / (float)samples; // 0..1
                        float2 off = dir * t * _StreakLength * 8.0 * texel;
                        float3 sampleCol = tex2D(_MainTex, uv + off).rgb;
                        float3 sampleBright = max(sampleCol - _Threshold, 0);
                        streakAccum += sampleBright * weight * decay;
                        decay *= _Falloff;
                        weight *= 1.0; // keep simple
                    }
                }

                // combine and tone
                float3 glare = (bright + streakAccum) * _Intensity;

                float3 outCol = src + glare;
                return float4(outCol, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
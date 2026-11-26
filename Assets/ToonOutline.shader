Shader "Custom/ToonOutline"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Object Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.3,0.3,0.3,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.001, 0.05)) = 0.015
        _Steps ("Lighting Steps", Range(2,5)) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        // OUTLINE PASS
        Pass
        {
             Name "Outline"

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 position = IN.positionOS.xyz +
                    IN.normalOS * _OutlineWidth;

                OUT.positionHCS = TransformObjectToHClip(position);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }

        // TOON PASS
        Pass
        {
            Name "Toon"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float _Steps;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                OUT.positionHCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;

                OUT.normalWS =
                    TransformObjectToWorldNormal(IN.normalOS);

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                Light mainLight = GetMainLight();

                float NdotL = saturate(
                    dot(normal, mainLight.direction)
                );

                float toon = floor(NdotL * _Steps) /
                    (_Steps - 1);

                float3 lighting = lerp(
                    _ShadowColor.rgb,
                    _Color.rgb,
                    saturate(toon)
                );

                half4 tex = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    IN.uv
                );

                float3 finalColor = tex.rgb * lighting;

                return half4(
                    finalColor,
                    tex.a * _Color.a
                );
            }

            ENDHLSL
        }
    }
}

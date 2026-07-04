Shader "Game/Level/Finish Threshold Tiled Fade"
{
    Properties
    {
        [MainTexture] _BaseMap("Checker Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Color("Color", Color) = (1, 1, 1, 1)
        _BottomAlpha("Bottom Alpha", Range(0, 1)) = 0.85
        _TopAlpha("Top Alpha", Range(0, 1)) = 0
        _FadeExponent("Fade Exponent", Range(0.1, 8)) = 1.2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _Color;
                float _BottomAlpha;
                float _TopAlpha;
                float _FadeExponent;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 tiledUv : TEXCOORD0;
                float2 fadeUv : TEXCOORD1;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.tiledUv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fadeUv = input.uv;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                const half minimumExponent = 0.0001h;
                const half presentationAlpha = min((half)_BaseColor.a, (half)_Color.a);
                const half fadePosition = pow(saturate((half)input.fadeUv.y), max((half)_FadeExponent, minimumExponent));
                const half verticalAlpha = lerp((half)_TopAlpha, (half)_BottomAlpha, fadePosition);
                const half4 checker = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.tiledUv);
                const half3 tint = checker.rgb * (half3)_BaseColor.rgb * (half3)_Color.rgb;

                return half4(tint, checker.a * verticalAlpha * presentationAlpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}

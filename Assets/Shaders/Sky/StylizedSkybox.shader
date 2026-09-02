Shader "GenesisWorld/StylizedSkybox"
{
    Properties
    {
        [Header(Gradient)]
        _ZenithColor("Zenith Color", Color) = (0.18, 0.42, 0.72, 1)
        _HorizonColor("Horizon Color", Color) = (0.72, 0.84, 0.82, 1)
        _LowerColor("Lower Color", Color) = (0.32, 0.38, 0.28, 1)
        _HorizonExponent("Horizon Exponent", Range(0.1, 4)) = 0.65
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "Skybox"

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ZenithColor;
                half4 _HorizonColor;
                half4 _LowerColor;
                float _HorizonExponent;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDirectionWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDirectionWS = TransformObjectToWorldDir(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float verticalDirection = normalize(input.viewDirectionWS).y;
                float exponent = max(_HorizonExponent, 0.01);

                float upperAmount = pow(smoothstep(0.0, 1.0, saturate(verticalDirection)), exponent);
                float lowerAmount = pow(smoothstep(0.0, 1.0, saturate(-verticalDirection)), exponent);

                half3 upperGradient = lerp(_HorizonColor.rgb, _ZenithColor.rgb, upperAmount);
                half3 lowerGradient = lerp(_HorizonColor.rgb, _LowerColor.rgb, lowerAmount);
                half upperMask = step(0.0, verticalDirection);
                half3 skyColor = lerp(lowerGradient, upperGradient, upperMask);

                return half4(skyColor, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

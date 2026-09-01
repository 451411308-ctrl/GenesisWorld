Shader "GenesisWorld/StylizedTerrain"
{
    Properties
    {
        [Header(Height Colors)]
        _LowColor("Low Color", Color) = (0.10, 0.24, 0.07, 1)
        _HighColor("High Color", Color) = (0.48, 0.62, 0.24, 1)
        _HeightMin("Height Min", Float) = -2.5
        _HeightMax("Height Max", Float) = 2.5

        [Header(Slope)]
        _SlopeColor("Slope Color", Color) = (0.36, 0.32, 0.27, 1)
        _SlopeStart("Slope Start", Range(0, 1)) = 0.04
        _SlopeEnd("Slope End", Range(0, 1)) = 0.12

        [Header(Lighting)]
        _AmbientStrength("Ambient Strength", Range(0, 1)) = 0.32

        [HideInInspector] _BaseMap("Base Map", 2D) = "white" {}
        [HideInInspector] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        [HideInInspector] _Surface("Surface Type", Float) = 0
        [HideInInspector] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _LowColor;
                half4 _HighColor;
                half4 _SlopeColor;
                float _HeightMin;
                float _HeightMax;
                float _SlopeStart;
                float _SlopeEnd;
                float _AmbientStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normalWS = NormalizeNormalPerPixel(input.normalWS);

                float safeHeightRange = max(_HeightMax - _HeightMin, 0.0001);
                half heightFactor = saturate((input.positionWS.y - _HeightMin) / safeHeightRange);
                half3 heightColor = lerp(_LowColor.rgb, _HighColor.rgb, heightFactor);

                half upAlignment = saturate(dot(normalWS, half3(0.0, 1.0, 0.0)));
                half slope = 1.0h - upAlignment;
                float safeSlopeEnd = max(_SlopeEnd, _SlopeStart + 0.0001);
                half slopeFactor = smoothstep(_SlopeStart, safeSlopeEnd, slope);
                half3 baseColor = lerp(heightColor, _SlopeColor.rgb, slopeFactor);

                Light mainLight = GetMainLight(input.shadowCoord);
                half3 lightDirectionWS = normalize(mainLight.direction);
                half ndotl = saturate(dot(normalWS, lightDirectionWS));
                half directLight = ndotl * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                half3 lighting = _AmbientStrength.xxx + mainLight.color * directLight;
                half3 color = baseColor * lighting;

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack Off
}

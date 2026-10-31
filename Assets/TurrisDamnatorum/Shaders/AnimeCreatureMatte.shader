// Matowy materiał PoC: pasma światła rozproszonego, bez BRDF metalu, połysku i odbić otoczenia.
Shader "Turris/AnimeCreatureMatte"
{
    Properties
    {
        [MainTexture] _BaseMap("Kolor", 2D) = "white" {}
        [MainColor] _BaseColor("Odcień", Color) = (1,1,1,1)
        _SteelTint("Najjaśniejsza stal", Color) = (0.65,0.63,0.59,1)
        _TrimTint("Stare złoto obrzeży", Color) = (0.74,0.64,0.46,1)
        _ShadowTint("Wypełnienie cienia", Color) = (0.60,0.63,0.68,1)
        _BandSoftness("Miękkość pasm", Range(0.01,0.3)) = 0.08
        _AmbientStrength("Światło otoczenia", Range(0,1)) = 0.65
        _KeyLightLimit("Maksymalna jasność światła głównego", Range(0.5,2)) = 0.95
        _TotalLightLimit("Limit sumy świateł", Range(0.5,2)) = 1.1
        _MinimumLight("Czytelność w cieniu", Range(0,0.5)) = 0.2
        [HDR] _EmissionColor("Emisja trafienia", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "TurrisCommon.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _SteelTint;
            half4 _TrimTint;
            half4 _ShadowTint;
            half4 _EmissionColor;
            half _BandSoftness;
            half _AmbientStrength;
            half _KeyLightLimit;
            half _TotalLightLimit;
            half _MinimumLight;
        CBUFFER_END
        // PropertyBlock statusów HU; moknięcie zmienia barwę, nigdy połysk pancerza.
        float4 _HellSurface;
        float3 _LightDirection;
        float3 _LightPosition;
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            float2 uv : TEXCOORD2;
            half fog : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };
        Varyings Vertex(Attributes input)
        {
            Varyings output = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
            output.positionCS = p.positionCS;
            output.positionWS = p.positionWS;
            output.normalWS = TransformObjectToWorldNormal(input.normalOS);
            output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            output.fog = ComputeFogFactor(p.positionCS.z);
            return output;
        }
        Varyings ShadowVertex(Attributes input)
        {
            Varyings output = Vertex(input);
        #if _CASTING_PUNCTUAL_LIGHT_SHADOW
            float3 direction = normalize(_LightPosition - output.positionWS);
        #else
            float3 direction = _LightDirection;
        #endif
            output.positionCS = ApplyShadowClamping(TransformWorldToHClip(
                ApplyShadowBias(output.positionWS, output.normalWS, direction)));
            return output;
        }
        half Band(half ndl)
        {
            return 0.55h * smoothstep(-_BandSoftness, _BandSoftness, ndl)
                + 0.45h * smoothstep(0.55h - _BandSoftness, 0.55h + _BandSoftness, ndl);
        }
        half3 Diffuse(Light light, half3 normal, bool main)
        {
            half band = Band(dot(normal, light.direction)) * light.shadowAttenuation;
            // Kolor wypełnienia jest dolnym pasmem, bez dodatkowego mnożenia go przez 0,35.
            // Cień opisuje płytę również od przodu, zamiast zamieniać ją w czarną plamę.
            half3 shade = main ? lerp(_ShadowTint.rgb, 1.0h, band) : band;
            // Mocne światło areny nie wypala namalowanych płyt do bieli. Zachowujemy odcień światła i kontrast pasm.
            half peak = max(light.color.r,max(light.color.g,light.color.b));
            half3 energy = light.color*(main ? min(1.0h,_KeyLightLimit/max(0.001h,peak)) : 1.0h);
            return energy * saturate(light.distanceAttenuation) * shade;
        }
        half4 Fragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            InputData inputData = (InputData)0;
            inputData.positionWS = input.positionWS;
            inputData.normalWS = NormalizeNormalPerPixel(input.normalWS);
            inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
            half4 mask = half4(1,1,1,1);
            float4 shadowCoord;
        #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
            shadowCoord = ComputeScreenPos(TransformWorldToHClip(input.positionWS));
        #else
            shadowCoord = TransformWorldToShadowCoord(input.positionWS);
        #endif
            Light mainLight = GetMainLight(shadowCoord, input.positionWS, mask);
            half3 lighting = Diffuse(mainLight, inputData.normalWS, true)
                + SampleSH(inputData.normalWS) * _AmbientStrength;
        #if defined(_ADDITIONAL_LIGHTS)
            uint count = GetAdditionalLightsCount();
            #if USE_CLUSTER_LIGHT_LOOP
            [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
            {
                CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                lighting += Diffuse(GetAdditionalLight(lightIndex, input.positionWS, mask), inputData.normalWS, false);
            }
            #endif
            LIGHT_LOOP_BEGIN(count)
                lighting += Diffuse(GetAdditionalLight(lightIndex, input.positionWS, mask), inputData.normalWS, false);
            LIGHT_LOOP_END
        #endif
            half lightPeak = max(lighting.r,max(lighting.g,lighting.b));
            lighting *= min(1.0h,_TotalLightLimit/max(0.001h,lightPeak));
            lighting = max(lighting,half3(0.85h,0.92h,1.0h)*_MinimumLight);
            half3 pigment = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
            pigment = pow(max(pigment,0.0h),0.9h);
            // Naturalna paleta skóry, rogów i błon; bez mapowania pigmentu na stal paladyna.
            half3 albedo = pigment * _BaseColor.rgb;
            albedo *= lerp(1, 0.18, _HellSurface.x) * lerp(1, 0.72, _HellSurface.y);
            half3 color = albedo * lighting + _EmissionColor.rgb;
            // Żar i sygnały statusów pozostają czytelne; nie są odbiciami zbroi.
            half ember = pow(saturate(TurrisGammaOf(albedo).r - 0.28), 3) * (15 + 3 * sin(_Time.y * 5));
            color += TurrisColor(float3(1.8,0.18,0.02)) * ember * _HellSurface.z;
            color += TurrisColor(float3(0.25,0.10,0.40)) * _HellSurface.x;
            color += TurrisColor(float3(0.65,0.30,0.03)) * _HellSurface.w;
            return half4(MixFog(color, InitializeInputDataFog(float4(input.positionWS,1), input.fog)), 1);
        }
        half4 DepthFragment(Varyings input) : SV_Target { return input.positionCS.z; }
        half4 NormalsFragment(Varyings input) : SV_Target
        {
            half3 normal = NormalizeNormalPerPixel(input.normalWS);
        #if defined(_GBUFFER_NORMALS_OCT)
            float2 octNormal = saturate(PackNormalOctQuadEncode(normal) * 0.5 + 0.5);
            return half4(PackFloat2To888(octNormal), 0);
        #else
            return half4(normal,0);
        #endif
        }
        ENDHLSL
        Pass
        {
            Name "AnimeForward"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVertex
            #pragma fragment DepthFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment DepthFragment
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment NormalsFragment
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}


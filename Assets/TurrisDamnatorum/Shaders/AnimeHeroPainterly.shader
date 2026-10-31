// Bohaterowie: malarska paleta nad oryginalną teksturą 4K; osobny shader, bez wpływu na demony.
Shader "Turris/AnimeHeroPainterly"
{
    Properties
    {
        [MainTexture] _BaseMap("Kolor", 2D) = "white" {}
        [MainColor] _BaseColor("Odcień", Color) = (1,1,1,1)
        _ClothDark("Atrament tkaniny", Color) = (0.12,0.14,0.23,1)
        _ClothLight("Fiolet jasnych warstw", Color) = (0.32,0.32,0.44,1)
        _ClothMix("Korekta granatu i fioletu", Range(0,1)) = 0
        _SkinMix("Malarska skóra", Range(0,1)) = 0
        _DetailStrength("Siła drobnej faktury", Range(0,1)) = 0.55
        _PigmentExposure("Ekspozycja pigmentu", Range(0.5,1.5)) = 1.1
        _EdgeStrength("Subtelna krawędź formy", Range(0,0.5)) = 0.18
        _BakedGlintSuppression("Tłumienie namalowanych refleksów", Range(0,1)) = 0.6
        _UseStyleMask("Maski obszarów modelu", Range(0,1)) = 0
        _SteelDark("Ciemna stal", Color) = (0.20,0.21,0.26,1)
        _SteelLight("Matowa krawędź stali", Color) = (0.43,0.42,0.41,1)
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
            half4 _ClothDark;
            half4 _ClothLight;
            half4 _ShadowTint;
            half4 _EmissionColor;
            half _BandSoftness;
            half _AmbientStrength;
            half _KeyLightLimit;
            half _TotalLightLimit;
            half _MinimumLight;
            half _ClothMix;
            half _SkinMix;
            half _DetailStrength;
            half _PigmentExposure;
            half _EdgeStrength;
            half _BakedGlintSuppression;
            half _UseStyleMask;
            half4 _SteelDark;
            half4 _SteelLight;
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
            half4 styleMask : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            float2 uv : TEXCOORD2;
            half fog : TEXCOORD3;
            half4 styleMask : TEXCOORD4;
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
            output.styleMask=input.styleMask*_UseStyleMask;
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
            return 0.42h * smoothstep(0.12h - _BandSoftness, 0.12h + _BandSoftness, ndl)
                + 0.58h * smoothstep(0.62h - _BandSoftness, 0.62h + _BandSoftness, ndl);
        }
        half3 PainterlyPigment(float2 uv,half4 styleMask)
        {
            half3 raw = TurrisGammaOf(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb);
            half3 broad = TurrisGammaOf(SAMPLE_TEXTURE2D_BIAS(_BaseMap, sampler_BaseMap, uv, 2.8).rgb);
            // Tłumienie tylko podobnych barw. Nie rozmywamy granic skóry, włosów, nitów i wysp UV.
            half edge = length(raw - broad);
            half protectedEdge = smoothstep(0.075h, 0.19h, edge);
            half detail=lerp(_DetailStrength,0.94h,styleMask.r);
            half3 pigment = lerp(broad, raw, lerp(detail, 1.0h, protectedEdge));
            half luma = dot(pigment, half3(0.2126h,0.7152h,0.0722h));
            half cloth = smoothstep(-0.055h,0.015h,pigment.b-pigment.r)
                * smoothstep(-0.045h,0.015h,pigment.b-pigment.g)
                * (1.0h-smoothstep(0.48h,0.64h,luma));
            // Zachowujemy lokalną wartość fałdy; paleta nie zależy od wysokości ani pozy kości.
            half clothValue = smoothstep(0.055h,0.37h,luma);
            half3 clothColor = lerp(TurrisGammaOf(_ClothDark.rgb),TurrisGammaOf(_ClothLight.rgb),clothValue);
            pigment = lerp(pigment,clothColor,cloth*_ClothMix*lerp(1.0h,.48h,styleMask.g));
            half skin = smoothstep(0.36h,0.53h,luma)
                * smoothstep(0.018h,0.075h,raw.r-raw.g)
                * smoothstep(0.025h,0.09h,raw.r-raw.b);
            half skinValue = smoothstep(0.31h,0.76h,luma);
            half3 skinColor = lerp(half3(0.46h,0.33h,0.30h),half3(0.78h,0.64h,0.55h),skinValue);
            pigment = lerp(pigment,skinColor,skin*_SkinMix);
            // Wąskie jasne rysy w albedo nie powinny udawać odbić. Ciemne szwy i rysunek zostają.
            half3 positiveDetail = max(raw-broad,0.0h);
            pigment -= positiveDetail*_BakedGlintSuppression*(1.0h-protectedEdge)*(1.0h-skin*.7h)*(1.0h-styleMask.r);
            // Stal ma własną ograniczoną paletę, zamiast refleksów wypalonych w mapie Meshy.
            half steelValue=smoothstep(.035h,.55h,dot(broad,half3(.2126h,.7152h,.0722h)));
            half3 steel=lerp(TurrisGammaOf(_SteelDark.rgb),TurrisGammaOf(_SteelLight.rgb),steelValue);
            half cavity=saturate(dot(broad-raw,half3(.2126h,.7152h,.0722h))*3.0h);
            steel*=1.0h-cavity*.65h;
            pigment=lerp(pigment,steel,styleMask.a*.90h);
            pigment = saturate(pigment*_PigmentExposure);
            return TurrisColor(pigment)*_BaseColor.rgb;
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
            shadowCoord = float4(inputData.normalizedScreenSpaceUV,0,1);
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
            half3 albedo = PainterlyPigment(input.uv,input.styleMask);
            albedo *= lerp(1, 0.18, _HellSurface.x) * lerp(1, 0.72, _HellSurface.y);
            half3 viewDirection = GetWorldSpaceNormalizeViewDir(input.positionWS);
            half edge = pow(1.0h-saturate(abs(dot(inputData.normalWS,viewDirection))),7.0h);
            // To przyciemnienie stycznych powierzchni, nie obrys każdego trójkąta ani nowy przebieg renderera.
            half3 color = albedo * lighting * (1.0h-edge*_EdgeStrength) + _EmissionColor.rgb;
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


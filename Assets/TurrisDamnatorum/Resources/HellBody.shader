Shader "Turris/HellBody"
{
    Properties
    {
        _Color ("Kolor", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _BumpMap ("Normalna", 2D) = "bump" {}
        _BumpScale ("Siła normalnej", Float) = 1
        _MetallicGlossMap ("Metal / gładkość", 2D) = "white" {}
        _Metallic ("Metaliczność", Range(0,1)) = 0
        _Glossiness ("Gładkość", Range(0,1)) = 0.5
        _GlossMapScale ("Skala gładkości", Range(0,1)) = 1
        _EmissionMap ("Emisja", 2D) = "white" {}
        _EmissionColor ("Kolor emisji", Color) = (0,0,0,0)
        _HellSurface ("Cień / woda / żar / elita", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma shader_feature_local _NORMALMAP
        #pragma shader_feature_local _METALLICGLOSSMAP
        #include "UnityCG.cginc"
        #include "UnityStandardUtils.cginc"
        sampler2D _MainTex, _BumpMap, _MetallicGlossMap, _EmissionMap;
        fixed4 _Color, _EmissionColor;
        float _BumpScale, _Metallic, _Glossiness, _GlossMapScale;
        float4 _HellSurface;
        struct Input { float2 uv_MainTex; float3 viewDir; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = baseColor.rgb * lerp(1, 0.18, _HellSurface.x) * lerp(1, 0.72, _HellSurface.y);
            o.Metallic = _Metallic; o.Smoothness = _Glossiness;
            #ifdef _METALLICGLOSSMAP
                fixed4 metal = tex2D(_MetallicGlossMap, IN.uv_MainTex);
                o.Metallic = metal.r; o.Smoothness = metal.a * _GlossMapScale;
            #endif
            #ifdef _NORMALMAP
                o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
            #endif
            o.Smoothness = lerp(o.Smoothness, 0.94, _HellSurface.y);
            float rim = pow(1 - saturate(dot(normalize(IN.viewDir), o.Normal)), 3);
            float ember = pow(saturate(tex2D(_MainTex, IN.uv_MainTex * 5).r - 0.28), 3) * (15 + 3 * sin(_Time.y * 5));
            o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _EmissionColor.rgb
                + float3(1.8,0.18,0.02) * ember * _HellSurface.z
                + float3(1.2,0.58,0.06) * rim * _HellSurface.w
                + float3(0.17,0.03,0.28) * rim * _HellSurface.x;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

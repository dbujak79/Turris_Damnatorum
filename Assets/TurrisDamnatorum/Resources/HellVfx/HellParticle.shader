Shader "Turris/HellParticle"
{
    Properties
    {
        _MainTex ("Tekstura HU", 2D) = "white" {}
        _TintColor ("Kolor", Color) = (1,1,1,1)
        _Intensity ("Jasność w pipeline Unity", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Mieszanie", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha [_DstBlend]
        Cull Off ZWrite Off
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST, _TintColor;
                float _Intensity, _DstBlend;
            CBUFFER_END
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o; o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.color = v.color * _TintColor; o.uv = TRANSFORM_TEX(v.uv, _MainTex); return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                color.rgb *= _Intensity;
                return color;
            }
            ENDHLSL
        }
    }
}

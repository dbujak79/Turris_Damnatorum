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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha [_DstBlend]
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST, _TintColor;
            float _Intensity;
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _TintColor; o.uv = TRANSFORM_TEX(v.uv, _MainTex); return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv) * i.color;
                color.rgb *= _Intensity;
                return color;
            }
            ENDCG
        }
    }
}

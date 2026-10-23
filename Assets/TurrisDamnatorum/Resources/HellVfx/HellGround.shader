// Wzory z ElementFx.GroundShaderCode HU: ogień, kaustyka, wiatr, pęknięcia, lód.
Shader "Turris/HellGround"
{
    Properties
    {
        _NoiseTex ("Szum HU", 2D) = "gray" {}
        _TintColor ("Kolor", Color) = (1,1,1,1)
        _Color ("Kolor i wygaszanie", Color) = (1,1,1,1)
        _Element ("Rodzina HU", Float) = 0
        _Telegraph ("Zapowiedź", Float) = 0
        _Fill ("Wypełnienie", Range(0,1)) = 1
        _Clock ("Czas gry", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One
        Cull Off ZWrite Off Offset -1, -1
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/TurrisDamnatorum/Shaders/TurrisCommon.hlsl"
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            #define tex2D(t, uv) SAMPLE_TEXTURE2D(t, sampler##t, uv)
            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor, _Color;
                float _Element, _Telegraph, _Fill, _Clock;
            CBUFFER_END
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o; o.vertex=TransformObjectToHClip(v.vertex.xyz); o.uv=v.uv*2-1;
                o.world=TransformObjectToWorld(v.vertex.xyz); return o;
            }
            float4 frag(v2f i):SV_Target
            {
                float t = _Element > 7.5 ? 0 : _Clock;
                float2 w=i.world.xz;
                float d=length(i.uv);
                // Jasny brzeg zawsze pokrywa rzeczywisty promień obrażeń; szum nie zmienia hitboxa.
                float inside=1-smoothstep(0.96,1,d);
                float rim=smoothstep(0.87,0.96,d)*inside;
                float body=inside*lerp(0.35,1,smoothstep(0.2,0.95,d));
                float3 color=TurrisGammaOf(_TintColor.rgb);
                float pattern=0.3;
                float3 a=tex2D(_NoiseTex,w*0.22+float2(t*0.05,-t*0.22)).rgb;
                float3 b=tex2D(_NoiseTex,w*0.5+float2(-t*0.12,-t*0.4)).rgb;
                if (_Element > 0.5 && _Element < 1.5)
                {
                    float heat=clamp(pow(a.r*b.g*2.2,2),0,1.5);
                    color=lerp(float3(0.55,0.05,0),float3(1,0.42,0.06),saturate(heat));
                    color+=float3(1,0.7,0.3)*pow(saturate(heat-0.7),2)*1.5;
                    pattern=0.1+heat*0.8;
                }
                else if (_Element > 1.5 && _Element < 2.5)
                {
                    float caustic=pow(1-min(a.b,b.b),5);
                    color=lerp(float3(0.1,0.3,0.6),float3(0.55,0.85,1),saturate(caustic*1.4)); pattern=0.12+caustic*1.4;
                }
                else if (_Element > 2.5 && _Element < 3.5)
                {
                    float angle=atan2(i.uv.y,i.uv.x);
                    float streak=smoothstep(0.55,0.8,tex2D(_NoiseTex,float2(angle/6.2832*3+d*1.2-t*0.7,d*0.5)).g);
                    color=lerp(color*0.45,float3(0.75,0.95,0.88),streak); pattern=0.05+streak*1.1;
                }
                else if (_Element > 3.5 && _Element < 4.5)
                {
                    float crack=tex2D(_NoiseTex,w*0.32).b;
                    float seam=1-smoothstep(0,0.1,crack);
                    color=lerp(float3(0.3,0.2,0.1),float3(1,0.55,0.15),seam);
                    pattern=0.08+seam*1.5*(0.7+0.3*sin(t*3+w.x))+a.r*0.1;
                }
                else if (_Element > 7.5)
                {
                    float frost=smoothstep(0.4,0.8,a.g*b.r*2);
                    color=lerp(float3(0.22,0.42,0.62),float3(0.8,0.92,1),frost); pattern=0.12+frost*0.9;
                }
                else if (_Element > 4.5 && _Element < 5.5)
                {
                    float smoke=a.r*b.g*2;
                    color=lerp(float3(0.45,0.04,0.02),float3(1,0.3,0.15),saturate(smoke-0.3));
                    pattern=(0.1+smoke*0.45)*(0.75+0.25*sin(t*6));
                }
                float alpha=body*pattern*0.45+rim*0.8;
                if (_Telegraph > 0.5)
                {
                    float fill=1-smoothstep(max(0,_Fill-0.05),max(0.001,_Fill),d);
                    alpha=rim*0.8+fill*inside*(0.1+pattern*0.3);
                }
                return float4(TurrisColor(color),saturate(alpha*_Color.a));
            }
            ENDHLSL
        }
    }
}

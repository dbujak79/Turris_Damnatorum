Shader "Turris/GothicNightSky"
{
    Properties
    {
        _BaseMap("Malowane niebo nocne",2D)="black" {}
        _Rotation("Obrót panoramy",Range(0,1))=0.19
        _Exposure("Jasność",Range(0,2))=0.8
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float _Rotation,_Exposure;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 direction:TEXCOORD0; };
            V Vert(A input)
            {
                V output;
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.direction=input.positionOS.xyz;
                return output;
            }
            half4 Frag(V input):SV_Target
            {
                float3 d=normalize(input.direction);
                float elevation=atan2(d.y,length(d.xz));
                float angle=atan2(d.x,d.z);
                // Pełna panorama w przedniej półsferze zachowuje szczegółowość malowanych chmur.
                // Tył używa bezksiężycowej części tej samej mapy; nie tworzymy drugiego księżyca.
                float2 uv=float2(frac(0.5+angle/3.14159265+_Rotation),saturate(0.20+elevation*2/3.14159265));
                half3 sky=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;
                half3 rear=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,float2(uv.x*.5,uv.y)).rgb;
                sky=lerp(sky,rear,smoothstep(1.42,1.76,abs(angle)));
                half grey=dot(sky,half3(.2126,.7152,.0722));
                return half4(lerp(grey,sky,.72h)*_Exposure,1);
            }
            ENDHLSL
        }
    }
}

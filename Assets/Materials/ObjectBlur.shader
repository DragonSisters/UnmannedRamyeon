Shader "Hidden/URP/ObjectBlur"
{
    Properties
    {
        _Color("Base Color", Color) = (1,1,1,1)
        _BlurStrength("Blur Strength", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "ObjectBlur"
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _BlurStrength;
            float4 _Color;

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // 오브젝트 색상 + 단순 흐림 처리
                // (실제 FullScreen 블러처럼 UV 샘플링 없이 색상을 희미하게)
                half4 col = _Color;
                col.rgb = lerp(col.rgb, float3(0.5,0.5,0.5), _BlurStrength);
                return col;
            }
            ENDHLSL
        }
    }
}

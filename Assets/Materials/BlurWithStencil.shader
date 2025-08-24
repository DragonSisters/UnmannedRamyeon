// 블러 후처리 - 스텐실이 1인 곳은 블러 제외
Shader "Hidden/URP/BlurWithStencil"
{
    Properties
    {
        _Radius("Blur Radius (pixels)", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            Name "FullScreenBlur"
            ZWrite Off
            ZTest Always
            Cull Off

            // 스텐실이 1인 픽셀(=선명하게 둘 영역)은 이 패스를 "그리지 않음"
            Stencil
            {
                Ref 1
                Comp NotEqual
                ReadMask 255
                WriteMask 0
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // URP Full Screen Pass가 제공하는 블릿 텍스처
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _Radius;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings Vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            half4 sampleTex(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
            }

            half4 Frag (Varyings i) : SV_Target
            {
                // 간단 9탭 박스 블러
                float2 texel = _ScreenParams.zw * _Radius; // 1/width,1/height * Radius
                half4 c = half4(0,0,0,0);

                c += sampleTex(i.uv + texel * float2(-1,-1));
                c += sampleTex(i.uv + texel * float2( 0,-1));
                c += sampleTex(i.uv + texel * float2( 1,-1));

                c += sampleTex(i.uv + texel * float2(-1, 0));
                c += sampleTex(i.uv + texel * float2( 0, 0));
                c += sampleTex(i.uv + texel * float2( 1, 0));

                c += sampleTex(i.uv + texel * float2(-1, 1));
                c += sampleTex(i.uv + texel * float2( 0, 1));
                c += sampleTex(i.uv + texel * float2( 1, 1));

                return c / 9.0;
            }
            ENDHLSL
        }
    }
}

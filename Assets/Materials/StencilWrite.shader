// 화면에 스텐실 값 (마스크) 만 찍는 전용 매터리얼
Shader "Hidden/StencilWrite"
{
    subshader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="Transparent" }
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            ColorMask 0 // 화면에 색 안 보이도록

            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return half4(0,0,0,0); // 화면에 색상 안 표시
            }
            ENDHLSL
        }
    }
}

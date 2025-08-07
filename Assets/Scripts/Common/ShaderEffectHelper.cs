using System.Collections;
using UnityEngine;

public class ShaderEffectHelper : MonoBehaviour
{
    private static readonly int outlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
    private static readonly int outlineWidthId = Shader.PropertyToID("_Thickness");
    private static readonly int outlineColorId = Shader.PropertyToID("_SolidOutline");
    private static readonly int SpringScaleId = Shader.PropertyToID("_SpringScale");
    public const float SPRING_ORIGIN_SCALE = 1f;

    public static void SetOutlineEnable(Material material, bool isEnable)
    {
        material.SetFloat(outlineEnabledId, isEnable ? 1f : 0f);
    }

    public static void SetOutlineWidth(Material material, float width)
    {
        material.SetFloat(outlineWidthId, width);
    }

    public static void SetOutlineColor(Material material, Color color)
    {
        material.SetColor(outlineColorId, color);
    }


    public static void SetSpringScaleFloat(Material material, float springScale)
    {
        material.SetFloat(SpringScaleId, springScale);
    }

    public static IEnumerator SpringAnimation(Material material)
    {
        float duration = 0.6f;

        // 랜덤값 설정
        float amplitude = Random.Range(0.2f, 0.25f); // 진폭
        float frequency = Random.Range(15f, 20f);    // 진동수
        float damping = Random.Range(3f, 6f);        // 감쇠 계수

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed;

            // 감쇠 진동 함수 (스프링 수식)
            float scale = 1f + amplitude * Mathf.Exp(-damping * t) * Mathf.Sin(frequency * t);
            material.SetFloat(SpringScaleId, scale);

            yield return null;
        }

        // 애니메이션 끝나면 원래 크기로
        material.SetFloat(SpringScaleId, SPRING_ORIGIN_SCALE);
    }
}

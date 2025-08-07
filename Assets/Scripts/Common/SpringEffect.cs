using System.Collections;
using UnityEngine;

public class SpringEffect : MonoBehaviour
{
    public static readonly int SpringScaleId = Shader.PropertyToID("_SpringScale");
    public const float SPRING_ORIGIN_SCALE = 1f;

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

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIEffectManager : Singleton<UIEffectManager>
{
    private int flickerCount = 2;
    private float dimFactor = 0.4f;
    private float firstFlickerInterval = 0.1f;
    private float secondFlickerInterval = 0.2f;
    private float scaleFactor = 1.1f;

    // @anditsoon TODO: 나중에 bloom 효과 같은 걸 좀 줘야 리얼할 것 같습니다. 일단은 이렇게 간단하게 구현해두고 나중에 포스트 프로세싱 할 때 한꺼번에 합시다.
    public IEnumerator Flicker(Image targetImage)
    {
        Color originalColor = targetImage.color;
        Vector3 originalScale = targetImage.rectTransform.localScale;

        for (int i = 0; i < flickerCount; i++) // 2번 깜빡임
        {
            // 어두워짐 (네온이 순간적으로 꺼지는 느낌) + 조금 작아짐
            targetImage.color = originalColor * dimFactor;
            targetImage.rectTransform.localScale = originalScale;
            yield return new WaitForSeconds(firstFlickerInterval);

            // 다시 밝아짐 + 조금 커짐
            targetImage.color = originalColor;
            targetImage.rectTransform.localScale = originalScale * scaleFactor;
            yield return new WaitForSeconds(secondFlickerInterval);

            // 원래 크기로 돌아오기
            targetImage.rectTransform.localScale = originalScale;
        }

        // 마지막은 원래 색상과 크기로 복원
        targetImage.color = originalColor;
        targetImage.rectTransform.localScale = originalScale;
    }
}

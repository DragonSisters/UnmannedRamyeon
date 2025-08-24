using UnityEngine;
using UnityEngine.UI;

public class UIEffectControl : Singleton<UIEffectControl>
{
    public void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha; // 0 = 완전 투명, 1 = 완전 불투명
        image.color = color;
    }
}

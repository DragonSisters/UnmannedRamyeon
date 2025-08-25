using UnityEngine;
using UnityEngine.UI;

public class UIEffectControl : Singleton<UIEffectControl>
{
    public void SetAlpha(SpriteRenderer spriteRenderer, float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha; // 0 = 완전 투명, 1 = 완전 불투명
        spriteRenderer.color = color;
    }
}

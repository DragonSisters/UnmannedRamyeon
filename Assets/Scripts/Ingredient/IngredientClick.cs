using UnityEngine;

public class IngredientClick : MonoBehaviour, IClickableSprite
{
    public bool IsClickable => isClickable;
    private bool isClickable = false;

    private Material material;
    private readonly Color enableColor = Color.white;
    private readonly Color answerColor = Color.green;
    private readonly Color wrongColor = Color.red;
    private readonly float outlineWidth = 4f;

    public void Initialize()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.LogError($"재료 프리팹에 SpriteRenderer가 없습니다. 원본 프리팹을 확인해주세요.");
        }
        material = spriteRenderer.material;
        ShaderEffect.SetOutlineWidth(material, outlineWidth);
        ShaderEffect.SetOutlineColor(material, enableColor);
    }

    public void OnSpriteClicked()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);

        // 클릭된 IngredientScriptableObject 찾기
        string ingredientName = transform.parent.name;
        IngredientScriptableObject matchingIngredient = IngredientManager.Instance.FindMatchingIngredient(ingredientName);

        // 해당 recipeConsumer 의 ingredientHandler 에 보내기
        IngredientManager.Instance.SendIngredientToCorrectConsumer(matchingIngredient);

        // 선택한 재료가 필요한 재료였는지 검사
        bool isCorrect = IngredientManager.Instance.IsCorrectIngredient(matchingIngredient);
        if(isCorrect)
        {
            ShaderEffect.SetOutlineColor(material, answerColor);
        }
        else
        {
            ShaderEffect.SetOutlineColor(material, wrongColor);
        }

        StartCoroutine(ShaderEffect.SpringAnimation(material));
    }

    public void OnSpriteDeselected()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Unclick);
    }

    public void SetClickable(bool isClickable)
    {
        this.isClickable = isClickable;
        if(isClickable)
        {
            ShaderEffect.SetOutlineEnable(material, true);
            ShaderEffect.SetOutlineColor(material, enableColor);
        }
        else
        {
            ShaderEffect.SetOutlineEnable(material, false);
        }
    }

}

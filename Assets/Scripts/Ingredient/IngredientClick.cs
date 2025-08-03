using UnityEngine;

public class IngredientClick : MonoBehaviour, IClickableSprite
{
    public bool IsClickable => isClickable;
    private bool isClickable = false;

    public void OnSpriteClicked()
    {
        // 클릭된 IngredientScriptableObject 찾기
        string ingredientName = gameObject.name;
        IngredientScriptableObject matchingIngredient = IngredientManager.Instance.FindMatchingIngredient(ingredientName);

        // 해당 recipeConsumer 의 ingredientHandler 에 보내기
        IngredientManager.Instance.SendIngredientToCorrectConsumer(matchingIngredient);
    }

    public void OnSpriteDeselected()
    {

    }

    public void SetClickable(bool isClickable)
    {
        this.isClickable = isClickable;
    }
}

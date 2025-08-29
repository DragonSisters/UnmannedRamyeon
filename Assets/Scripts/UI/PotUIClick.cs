
using UnityEngine;

public class PotUIClick : MonoBehaviour, IClickableSprite
{
    // SetActive 로 조절할 것이기 때문에 항상 true
    public bool IsClickable => true;

    public void OnSpriteClicked()
    {
        RecipeConsumer recipeConsumer = IngredientManager.Instance.CurrentRecipeConsumer;
        if (recipeConsumer != null)
        {
            recipeConsumer.IsAllIngredientSelected = true;
        }
    }

    public void OnSpriteDeselected()
    {
        RecipeConsumer recipeConsumer = IngredientManager.Instance.CurrentRecipeConsumer;
        if (recipeConsumer != null)
        {
            recipeConsumer.IsAllIngredientSelected = false;
        }
    }
}

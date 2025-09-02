
using UnityEngine;

public class SubmitUI : MonoBehaviour, IClickableSprite
{
    // SetActive 로 조절할 것이기 때문에 항상 true
    public bool IsClickable => true;

    public void OnSpriteClicked()
    {
        RecipeConsumer recipeConsumer = IngredientManager.Instance.CurrentRecipeConsumer;
        if (recipeConsumer != null)
        {
            recipeConsumer.IsSubmit = true;
            IngredientManager.Instance.PotUIController.StopSubmitAnim();
        }
    }

    public void OnSpriteDeselected()
    {
        RecipeConsumer recipeConsumer = IngredientManager.Instance.CurrentRecipeConsumer;
        if (recipeConsumer != null)
        {
            recipeConsumer.IsSubmit = false;
        }
    }
}

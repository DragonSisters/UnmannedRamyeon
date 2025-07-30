using UnityEngine;

public class IngredientClick : MonoBehaviour, IClickableSprite
{
    public bool IsClickable => isClickable;
    private bool isClickable = false;

    private void Start()
    {

    }

    public void OnSpriteClicked()
    {
        // RecipeConsumer 일 경우 isClickable = true
    }

    public void OnSpriteDeselected()
    {
        throw new System.NotImplementedException();
    }
}

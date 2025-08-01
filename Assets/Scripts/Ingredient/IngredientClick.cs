using UnityEngine;

public class IngredientClick : MonoBehaviour, IClickableSprite
{
    public bool IsClickable => isClickable;
    private bool isClickable = false;

    public void OnSpriteClicked()
    {
        string IngredientName = SendClickedIngredient();
        Debug.LogError(IngredientName);
    }

    public void OnSpriteDeselected()
    {

    }

    public void SetClickable(bool isClickable)
    {
        this.isClickable = isClickable;
    }

    private string SendClickedIngredient()
    {
        return gameObject.name;
    }
}

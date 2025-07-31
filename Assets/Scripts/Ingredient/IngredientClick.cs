using UnityEngine;

public class IngredientClick : MonoBehaviour, IClickableSprite
{
    public bool IsClickable => isClickable;
    private bool isClickable = false;

    public void Initialize()
    {
    }

    public void OnSpriteClicked()
    {
        
    }

    public void OnSpriteDeselected()
    {
        
    }

    public void SetClickable(bool isClickable)
    {
        this.isClickable = isClickable;
    }
}

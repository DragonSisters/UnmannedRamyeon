using UnityEngine;

public class IngredientClick : MonoBehaviour, IClickableSprite
{
    public bool IsClickable => isClickable;
    private bool isClickable = true;

    public void OnSpriteClicked()
    {
        Debug.LogError($"재료 아이템 {gameObject.name}이 클릭되었습니다.");
    }

    public void OnSpriteDeselected()
    {
        throw new System.NotImplementedException();
    }
}

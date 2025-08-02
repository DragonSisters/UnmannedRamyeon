using TMPro;
using UnityEngine;

public class IngredientBox : MonoBehaviour
{
    [SerializeField] private SpriteRenderer BoxSprite;
    [SerializeField] private SpriteRenderer ingredientSprite;
    [SerializeField] private TMP_Text ingredientNameUI;

    public void SetBoxVisible(bool isOutside)
    {
        if(isOutside)
        {
            BoxSprite.forceRenderingOff = true;
        }
    }

    public void GetOrAddCollision()
    {
        // 콜라이더 추가
        var collider = ingredientSprite.gameObject.GetComponent<PolygonCollider2D>();
        if (collider == null)
        {
            collider = ingredientSprite.gameObject.AddComponent<PolygonCollider2D>();
        }
        // 충돌되지 않도록 trigger on
        collider.isTrigger = true;
    }

    public void SetIngredientSprite(Sprite sprite)
    {
        ingredientSprite.sprite = sprite;
    }

    public void SetIngredientName(string name)
    {
        ingredientNameUI.text = name;
    }
}

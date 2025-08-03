using TMPro;
using UnityEngine;

public class IngredientBox : MonoBehaviour
{
    [SerializeField] private SpriteRenderer BoxSprite;
    [SerializeField] private SpriteRenderer ingredientSprite;
    [SerializeField] private Canvas ingredientCanvas;
    [SerializeField] private TMP_Text ingredientNameUI;
    [SerializeField] private int splitYPosition = 0;
    [SerializeField] private int topDrawingOrder = 6;
    [SerializeField] private int bottomDrawingOrder = 11;
    public GameObject Ingredient => ingredientSprite.gameObject;

    public void SetBoxVisible(bool isOutside)
    {
        if(isOutside)
        {
            BoxSprite.forceRenderingOff = true;
        }
    }

    public void SetSpriteDrawingOrder(Vector2 position)
    {
        var isBottom = position.y < splitYPosition;
        BoxSprite.sortingOrder = isBottom ? bottomDrawingOrder : topDrawingOrder;
        ingredientCanvas.sortingOrder = isBottom ? bottomDrawingOrder : topDrawingOrder;
        ingredientSprite.sortingOrder = isBottom ? bottomDrawingOrder + 1: topDrawingOrder + 1;
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

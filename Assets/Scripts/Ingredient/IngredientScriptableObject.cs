using UnityEngine;

[CreateAssetMenu(fileName = "Ingredient", menuName = "Scriptable Objects/Ingredient")]
public class IngredientScriptableObject : ScriptableObject
{
    [SerializeField] private string name;
    [SerializeField] private Sprite bgSprite;
    [SerializeField] private Sprite icon;
    [SerializeField] private Texture2D cursorIcon;
    [SerializeField] private bool isOutsideBox;
    [SerializeField] private int price;
    [SerializeField] private Vector2 ingredientCreatePosition;
    [SerializeField] private Vector2 point;

    public string Name => name;
    public Sprite BgSprite => bgSprite;
    public Sprite Icon => icon;
    public Texture2D CursorIcon => cursorIcon;
    public bool IsOutsideBox => isOutsideBox;
    public int Price => price;
    public Vector2 IngredientCreatePosition => ingredientCreatePosition;
    public Vector2 Point => ingredientCreatePosition + point;
}

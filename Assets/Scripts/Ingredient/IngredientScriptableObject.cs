using UnityEngine;

[CreateAssetMenu(fileName = "Ingredient", menuName = "Scriptable Objects/Ingredient")]
public class IngredientScriptableObject : ScriptableObject
{
    [SerializeField] private string name;
    [SerializeField] private Sprite icon;
    [SerializeField] private int price;

    public string Name => name;
    public Sprite Icon => icon;
    public int Price => price;
}

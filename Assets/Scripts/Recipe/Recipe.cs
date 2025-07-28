using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] private string name;
    [SerializeField] private IngredientScriptableObject[] ingredients;
    [SerializeField] private int price;

    public string Name => name;
    public IngredientScriptableObject[] Ingredients => ingredients;
    public int Price => price;
}

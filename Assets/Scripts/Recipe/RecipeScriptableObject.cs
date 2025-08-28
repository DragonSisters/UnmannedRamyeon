using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
public class RecipeScriptableObject : ScriptableObject
{
    [SerializeField] private string recipeName;
    [SerializeField] private List<IngredientScriptableObject> ingredients;

    public string Name => recipeName;
    public List<IngredientScriptableObject> Ingredients => ingredients;
}

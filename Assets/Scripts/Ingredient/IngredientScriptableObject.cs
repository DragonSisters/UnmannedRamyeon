using UnityEngine;

[CreateAssetMenu(fileName = "Ingredient", menuName = "Scriptable Objects/Ingredient")]
public class IngredientScriptableObject : ScriptableObject
{
    [SerializeField] private string Name;
    [SerializeField] private Sprite Icon;
}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IngredientPicker : MonoBehaviour
{
    [Header("재료들 리스트")]
    [SerializeField] private List<IngredientScriptableObject> targetIngredients = new();
    [SerializeField] private List<IngredientScriptableObject> ownedIngredients = new();
    [SerializeField] private List<IngredientScriptableObject> requiredIngredients = new();

    [Header("재료를 고를 확률")]
    [SerializeField] private float correctIngredientProbability = 0.9f;
    [SerializeField] private float wrongIngredientProbability = 0.1f;

    [Header("기타")]
    [SerializeField] private Consumer consumer;

    // 1. 타겟 재료들 리스트를 구한다
    public List<IngredientScriptableObject> GetTargetIngredients()
    {
        targetIngredients = gameObject.GetComponent<Consumer>().ingredients;

        return targetIngredients;
    }

    // 2. 현재 필요한 재료를 구한다 (타겟 재료에서 이미 있는 재료를 뺀다)
    public List<IngredientScriptableObject> GetRequiredIngredients()
    {
        foreach(IngredientScriptableObject ingredient in targetIngredients)
        {
            if(!ownedIngredients.Contains(ingredient))
            {
                requiredIngredients.Add(ingredient);
            }
        }

        return requiredIngredients;
    }


    public void PickIngredient()
    {
        if (targetIngredients.Count <= 0 || ownedIngredients.Count <= targetIngredients.Count) return;
    }

    private List<IngredientScriptableObject> UpdateRequiredIngredients(IngredientScriptableObject ingredient)
    {
        requiredIngredients.Remove(ingredient);

        return requiredIngredients;
    }
}

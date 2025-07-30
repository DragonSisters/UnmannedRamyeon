using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GenerateIngredients : MonoBehaviour
{
    void Start()
    {
        CreateIngredientImageOnPosition();
    }

    private void CreateIngredientImageOnPosition()
    {
        List<IngredientScriptableObject> ingredientList = IngredientManager.Instance.IngredientScriptableObject;
        foreach (IngredientScriptableObject ingredient in ingredientList)
        {
            GameObject ingredientGameObj = new GameObject(ingredient.name);
            ingredientGameObj.transform.SetParent(gameObject.transform, false); // 생성할 때는 로컬 좌표 유지
            ingredientGameObj.transform.position = ingredient.Point; // 각 재료별 좌표로 옮김

            // SpriteRenderer 생성
            SpriteRenderer spriteRenderer = ingredientGameObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = ingredientGameObj.AddComponent<SpriteRenderer>();
            }
            spriteRenderer.sprite = ingredient.Icon;


            ingredientGameObj.GetOrAddComponent<IngredientClick>();
        }
    }
}

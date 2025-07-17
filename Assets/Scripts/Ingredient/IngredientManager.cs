using System.Collections.Generic;
using UnityEngine;

public class IngredientManager : Singleton<IngredientManager>
{
    [SerializeField] private List<IngredientScriptableObject> ingredientScriptableObject = new();


    void Start()
    {
        if(ingredientScriptableObject.Count <= 0)
        {
            throw new System.Exception($"재료매니저에 등록된 재료가 하나도 없습니다. {this.name}를 확인해주세요.");
        }
    }

    public List<IngredientScriptableObject> GetRandomIngredients(int count)
    {
        if (count > ingredientScriptableObject.Count)
        {
            Debug.LogWarning("요청한 개수가 리스트 크기보다 큽니다.");
            count = ingredientScriptableObject.Count;
        }

        // 원본 리스트 복사
        var shuffledList = new List<IngredientScriptableObject>(ingredientScriptableObject);

        // Fisher-Yates 셔플 (일부만)
        for (int i = 0; i < count; i++)
        {
            var randomIndex = Random.Range(i, shuffledList.Count);
            var temp = shuffledList[i];
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // 앞에서 n개만 반환
        var selectedList = shuffledList.GetRange(0, count);
        Debug.Log($"손님이 고른 재료들: {string.Join(", ", selectedList)}");
        return selectedList;
    }
}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IngredientManager : Singleton<IngredientManager>
{
    public const int MAX_INGREDIENT_NUMBER = 4;
    public const float INGREDIENT_PICKUP_TIME = 2f;
    public const float UI_DURATION_ON_COLLECT = 1f;
    public const float UI_DURATION_PREVIEW = 2f;
    public const float CORRECT_INGREDIENT_PROBAILITY = 9f;
    public List<IngredientScriptableObject> IngredientScriptableObject = new();
    [SerializeField] private Transform ingredientsParent;
    [SerializeField] private GameObject ingredientBoxPrefab;
    List<IngredientClick> ingredientsClickable = new List<IngredientClick>();

    private bool isIngredientSelectMode;
    public bool IsIngredientSelectMode
    {
        get => isIngredientSelectMode;
        set
        {
            if (isIngredientSelectMode == value) return;

            isIngredientSelectMode = value;

            if (isIngredientSelectMode)
            {
                OnIngredientSelectMode?.Invoke();
            }
            else
            {
                OnIngredientDeselectMode?.Invoke();
            }
        }
    }
    public event System.Action OnIngredientSelectMode;
    public event System.Action OnIngredientDeselectMode;

    // 현재 처리하고 있는 레시피 손님
    private RecipeConsumer currentRecipeConsumers = null;
    // 레시피 손님이 가져간 재료 수
    private int currPickCount = 0;

    void Start()
    {
        if(IngredientScriptableObject.Count <= 0)
        {
            throw new System.Exception($"재료매니저에 등록된 재료가 하나도 없습니다. {this.name}를 확인해주세요.");
        }

        foreach (var item in IngredientScriptableObject)
        {
            var isValidate = IsValidate(item);
            if(!isValidate)
            {
                throw new System.Exception($"재료({item.Name})에 문제가 있습니다 scriptable object를 확인해주세요.");
            }
        }

        OnIngredientSelectMode += HandleIngredientSelectMode;
        OnIngredientDeselectMode += HandleIngredientDeselectMode;
    }

    public void CreateIngredientObjOnPosition()
    {
        foreach (IngredientScriptableObject ingredient in IngredientScriptableObject)
        {
            GameObject ingredientGameObj = Instantiate(ingredientBoxPrefab, ingredientsParent);
            ingredientGameObj.name = ingredient.name;

            ingredientGameObj.transform.position = ingredient.IngredientCreatePosition; // 각 재료별 좌표로 옮김

            IngredientBox ingredientBox = ingredientGameObj.GetComponent<IngredientBox>();
            if (ingredientBox == null)
            {
                Debug.Log($"재료 박스 프리팹에 스크립트가 부착되지 않았습니다.");
            }
            ingredientBox.SetIngredientSprite(ingredient.BgSprite);
            ingredientBox.SetIngredientName(ingredient.Name);
            ingredientBox.GetOrAddCollision();
            ingredientBox.SetBoxVisible(ingredient.IsOutsideBox);

            IngredientClick ingredientClick = ingredientBox.Ingredient.GetOrAddComponent<IngredientClick>();
            ingredientsClickable.Add(ingredientClick);
        }
    }

    public void SwitchClickable(bool clickable)
    {
        if(ingredientsClickable == null || ingredientsClickable.Count == 0)
        {
            Debug.LogWarning("ingredientsClickable 리스트가 비어있습니다.");
            return;
        }

        foreach(IngredientClick ingredientClick in ingredientsClickable)
        {
             ingredientClick.SetClickable(clickable);
        }
    }

    private void HandleIngredientSelectMode()
    {
        Debug.Log("셀렉트 이벤트 받았다");
        // ingredient 클릭 활성화
        SwitchClickable(true);
    }

    private void HandleIngredientDeselectMode()
    {
        Debug.Log("디셀렉트 이벤트 받았다");
        // ingredient 클릭 비활성화
        SwitchClickable(false);
    }

    private bool IsValidate(IngredientScriptableObject ingredient)
    {
        if(string.IsNullOrEmpty(ingredient.Name))
        {
            return false;
        }
        if(ingredient.Icon == null)
        {
            return false;
        }
        if(ingredient.Price == 0)
        {
            return false;
        }
        return true;
    }

    public List<IngredientScriptableObject> GetPaidIngredients(int count)
    {
        if (count > IngredientScriptableObject.Count)
        {
            Debug.LogWarning("요청한 개수가 리스트 크기보다 큽니다.");
            count = IngredientScriptableObject.Count;
        }

        // 원본 리스트 복사
        var shuffledList = ShufflePartOfList(IngredientScriptableObject, count);

        // 앞에서 n개만 반환
        var selectedList = shuffledList.GetRange(0, count);
        Debug.Log($"손님이 고른 재료들: {string.Join(", ", selectedList)}");
        return selectedList;
    }

    public List<T> ShufflePartOfList<T>(List<T> originalList, int count)
    {
        List<T> shuffledList = new List<T>(originalList);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, shuffledList.Count);
            T temp = shuffledList[i];
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        return shuffledList;
    }

    public void ReceiveRecipeConsumer(RecipeConsumer recipeConsumer)
    {
        currentRecipeConsumers = recipeConsumer;
    }

    public void DeleteRecipeConsumer()
    {
        if (currentRecipeConsumers == null)
        {
            Debug.LogWarning($"현재 돕고 있는 손님이 없습니다");
            return;
        }

        currentRecipeConsumers = null;
        currPickCount = 0;
    }

    public IngredientScriptableObject FindMatchingIngredient(string ingredientName)
    {
        foreach(IngredientScriptableObject ingredient in IngredientScriptableObject)
        {
            if(ingredient.name == ingredientName)
            {
                return ingredient;
            }
        }

        throw new System.Exception($"{ingredientName}이라는 이름과 일치하는 재료가 없습니다.");
    }

    public void SendIngredientToCorrectConsumer(IngredientScriptableObject ingredient)
    {
        ConsumerIngredientHandler ingredientHandler = currentRecipeConsumers.gameObject.GetComponent<ConsumerIngredientHandler>();
        ingredientHandler.AddAttemptIngredients(ingredient, out bool isNoDuplicate);
        if(isNoDuplicate) currPickCount++;

        if (currPickCount >= MAX_INGREDIENT_NUMBER)
        {
            currentRecipeConsumers.IsAllIngredientSelected = true;
            currPickCount = 0;
            IsIngredientSelectMode = false;
        }
    }
}

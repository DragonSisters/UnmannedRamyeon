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
    [SerializeField] private GameObject ingredientsParent;
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

    [Header("현재 처리하고 있는 레시피 손님들")] // @anditsoon TODO: 테스트를 위해 보이게 처리한 것, 나중에 지울 것.
    [SerializeField] private Dictionary<int, RecipeConsumer> recipeConsumers = new Dictionary<int, RecipeConsumer>();

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
        List<IngredientScriptableObject> ingredientList = IngredientManager.Instance.IngredientScriptableObject;
        foreach (IngredientScriptableObject ingredient in ingredientList)
        {
            GameObject ingredientGameObj = new GameObject(ingredient.name);
            ingredientGameObj.transform.SetParent(ingredientsParent.transform, false); // 생성할 때는 로컬 좌표 유지
            ingredientGameObj.transform.position = ingredient.Point; // 각 재료별 좌표로 옮김

            // SpriteRenderer 생성
            SpriteRenderer spriteRenderer = ingredientGameObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = ingredientGameObj.AddComponent<SpriteRenderer>();
            }
            spriteRenderer.sprite = ingredient.Icon;

            // 박스 콜라이더 추가
            var boxCollider = ingredientGameObj.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = ingredientGameObj.AddComponent<BoxCollider2D>();
            }
            // 충돌되지 않도록 trigger on
            boxCollider.isTrigger = true;

            IngredientClick ingredientClick = ingredientGameObj.GetOrAddComponent<IngredientClick>();
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
        // 클릭해서 네 개 다 맞는 재료 클릭하면 성공 -> 파이낸스 매니저에 보냄
        // 아니면 실패 -> 그냥 바로 퇴장
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

    public List<IngredientScriptableObject> GetTargetIngredients(int count)
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

    public void ReceiveRecipeCx(int index, RecipeConsumer recipeConsumer)
    {
        recipeConsumers[index] = recipeConsumer;
        Debug.LogError("레시피 컨슈머 저장~");

        Debug.LogError(string.Join(", ", recipeConsumers.Keys));
    }

    public void DeleteRecipeCx(int index)
    {
        if (!recipeConsumers.ContainsKey(index))
        {
            Debug.LogWarning($"해당 인덱스({index})가 딕셔너리에 존재하지 않습니다.");
            return;
        }

        recipeConsumers.Remove(index);
        Debug.LogError("레시피 컨슈머 삭제~");

        Debug.LogError(string.Join(", ", recipeConsumers.Keys));
    }
}

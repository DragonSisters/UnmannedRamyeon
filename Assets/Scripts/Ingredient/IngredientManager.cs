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
        }
    }
    public event System.Action OnIngredientSelectMode;

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
        Debug.LogWarning("이벤트 받았다");
        // ingredient 클릭 활성화
        SwitchClickable(true);
        // 다른 데 클릭하면 다시 일반 모드로 돌아가야 됨
        // 클릭해서 네 개 다 맞는 재료 클릭하면 성공 -> 파이낸스 매니저에 보냄
        // 아니면 실패 -> 그냥 바로 퇴장
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
}

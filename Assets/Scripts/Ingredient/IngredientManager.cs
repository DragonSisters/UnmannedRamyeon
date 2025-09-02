using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IngredientManager : Singleton<IngredientManager>
{
    public const int MAX_COMMON_CONSUMER_INGREDIENT_NUMBER = 4;
    public const float INGREDIENT_PICKUP_TIME = 2f;
    public const float UI_DURATION_ON_COLLECT = 1f;
    public const float UI_DURATION_PREVIEW = 2f;
    public const float CORRECT_INGREDIENT_PROBAILITY = 8f;
    
    [SerializeField] private GameObject stencil;
    [SerializeField] private CapsuleCollider2D potCollider;
    [SerializeField] private List<SpriteRenderer> ingredientsInPot;

    private bool isFirstIngredientIn = false;
    public bool IsFirstIngredientIn
    {
        get => isFirstIngredientIn;
        set
        {
            if(isFirstIngredientIn == value) return;

            isFirstIngredientIn = value;

            if (isFirstIngredientIn) OnBringFirstIngredient?.Invoke();
        }
    }
    private bool isFirstWrongIngredientOut = false;
    public bool IsFirstWrongIngredientOut
    {
        get => isFirstWrongIngredientOut;
        set
        {
            if(isFirstWrongIngredientOut == value) return;

            isFirstWrongIngredientOut = value;

            if (isFirstWrongIngredientOut) OnTakeOutFirstWrongIngredient?.Invoke();
        }
    }

    public List<IngredientScriptableObject> IngredientScriptableObject = new();
    [SerializeField] private Transform ingredientsParent;
    [SerializeField] private GameObject ingredientBoxPrefab;
    [SerializeField] private List<IngredientDrag> ingredientsDraggable = new List<IngredientDrag>();
    private List<GameObject> ingredientGameObjs = new List<GameObject>();
    private bool isInitializeInPot = false;

    private bool isIngredientSelectMode = false;
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
    public event Action OnIngredientSelectMode;
    public event Action OnIngredientDeselectMode;
    public event Action OnBringFirstIngredient;
    public event Action OnTakeOutFirstWrongIngredient;

    // 현재 처리하고 있는 레시피 손님
    private RecipeConsumer currentRecipeConsumer = null;
    public RecipeConsumer CurrentRecipeConsumer => currentRecipeConsumer;

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

        UIManager.Instance.PotUIController.Initialize();
    }

    public void ActivateIngredientObjOnPosition(bool IsActive)
    {
        foreach (var obj in ingredientGameObjs)
        {
            obj.SetActive(IsActive);
        }
    }

    public void OnGameEnd()
    {
        if(currentRecipeConsumer != null)
        {
            OnRecipeConsumerFinished(currentRecipeConsumer, false);
            currentRecipeConsumer = null;
        }

        isFirstIngredientIn = false;
        isFirstWrongIngredientOut = false;

        ActivateIngredientObjOnPosition(false);
        HandleIngredientDeselectMode();
        UIManager.Instance.PotUIController.OnGameEnd();
    }

    public void CreateIngredientObjOnPosition()
    {
        // 이미 생성되어 있다면 생성하지 않고 Activate만 해줍니다.
        if(ingredientGameObjs.Count == IngredientScriptableObject.Count)
        {
            ActivateIngredientObjOnPosition(true);
            return;
        }

        foreach (IngredientScriptableObject ingredient in IngredientScriptableObject)
        {
            GameObject ingredientGameObj = Instantiate(ingredientBoxPrefab, ingredientsParent);
            ingredientGameObj.name = ingredient.name;
            ingredientGameObj.layer = LayerMask.NameToLayer("SharpMask");

            ingredientGameObj.transform.position = ingredient.IngredientCreatePosition; // 각 재료별 좌표로 옮김

            IngredientBox ingredientBox = ingredientGameObj.GetComponent<IngredientBox>();
            if (ingredientBox == null)
            {
                Debug.LogError($"재료 박스 프리팹에 스크립트가 부착되지 않았습니다.");
            }
            ingredientBox.SetLayerForSprite();
            ingredientBox.SetIngredientSprite(ingredient.BgSprite);
            ingredientBox.SetIngredientName(ingredient.Name);
            ingredientBox.GetOrAddCollision();
            ingredientBox.SetBoxVisible(ingredient.IsOutsideBox);
            ingredientBox.SetSpriteDrawingOrder(ingredient.IngredientCreatePosition);

            IngredientDrag ingredientDrag = ingredientBox.Ingredient.GetOrAddComponent<IngredientDrag>();
            ingredientDrag.Initialize(ingredient, potCollider, ingredientsInPot);
            ingredientsDraggable.Add(ingredientDrag);
            ingredientGameObjs.Add(ingredientGameObj);
        }
    }

    public void SwitchDraggable(bool draggable)
    {
        if(ingredientsDraggable == null || ingredientsDraggable.Count == 0)
        {
            Debug.LogWarning("ingredientsDraggable 리스트가 비어있습니다.");
            return;
        }

        // Pot 안의 재료들의 IngredientDrag 에서만, 가장 처음 한 번만
        if(!isInitializeInPot)
        {
            isInitializeInPot = true;

            for(int i = 0; i < ingredientsInPot.Count; i++)
            {
                var ingredientDrag = ingredientsInPot[i].gameObject.GetComponent<IngredientDrag>();
                if(ingredientDrag != null)
                {
                    // 시작할 때에는 IngredientScriptableObject 리스트의 처음 네 IngredientScriptableObject 로 넣어놓습니다. (나중에 재료를 냄비에 넣을 때 업데이트)
                    ingredientDrag.Initialize(IngredientScriptableObject[i], potCollider, ingredientsInPot);
                    ingredientsDraggable.Add(ingredientDrag);
                }
            }
        }

        foreach(IngredientDrag ingredientDrag in ingredientsDraggable)
        {
            ingredientDrag.SetDraggable(draggable);
        }
    }

    public bool IsCorrectIngredient(IngredientScriptableObject ingredient)
    {
        if(currentRecipeConsumer == null)
        {
            throw new System.Exception($"선택된 레시피 손님이 없는데 가져온 재료가 필요한 재료가 맞는지 검사하려 했습니다.");
        }

        return currentRecipeConsumer.MyRecipe.Ingredients.Contains(ingredient);
    }

    private void HandleIngredientSelectMode()
    {
        UIManager.Instance.PotUIController.EnqueuePotRoutine(UIManager.Instance.PotUIController.BringPot());
        stencil.SetActive(true);

        // ingredient 클릭 활성화
        SwitchDraggable(true);
    }

    private void HandleIngredientDeselectMode()
    {
        // ingredient 클릭 비활성화
        SwitchDraggable(false);
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

    #region RecipeConsumer 관리

    public void ReceiveRecipeConsumer(RecipeConsumer consumer)
    {
        if (CurrentRecipeConsumer != null && CurrentRecipeConsumer != consumer)
        {
            CurrentRecipeConsumer.StopIssueCoroutine();
        }
        currentRecipeConsumer = consumer;
    }

    public void RemoveRecipeConsumer(RecipeConsumer consumer)
    {
        if (currentRecipeConsumer == consumer)
        {
            currentRecipeConsumer = null;
        }
    }

    public void SendIngredientToCorrectConsumer(IngredientScriptableObject ingredient, out bool isNoDuplicate)
    {
        isNoDuplicate = false;

        if (currentRecipeConsumer == null)
        {
            Debug.LogError("currentRecipeConsumer 가 없습니다.");
            return;
        }
        ConsumerIngredientHandler ingredientHandler = currentRecipeConsumer.gameObject.GetComponent<ConsumerIngredientHandler>();
        if (ingredientHandler == null)
        {
            Debug.LogError("ConsumerIngredientHandler 가 없습니다.");
            return;
        }

        ingredientHandler.AddAttemptIngredients(ingredient, out isNoDuplicate);
        if (isNoDuplicate)
        {
            currentRecipeConsumer.AddPickCount();
        }
        else
        {
            ConsumerSpeech consumerSpeech = currentRecipeConsumer.GetComponent<ConsumerSpeech>();
            if (consumerSpeech == null)
            {
                Debug.LogWarning("ConsumerSpeech 를 찾을 수 없습니다");
                return;
            }
            StartCoroutine(consumerSpeech.StartSpeechFromSituation(currentRecipeConsumer.currentConsumerScriptableObject, ConsumerSituation.DuplicateIngredientDetected, false, false, true, false));
            StartCoroutine(consumerSpeech.StartSpeechFromSituation(currentRecipeConsumer.currentConsumerScriptableObject, ConsumerSituation.RecipeOrder, true, true, true, true, -1, $"{currentRecipeConsumer.MyRecipe.Name}"));
        }

        if(ingredientHandler.IsAllIngredientCorrect())
        {
            currentRecipeConsumer.IsAllIngredientCorrect = true;
            UIManager.Instance.PotUIController.PlaySubmitAnim();
        }
        else
        {
            currentRecipeConsumer.IsAllIngredientCorrect = false;
            UIManager.Instance.PotUIController.StopSubmitAnim();
        }

        // 타이머 쓰는 경우는 조리완료 버튼을 쓰지 않고 바로 서빙
        if (currentRecipeConsumer.CurrPickCount >= currentRecipeConsumer.MyRecipe.Ingredients.Count && GameManager.Instance.UseRecipeConsumerTimer)
        {
            currentRecipeConsumer.IsSubmit = true;
        }
    }

    public void RemoveIngredientFromCorrectCunsumer(IngredientScriptableObject ingredient)
    {
        if (currentRecipeConsumer == null)
        {
            Debug.LogError("currentRecipeConsumer 가 없습니다.", this);
            Debug.LogError(Environment.StackTrace);
            return;
        }
        ConsumerIngredientHandler ingredientHandler = currentRecipeConsumer.gameObject.GetComponent<ConsumerIngredientHandler>();
        if (ingredientHandler == null)
        {
            Debug.LogError("ConsumerIngredientHandler 가 없습니다.");
            return;
        }

        currentRecipeConsumer.GetComponent<ConsumerIngredientHandler>().RemoveWrongIngredient(ingredient);

        if (ingredientHandler.IsAllIngredientCorrect())
        {
            currentRecipeConsumer.IsAllIngredientCorrect = true;
            UIManager.Instance.PotUIController.PlaySubmitAnim();
        }
        else
        {
            currentRecipeConsumer.IsAllIngredientCorrect = false;
            UIManager.Instance.PotUIController.StopSubmitAnim();
        }
    }

    public void OnRecipeConsumerFinished(RecipeConsumer consumer, bool IsPlaySlideOutAnim)
    {
        if(currentRecipeConsumer == consumer)
        {
            UIManager.Instance.PotUIController.EnqueuePotRoutine(UIManager.Instance.PotUIController.RemovePot(IsPlaySlideOutAnim));
            consumer.OnConsumerHelpFinished();
            RemoveRecipeConsumer(consumer);

            UIManager.Instance.PotUIController.StopShakeAnim();
            stencil.SetActive(false);
            IsIngredientSelectMode = false;
        }
    }

    #endregion
}

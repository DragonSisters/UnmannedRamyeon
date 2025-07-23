using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 손님에게 상속되어야합니다.
/// </summary>
public abstract class Consumer : MonoBehaviour, IPoolable
{
    [SerializeField] internal ConsumerScriptableObject consumerScriptableObject;
    [SerializeField] internal ConsumerIngredientHandler ingredientHandler;
    [SerializeField] internal ConsumerUI consumerUI;
    internal ConsumerMood moodScript;

    /// <summary>
    /// 손님의 상태. 값을 설정할 떄 SetState() 함수를 사용합니다.
    /// </summary>
    public ConsumerState State { get; private set; }
    /// <summary>
    /// 손님 상태를 설정할 때 무조건 이 함수를 사용하도록 합니다.
    /// </summary>
    internal void SetState(ConsumerState newState)
    {
        State = newState;

        // Invalid 초기화일 때는 대사를 건너뜁니다.
        if (State == ConsumerState.Invalid)
        {
            return;
        }
        var line = consumerScriptableObject.GetDialogueFromState(newState);
        print($"손님{gameObject.name}: {string.Join(", ", line)}");
    }

    internal bool IsIssueSolved;

    internal List<IngredientScriptableObject> ingredients = new();
    private const int INGREDIENT_COUNT = 4;

    // 추상 함수
    internal abstract void OnEnter();
    internal abstract void OnExit();
    internal abstract IEnumerator OnUpdate();

    public void OnSpawn()
    {
        OnCustomerEnter();
        StartCoroutine(UpdateCustomerBehavior());
    }

    public void OnDespawn()
    {
        OnCustomerExit();
        StopCoroutine(UpdateCustomerBehavior());
        StopCoroutine(OnUpdate());
        ingredientHandler.ResetAllIngredientLists();
        consumerUI.DeactivateAllFeedbackUIs();
    }

    public bool ShouldDespawn()
    {
        // exit 상태에서 완료되었을 때 호출됩니다. delegate처리
        return false; // @charotiti9 TODO: 임시
    }
    private void Initialize()
    {
        SetState(ConsumerState.Invalid);
        IsIssueSolved = false;
        ingredients.Clear();

        // 스프라이트 렌더러 추가
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = consumerScriptableObject.Appearance;
        // 박스 콜라이더 추가
        var boxCollider = gameObject.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        // 충돌되지 않도록 trigger on
        boxCollider.isTrigger = true;
        // 손님 기분 스크립트 추가
        moodScript = gameObject.GetComponent<ConsumerMood>();
        if (moodScript == null)
        {
            moodScript = gameObject.AddComponent<ConsumerMood>();
        }
        moodScript.Initialize();
    }

    /// <summary>
    /// 손님이 들어올 때 해야하는 행동
    /// </summary>
    private void OnCustomerEnter()
    {
        Initialize();

        SetState(ConsumerState.Enter);

        // 재료를 고르고 필요한 재료의 리스트와 필요한 재료의 총 갯수를 구합니다.
        ingredientHandler.targetIngredients = IngredientManager.Instance.GetRandomIngredients(INGREDIENT_COUNT);
        ingredientHandler.maxIngredientNumber = ingredientHandler.targetIngredients.Count;

        // 필요한 재료들을 머리 위에 아이콘으로 표시합니다.
        consumerUI.UpdateIngredientImages(ingredientHandler.targetIngredients);

        // 필요하지 않은 재료의 리스트를 구합니다.
        ingredientHandler.untargetedIngredients = ingredientHandler.GetIngredientLists(ingredientHandler.targetIngredients, ingredientHandler.untargetedIngredients);

        // 재료를 고르기 시작합니다.
        if (!(ingredientHandler.targetIngredients.Count <= 0 || ingredientHandler.ownedIngredients.Count >= ingredientHandler.maxIngredientNumber)) 
            StartCoroutine(ingredientHandler.ChooseIngredientRoutine());

        OnEnter();
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    private void OnCustomerExit()
    {
        SetState(ConsumerState.Exit);
        OnExit();
    }

    /// <summary>
    /// 손님이 머무는 동안 해야하는 행동(update)
    /// </summary>
    private IEnumerator UpdateCustomerBehavior()
    {
        SetState(ConsumerState.Search);
        StartCoroutine(OnUpdate());

        while (!ShouldDespawn())
        {
            yield return null;
        }
    }
}

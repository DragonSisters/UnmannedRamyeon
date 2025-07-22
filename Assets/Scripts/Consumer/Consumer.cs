using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 모든 손님에게 상속되어야합니다.
/// </summary>
public abstract class Consumer : MonoBehaviour, IPoolable
{
    [SerializeField] internal ConsumerScriptableObject consumerScriptableObject;
    [SerializeField] internal ConsumerIngredientHandler ingredientHandler;
    [SerializeField] internal ConsumerUI consumerUI;
    internal ConsumerState state;
    internal bool IsIssueSolved;
    internal float spawnedTime;
    internal List<IngredientScriptableObject> ingredients = new();
    private const int INGREDIENT_COUNT = 4;

    

    // 추상 함수
    internal abstract void OnEnter();
    internal abstract void OnExit();
    internal abstract IEnumerator OnUpdate();

    // @anditsoon TODO: GetKeyDown 으로 테스트 완료, 이제 코루틴으로 순서대로 실행되게 해야 함.
    //                  실행 시 if 문 안의 조건들을 같이 호출할 것.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (ingredientHandler.targetIngredients.Count <= 0 || ingredientHandler.ownedIngredients.Count >= ingredientHandler.maxIngredientNumber) return;

            StartCoroutine(ingredientHandler.ChooseIngredientRoutine());
        }
    }

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
    }

    public bool ShouldDespawn()
    {
        // @charotiti9 TODO: 나중에 체류시간이 삭제되고
        //      1. 문제가 해결되었을 때
        //      2. 해결할 시간이 지났을 때
        //      3. 재료를 다 골랐을 때
        // 퇴장하는 것으로 변경합니다
        
        // 체류시간이 다 되었다면 퇴장
        if(consumerScriptableObject.LifeTime == 0)
        {
            throw new System.ArgumentException("손님의 체류시간이 0초일 수 없습니다. ScriptableObject를 확인해주세요.");
        }
        return Time.time - spawnedTime >= consumerScriptableObject.LifeTime;
    }

    private void Initialize()
    {
        state = ConsumerState.Invalid;
        IsIssueSolved = false;
        spawnedTime = 0f;
        ingredients.Clear();
    }

    /// <summary>
    /// 손님이 들어올 때 해야하는 행동
    /// </summary>
    private void OnCustomerEnter()
    {
        Initialize();

        spawnedTime = Time.time;
        state = ConsumerState.Enter;

        // 스프라이트 렌더러 추가
        var spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = consumerScriptableObject.Appearance;
        // 박스 콜라이더 추가
        var boxCollider = gameObject.GetOrAddComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        // 충돌되지 않도록 trigger on
        boxCollider.isTrigger = true;

        // @charotiti9 TODO: 등장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumerScriptableObject.GetDialogueFromState(ConsumerState.Enter);
        print($"손님{gameObject.name}: {string.Join(", ", line)}");

        // 재료를 고르고 필요한 재료의 리스트와 필요한 재료의 총 갯수를 구합니다.
        ingredientHandler.targetIngredients = IngredientManager.Instance.GetRandomIngredients(INGREDIENT_COUNT);
        ingredientHandler.maxIngredientNumber = ingredientHandler.targetIngredients.Count;

        // 필요한 재료들을 머리 위에 아이콘으로 표시합니다.
        consumerUI.UpdateIngredientImages(ingredientHandler.targetIngredients);

        // 필요하지 않은 재료의 리스트를 구합니다.
        ingredientHandler.untargetedIngredients = ingredientHandler.GetIngredientLists(ingredientHandler.targetIngredients, ingredientHandler.untargetedIngredients);

        OnEnter();
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    private void OnCustomerExit()
    {
        state = ConsumerState.Exit;

        // @charotiti9 TODO: 퇴장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumerScriptableObject.GetDialogueFromState(ConsumerState.Exit);
        print($"손님{gameObject.name}: {string.Join(", ", line)}");

        OnExit();
    }

    /// <summary>
    /// 손님이 머무는 동안 해야하는 행동(update)
    /// </summary>
    private IEnumerator UpdateCustomerBehavior()
    {
        state = ConsumerState.Usual;
        StartCoroutine(OnUpdate());

        while (!ShouldDespawn())
        {
            // @charotiti9 TODO: 각종 대사를 외쳐요. 나중에 손님 상태에 따라서 말하는 대사를 변경해야합니다.
            var line = consumerScriptableObject.GetDialogueFromState(state);
            print($"손님{gameObject.name}: {string.Join(", ", line)}");

            yield return null;
        }
    }
}

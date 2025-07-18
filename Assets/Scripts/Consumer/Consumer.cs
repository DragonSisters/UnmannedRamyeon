using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 모든 손님에게 상속되어야합니다.
/// </summary>
public class Consumer : MonoBehaviour, IPoolable
{
    [SerializeField] internal ConsumerScriptableObject consumerScriptableObject;
    internal ConsumerState state;
    internal float spawnedTime;
    internal List<IngredientScriptableObject> ingredients = new();
    private const int INGREDIENT_COUNT = 4;

    public void OnSpawn()
    {
        OnCustomerEnter();
        StartCoroutine(UpdateCustomerBehavior());
    }

    public void OnDespawn()
    {
        OnCustomerExit();
        StopCoroutine(UpdateCustomerBehavior());
    }

    public bool ShouldDespawn()
    {
        // 체류시간이 다 되었다면 퇴장
        if(consumerScriptableObject.LifeTime == 0)
        {
            throw new System.ArgumentException("손님의 체류시간이 0초일 수 없습니다. ScriptableObject를 확인해주세요.");
        }
        return Time.time - spawnedTime >= consumerScriptableObject.LifeTime;
    }

    public void Initialize()
    {
        state = ConsumerState.Invalid;
        spawnedTime = 0f;
        ingredients.Clear();
    }

    /// <summary>
    /// 손님이 들어올 때 해야하는 행동
    /// </summary>
    internal virtual void OnCustomerEnter()
    {
        Initialize();

        spawnedTime = Time.time;
        state = ConsumerState.Enter;

        var spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = consumerScriptableObject.Appearance;

        // @charotiti9 TODO: 등장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumerScriptableObject.GetDialogueFromState(ConsumerState.Enter);
        print($"손님{gameObject.name}: {string.Join(", ", line)}");

        // 재료를 고릅니다.
        ingredients = IngredientManager.Instance.GetRandomIngredients(INGREDIENT_COUNT);

        // 필요한 재료를 구합니다.
        gameObject.GetComponent<IngredientPicker>().GetTargetIngredients();
        gameObject.GetComponent<IngredientPicker>().GetRequiredIngredients();
    }

    /// <summary>
    /// 손님이 떠날 때 해야하는 행동
    /// </summary>
    internal virtual void OnCustomerExit()
    {
        state = ConsumerState.Exit;

        // @charotiti9 TODO: 퇴장대사를 외친다. 지금은 print로 간단히 처리
        var line = consumerScriptableObject.GetDialogueFromState(ConsumerState.Exit);
        print($"손님{gameObject.name}: {string.Join(", ", line)}");
    }

    /// <summary>
    /// 손님이 머무는 동안 해야하는 행동(update)
    /// </summary>
    internal virtual IEnumerator UpdateCustomerBehavior()
    {
        state = ConsumerState.Usual;

        while (!ShouldDespawn())
        {
            // @charotiti9 TODO: 각종 대사를 외쳐요. 나중에 손님 상태에 따라서 말하는 대사를 변경해야합니다.
            var line = consumerScriptableObject.GetDialogueFromState(state);
            print($"손님{gameObject.name}: {string.Join(", ", line)}");

            yield return null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class IngredientInfo
{
    public IngredientScriptableObject Ingredient;
    public int Index;
    public bool IsCorrect;

    public IngredientInfo(IngredientScriptableObject ingredient, int index, bool isCorrect)
    {
        Ingredient = ingredient;
        Index = index;
        IsCorrect = isCorrect;
    }
}

public class ConsumerIngredientHandler : MonoBehaviour
{
    private ConsumerUI consumerUI;

    /// <summary>
    /// 주문한 (결제한) 재료 목록
    /// </summary>
    private List<IngredientScriptableObject> paidIngredients = new List<IngredientScriptableObject>();
    /// <summary>
    /// 주문하지 (결제하지 않은) 않은 재료 목록
    /// </summary>
    private List<IngredientScriptableObject> unpaidIngredients = new List<IngredientScriptableObject>();
    /// <summary>
    /// 실제로 가져오려는 재료 목록. 가져왔다면 목록에서 삭제됩니다.
    /// </summary>
    public List<IngredientInfo> AttemptIngredients = new();
    /// <summary>
    /// 현재 갖고 있는 재료 목록
    /// </summary>
    public List<IngredientInfo> OwnedIngredients = new();

    public bool IsIngredientSelectDone => 
        OwnedIngredients.Count >= IngredientManager.MAX_INGREDIENT_NUMBER
        && AttemptIngredients.Count == 0;

    public void Initialize()
    {
        consumerUI = gameObject.GetOrAddComponent<ConsumerUI>();
        consumerUI.TransferClickEvent += UpdateCorrectOwnIngredient; // 재료를 클릭하면 isCorrect를 true로 전환해주는 이벤트
    }

    public void ResetAllIngredientLists()
    {
        paidIngredients.Clear();
        unpaidIngredients.Clear();
        AttemptIngredients.Clear();
        OwnedIngredients.Clear();
    }

    public void SetAllIngredientLists(List<IngredientScriptableObject> ingredientList = null)
    {
        // 재료를 고르고 필요한 재료의 리스트와 필요한 재료의 총 갯수를 구합니다.
        if (ingredientList == null)
        {
            paidIngredients = IngredientManager.Instance.GetPaidIngredients(IngredientManager.MAX_INGREDIENT_NUMBER);
        }
        else
        {
            paidIngredients = new List<IngredientScriptableObject>(ingredientList);
            Debug.Log($"레시피 손님이 고른 재료 : {string.Join(", ", ingredientList)}");
        }

        // 필요한 재료들을 머리 위에 아이콘으로 표시합니다.
        consumerUI.InitializeIngredintUI(paidIngredients);

        // 필요하지 않은 재료의 리스트를 구합니다.
        unpaidIngredients = GetFilteredIngredients(paidIngredients, unpaidIngredients);
    }

    private List<IngredientScriptableObject> GetFilteredIngredients(
        List<IngredientScriptableObject> standardList,
        List<IngredientScriptableObject> separatedList,
        List<IngredientScriptableObject> initialList = null)
    {
        if (initialList == null) initialList = IngredientManager.Instance.IngredientScriptableObject;

        foreach (IngredientScriptableObject ingredient in initialList)
        {
            if (!standardList.Contains(ingredient))
            {
                separatedList.Add(ingredient);
            }
        }
        return separatedList;
    }

    /// <summary>
    /// 입장과 동시에 가져올 재료를 미리 선정해놓습니다.
    /// </summary>
    public void ChooseAllIngredients()
    {
        // 재료를 가져올 순서를 섞습니다.
        var orderList = new List<int> { 0, 1, 2, 3 };
        orderList = IngredientManager.Instance.ShufflePartOfList(orderList, orderList.Count);

        // 먼저 재료를 모두 골라놓습니다.
        for (int i = 0; i < IngredientManager.MAX_INGREDIENT_NUMBER; i++)
        {
            var currentIndex = orderList[i];
            GetAttemptIngredient(currentIndex, out var ingredient, out var isCorrect);
            // 이후에 재료를 얻었을 때를 대비해서 미리 저장해둡니다.
            AttemptIngredients.Add(new IngredientInfo(ingredient, currentIndex, isCorrect));
        }
    }

    public IngredientInfo GetAttemptIngredientInfo()
    {
        if (AttemptIngredients.Count <= 0) throw new System.Exception("AttemptIngredients 리스트가 비어있습니다.");
        var ingredientInfo = AttemptIngredients[AttemptIngredients.Count - 1]; // 제거하지 않고 반환만 합니다. 언제 issue단계가 올지 모르기 때문에
        return ingredientInfo;
    }

    public void AddAttemptIngredients(IngredientScriptableObject ingredient, out bool isNoDuplicate)
    {
        bool isCorrect = paidIngredients.Contains(ingredient);
        IngredientInfo info = new IngredientInfo(ingredient, -1, isCorrect);
        for (int i = 0; i < paidIngredients.Count; i++)
        {
            if (ingredient.Equals(paidIngredients[i]))
            {
                info.Index = i;
            }
        }

        if (AttemptIngredients.Any(info => info.Ingredient == ingredient))
        {
            Debug.Log("중복된 재료입니다!");

            ConsumerSpeech consumerSpeech = gameObject.GetComponent<ConsumerSpeech>();
            if (consumerSpeech == null)
            {
                Debug.LogWarning("ConsumerSpeech 를 찾을 수 없습니다");
            }

            StartCoroutine(consumerSpeech.StartSpeechFromSituation(gameObject.GetComponent<Consumer>().currentConsumerScriptableObject, ConsumerSituation.DuplicateIngredientDetected, true, false, true, false));
            isNoDuplicate = false;

            return;
        }

        AttemptIngredients.Add(info); 
        Debug.Log($"{ingredient.Name} 재료가 attemptedIngredients 리스트에 추가되었습니다. 몇 번째? {info.Index}, 맞는 재료? {isCorrect}");
        isNoDuplicate = true;
    }

    public void AddOwnIngredient(IngredientScriptableObject ingredient, int index, bool isCorrect)
    {
        // 필요재료 리스트에서 뺍니다
        AttemptIngredients.RemoveAt(AttemptIngredients.Count - 1);
        // 얻은재료 리스트에 새로 가져온 재료를 추가합니다.
        OwnedIngredients.Add(new IngredientInfo(ingredient, index, isCorrect));
        Debug.Log($"가지고 있는 재료: {string.Join(", ", ingredient.Name)}");
    }

    public void UpdateCorrectOwnIngredient(int index)
    {
        if(index < 0)
        {
            gameObject.GetComponent<Consumer>().SetState(ConsumerState.IssueUnsolved);
        }

        // 같은 인덱스를 찾아서 재료 맞게 처리
        foreach (var ownedIngredient in OwnedIngredients)
        {
            if(ownedIngredient.Index == index)
            {
                ownedIngredient.IsCorrect = true;
                Debug.Log($"[{ownedIngredient.Ingredient.Name}] 클릭 완료. 이제 최종가격에 포함됩니다.");
            }
        }
    }

    private void GetAttemptIngredient(int index, out IngredientScriptableObject ingredient, out bool isCorrect)
    {
        // 확률에 따라 가져오는 재료가 다릅니다.
        int probability = Random.Range(0, 10);

        if (probability < IngredientManager.CORRECT_INGREDIENT_PROBAILITY)
        {
            ingredient = paidIngredients[index];
            isCorrect = true;
            Debug.Log($"올바른 재료! : {ingredient.Name}");
        }
        else
        {
            int randomIndex = GetRandomIndex(unpaidIngredients);
            ingredient = unpaidIngredients[randomIndex];
            isCorrect = false;
            Debug.Log($"틀린 재료! : {ingredient.Name}");
        }
    }

    private int GetRandomIndex(List<IngredientScriptableObject> ingredientList)
    {
        return Random.Range(0, ingredientList.Count - 1);
    }

    public void RemoveWrongIngredient(IngredientScriptableObject ingredient)
    {
        foreach(IngredientInfo info in AttemptIngredients)
        {
            if(info.Ingredient == ingredient)
            {
                AttemptIngredients.Remove(info);
            }
        }

        foreach(IngredientInfo info in OwnedIngredients)
        {
            if(info.Ingredient == ingredient)
            {
                OwnedIngredients.Remove(info);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PotUIController : MonoBehaviour
{
    [SerializeField] private GameObject pot;
    [SerializeField] private GameObject pointer;
    [SerializeField] private GameObject instruction;
    [SerializeField] private SpriteRenderer submitBtn;
    [SerializeField] private TMP_Text recipeNameTagText;
    [SerializeField] private TMP_Text submitText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private List<SpriteRenderer> ingredientsInPot;

    private Vector2 pot_originalPos = new Vector2(0, 0);
    [SerializeField] private Animation potAnim;
    private string slideIn = "pot_slideIn";
    private string slideOut = "pot_slideOut";
    private string shake = "pot_shake";
    private string drop = "pot_drop";
    [SerializeField] private Animation pointerAnim;
    private string pointerIn = "pointer_in";
    private string pointerOut = "pointer_out";
    [SerializeField] private Animation submitBtnAnim;
    private Vector2 btn_originalSize = new Vector3(1, 1, 1);
    private Color btn_originalColor;
    private Coroutine pointerCoroutine;
    [SerializeField] private int textSortingOrder = 17;
    private string dragInInstruction = "재료를\n드래그해서\n라면을\n완성해요!\n";
    private string dragOutInstruction = "틀린 재료는\n뺄 수\n있어요!";

    private Queue<IEnumerator> potQueue = new Queue<IEnumerator>();
    private bool isPotQueueRunning = false;

    private bool isFirstTimeIn = false;
    public bool IsFirstTimeIn
    {
        get => isFirstTimeIn;
        set
        {
            if (isFirstTimeIn == value) return;

            isFirstTimeIn = value;

            if(isFirstTimeIn)
            {
                OnPlayHint?.Invoke(true);
            }
        }
    }
    private bool isFirstTimeWrong = false;
    public bool IsFirstTimeWrong
    {
        get => isFirstTimeWrong;
        set
        {
            if (isFirstTimeWrong == value) return;

            isFirstTimeWrong = value;

            if(isFirstTimeWrong)
            {
                OnPlayHint?.Invoke(false);
            }
        }
    }

    public event Action<bool> OnPlayHint;

    public void Initialize()
    {
        MeshRenderer nameTagTextMeshRenderer = recipeNameTagText.GetComponent<MeshRenderer>();
        if(nameTagTextMeshRenderer == null)
        {
            Debug.LogError($"{nameof(nameTagTextMeshRenderer)} 를 찾을 수 없습니다.");
        }
        nameTagTextMeshRenderer.sortingOrder = textSortingOrder;
        MeshRenderer submitMeshRenderer = submitText.GetComponent<MeshRenderer>();
        if (submitMeshRenderer == null)
        {
            Debug.LogError($"{nameof(submitMeshRenderer)} 를 찾을 수 없습니다.");
        }
        submitMeshRenderer.sortingOrder = textSortingOrder;
        MeshRenderer instructionMeshRenderer = instructionText.GetComponent<MeshRenderer>();
        if (instructionMeshRenderer == null)
        {
            Debug.LogError($"{nameof(instructionMeshRenderer)} 를 찾을 수 없습니다.");
        }
        instructionMeshRenderer.sortingOrder = textSortingOrder;

        instruction.SetActive(false);

        submitBtn.gameObject.SetActive(true);
        btn_originalColor = submitBtn.color;

        OnPlayHint += ShowPotHint;
        IngredientManager.Instance.OnBringFirstIngredient += StopHint;
        IngredientManager.Instance.OnTakeOutFirstWrongIngredient += StopHint;
    }

    public void EnqueuePotRoutine(IEnumerator routine)
    {
        potQueue.Enqueue(routine);
        if (!isPotQueueRunning)
            StartCoroutine(ProcessPotQueue());
    }

    private IEnumerator ProcessPotQueue()
    {
        isPotQueueRunning = true;
        while (potQueue.Count > 0)
        {
            yield return StartCoroutine(potQueue.Dequeue());
        }
        isPotQueueRunning = false;
    }

    public IEnumerator BringPot()
    {
        pot.transform.position = pot_originalPos;
        pot.SetActive(true);
        SetRecipeName(IngredientManager.Instance.CurrentRecipeConsumer.MyRecipe.Name);
        potAnim.Play(slideIn);

        yield return new WaitUntil(() => !potAnim.IsPlaying(slideIn));

        if (!IsFirstTimeIn) IsFirstTimeIn = true;
    }

    public IEnumerator RemovePot(bool IsPlaySlideOutAnim)
    {
        if (IsPlaySlideOutAnim)
        {
            potAnim.Play(slideOut);
            yield return new WaitUntil(() => !potAnim.IsPlaying(slideOut));
        }
        else
        {
            potAnim.Play(drop);
            yield return new WaitUntil(() => !potAnim.IsPlaying(drop));
        }

        foreach (SpriteRenderer spriteRenderer in ingredientsInPot)
        {
            spriteRenderer.gameObject.SetActive(false);
        }

        if (pointerCoroutine != null) StopCoroutine(pointerCoroutine);
        instruction.SetActive(false);
        pointer.SetActive(false);
        pot.SetActive(false);
    }

    public void ShowIngredientInPot(int index, IngredientScriptableObject ingredientScriptableObject)
    {
        if (index < 0 || index >= ingredientsInPot.Count) return;

        ingredientsInPot[index].sprite = ingredientScriptableObject.Icon;
        ingredientsInPot[index].gameObject.SetActive(true);
        // 콜라이더 추가
        var collider = ingredientsInPot[index].gameObject.GetComponent<PolygonCollider2D>();
        if (collider == null)
        {
            collider = ingredientsInPot[index].gameObject.AddComponent<PolygonCollider2D>();
            collider.autoTiling = true;
            collider.isTrigger = true; // 충돌되지 않도록 trigger on
        }
    }

    public void SetIngredientInPotInactive(int index)
    {
        // 냄비 속 재료 안 보이게 없애기
        ingredientsInPot[index].gameObject.SetActive(false);
    }

    private void SetRecipeName(string name)
    {
        recipeNameTagText.text = name;
    }

    private void ShowPotHint(bool isToPot)
    {
        if(isToPot)
        {
            if(pointerCoroutine != null) StopCoroutine(pointerCoroutine);
            pointerCoroutine = StartCoroutine(PlayPointerAnim(pointerIn));
            instructionText.text = dragInInstruction;
        }
        else
        {
            if (pointerCoroutine != null) StopCoroutine(pointerCoroutine);
            pointerCoroutine = StartCoroutine(PlayPointerAnim(pointerOut));
            instructionText.text = dragOutInstruction;
        }

        instruction.SetActive(true);
    }

    private IEnumerator PlayPointerAnim(string name)
    {
        pointer.SetActive(true);
        pointerAnim.Play(name);
        yield return new WaitUntil(() => !pointerAnim.IsPlaying(name));
        pointer.SetActive(false);
    }

    public void StopHint()
    {
        pointerAnim.Stop();
        instruction.SetActive(false);
    }

    public void PlaySubmitAnim()
    {
        submitBtnAnim.Play();
    }

    public void StopSubmitAnim()
    {
        submitBtnAnim.Stop();
        submitBtn.transform.localScale = btn_originalSize;
        submitBtn.color = btn_originalColor;
    }

    public void PlayShakeAnim()
    {
        potAnim.Play(shake);
    }

    public void StopShakeAnim()
    {
        if(potAnim.IsPlaying(shake))
        {
            potAnim.Stop();
        }
        pot.transform.rotation = Quaternion.identity;
    }

    public void OnGameEnd()
    {
        isFirstTimeIn = false;
        isFirstTimeWrong = false;
    }
}

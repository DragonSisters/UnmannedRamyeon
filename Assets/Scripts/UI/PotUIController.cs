using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PotUIController : MonoBehaviour
{
    [SerializeField] private GameObject pot;
    [SerializeField] private TMP_Text recipeNameTagText;
    [SerializeField] private List<SpriteRenderer> ingredientsInPot;

    private Vector2 pot_originalPos = new Vector2(0, 0);
    [SerializeField] private Animation potAnim;
    private string slideIn = "pot_slideIn";
    private string slideOut = "pot_slideOut";
    private float potAnimTime = 0.5f;
    [SerializeField] private int nameTagSortingOrder = 17;

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
                PlayHint?.Invoke(true);
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
                PlayHint?.Invoke(false);
            }
        }
    }

    public event Action<bool> PlayHint;

    public void Initialize()
    {
        MeshRenderer nameTagTextMeshRenderer = recipeNameTagText.GetComponent<MeshRenderer>();
        nameTagTextMeshRenderer.sortingOrder = nameTagSortingOrder;

        PlayHint += ShowPotHint;
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

        yield return new WaitForSeconds(potAnimTime);
    }

    public IEnumerator RemovePot()
    {
        potAnim.Play(slideOut);
        yield return new WaitUntil(() => !potAnim.IsPlaying(slideOut));

        foreach (SpriteRenderer spriteRenderer in ingredientsInPot)
        {
            spriteRenderer.gameObject.SetActive(false);
        }

        pot.SetActive(false);
    }

    private void SetRecipeName(string name)
    {
        recipeNameTagText.text = name;
    }

    private void ShowPotHint(bool isToPot)
    {
        if(isToPot)
        {
            Debug.LogError("냄비에 넣어!");
        }
        else
        {
            Debug.LogError("냄비에서 빼!");
        }
    }

    public void OnGameEnd()
    {
        isFirstTimeIn = false;
        isFirstTimeWrong = false;
    }
}

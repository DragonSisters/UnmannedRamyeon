using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;

public class IngredientDrag : MonoBehaviour, IDraggableSprite
{
    public bool IsDraggable => isDraggable;
    private bool isDraggable = false;
    private DragMode currentMode = DragMode.None;
    private enum DragMode { None, Add, Remove }

    private IngredientScriptableObject ingredientScriptableObject;
    private SpriteRenderer spriteRenderer;
    private PotUIController uiController;

    [SerializeField] private Material material;
    private readonly Color clickableColor = Color.white;
    private readonly Color answerColor = Color.green;
    private readonly Color wrongColor = Color.red;
    private readonly float outlineWidth = 4f;

    [SerializeField] private CapsuleCollider2D potCollider;
    private List<SpriteRenderer> spritesInPot = new List<SpriteRenderer>();
    private List<PolygonCollider2D> ingredientsInPotCollider = new List<PolygonCollider2D>();
    private int currPickNumIdx
    {
        get
        {
            RecipeConsumer recipeConsumer = IngredientManager.Instance.CurrentRecipeConsumer;
            if (recipeConsumer != null)
            {
                return recipeConsumer.CurrPickCount;
            }
            else
            {
                return -1;
            }
        }
    }
    private int chosenIndex = -1;

    public void Initialize(IngredientScriptableObject ingredient, CapsuleCollider2D potRectTransform, List<SpriteRenderer> ingredients)
    {
        uiController = IngredientManager.Instance.PotUIController;
        ingredientScriptableObject = ingredient;
        potCollider = potRectTransform;
        spritesInPot.Clear();
        spritesInPot = ingredients;
        ingredientsInPotCollider.Clear();
        foreach(SpriteRenderer spriteRenderer in spritesInPot)
        {
            ingredientsInPotCollider.Add(spriteRenderer.gameObject.GetComponent<PolygonCollider2D>());
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.LogError($"재료 프리팹에 SpriteRenderer가 없습니다. 원본 프리팹을 확인해주세요.");
        }

        material = spriteRenderer.material;
        foreach(SpriteRenderer spriteRenderer in spritesInPot)
        {
            spriteRenderer.material = new Material(material);
        }

        IngredientManager.Instance.OnIngredientDeselectMode += OnConsumerHelpFinished;

        ShaderEffectHelper.SetOutlineWidth(material, outlineWidth);
        ShaderEffectHelper.SetOutlineColor(material, clickableColor);
        ShaderEffectHelper.SetOutlineEnable(material, false);
    }

    public void SetDraggable(bool isDraggable)
    {
        this.isDraggable = isDraggable;
        if (isDraggable)
        {
            ShaderEffectHelper.SetOutlineEnable(material, true);
        }
        else
        {
            ShaderEffectHelper.SetOutlineEnable(material, false);
        }
    }

    public void OnSpriteDown()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);
        StartCoroutine(ShaderEffectHelper.SpringAnimation(material));
        SetCursor();

        if (IsPointerOverPot(Input.mousePosition))
        {
            // 냄비에서 시작 : 빼기 준비
            currentMode = DragMode.Remove;
            chosenIndex = GetPickedIngredientIndex(Input.mousePosition);
        }
        else
        {
            // 재료에서 시작 : 더하기 준비
            currentMode = DragMode.Add;
        }
    }

    public void OnSpriteDragging()
    {
    }

    public void OnSpriteUp()
    {
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);

        ResetCursor();

        if (IsPointerOverPot(Input.mousePosition))
        {
            if (currentMode == DragMode.Add)
            {
                IngredientManager.Instance.SendIngredientToCorrectConsumer(ingredientScriptableObject, out bool isNoDuplicate);
                if (!isNoDuplicate)
                {
                    // 중복이면 UI 갱신 X
                    currentMode = DragMode.None;
                    return;
                }

                int index = GetNextAvailableSlotIndex();
                spritesInPot[index].gameObject.GetComponent<IngredientDrag>().SetIngredientScriptableObject(ingredientScriptableObject);
                AddIngredientToPot(index);

                if (IsCorrectIngredient())
                {
                    ShaderEffectHelper.SetOutlineColor(material, answerColor);
                    ShaderEffectHelper.SetOutlineColor(spritesInPot[index].material, answerColor);
                }
                else
                {
                    if (!uiController.IsFirstTimeWrong) uiController.IsFirstTimeWrong = true;
                    ShaderEffectHelper.SetOutlineColor(material, wrongColor);
                    ShaderEffectHelper.SetOutlineColor(spritesInPot[index].material, wrongColor);
                }
            }
        }
        else if (currentMode == DragMode.Remove)
        {
            RemoveIngredientFromPot(chosenIndex);
        }

        if (currPickNumIdx >= 4)
        {
            ShaderEffectHelper.SetOutlineColor(material, clickableColor);
        }

        currentMode = DragMode.None;
    }

    private void AddIngredientToPot(int index)
    {
        if (index < 0 || index >= spritesInPot.Count) return;

        IngredientManager.Instance.CurrentRecipeConsumer.AddIngredientsInPot(index, ingredientScriptableObject);

        spritesInPot[index].sprite = ingredientScriptableObject.Icon;
        spritesInPot[index].gameObject.SetActive(true);
        // 콜라이더 추가
        var collider = spritesInPot[index].gameObject.GetComponent<PolygonCollider2D>();
        if (collider == null)
        {
            collider = spritesInPot[index].gameObject.AddComponent<PolygonCollider2D>();
            collider.autoTiling = true;
            collider.isTrigger = true; // 충돌되지 않도록 trigger on
        }
    }

    private void RemoveIngredientFromPot(int index)
    {
        if (index < 0 || index >= spritesInPot.Count) return;

        // 냄비 속 재료 안 보이게 없애기
        spritesInPot[index].gameObject.SetActive(false);

        // 냄비 속에 더 이상 이 재료가 없다
        RecipeConsumer recipeConsumer = IngredientManager.Instance.CurrentRecipeConsumer;
        if(recipeConsumer != null) recipeConsumer.RemoveIngredientsInPot(index);

        // 가지고 올 재료 리스트와 가지고 있는 재료 리스트에서 제거
        IngredientManager.Instance.RemoveIngredientFromCorrectCunsumer(ingredientScriptableObject);
    }

    private int GetNextAvailableSlotIndex()
    {
        for (int i = 0; i < spritesInPot.Count; i++)
        {
            if (!spritesInPot[i].gameObject.activeSelf) // 비어있는 슬롯
                return i;
        }
        return -1; // 꽉 찼을 때
    }

    private void SetCursor()
    {
        GameManager.Instance.SetCursor(ingredientScriptableObject.CursorIcon);
    }

    private void ResetCursor()
    {
        GameManager.Instance.ResetCursor();
    }

    private bool IsPointerOverPot(Vector2 screenPoint)
    {
        if (potCollider == null) return false;

        Vector2 worldPoint = InputUtility.ScreenToWorldPoint(screenPoint);

        return potCollider.OverlapPoint(worldPoint);
    }


    private int GetPickedIngredientIndex(Vector2 screenPoint)
    {
        Vector2 worldPoint = InputUtility.ScreenToWorldPoint(screenPoint);
        int index = -1;
        for(int i = 0; i < ingredientsInPotCollider.Count; i++)
        {
            if (ingredientsInPotCollider[i].OverlapPoint(worldPoint))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void SetIngredientScriptableObject(IngredientScriptableObject ingredient)
    {
        ingredientScriptableObject = ingredient;
    }

    private bool IsCorrectIngredient()
    {
        return IngredientManager.Instance.IsCorrectIngredient(ingredientScriptableObject);
    }

    // 손님 다 도와줬을 때
    private void OnConsumerHelpFinished()
    {
        ShaderEffectHelper.SetOutlineColor(material, clickableColor);
    }
}

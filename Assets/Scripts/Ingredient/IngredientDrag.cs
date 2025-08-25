using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IngredientDrag : MonoBehaviour, IDraggableSprite
{
    public bool IsDraggable => isDraggable;
    private bool isDraggable = false;
    private DragMode currentMode = DragMode.None;
    private enum DragMode { None, Add, Remove }

    private IngredientScriptableObject ingredientScriptableObject;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Material material;
    private readonly Color clickableColor = Color.white;
    private readonly Color answerColor = Color.green;
    private readonly Color wrongColor = Color.red;
    private readonly float outlineWidth = 4f;

    [SerializeField] private CapsuleCollider2D potCollider;
    private List<SpriteRenderer> ingredientsInPot = new List<SpriteRenderer>();
    private List<PolygonCollider2D> ingredientsInPotCollider = new List<PolygonCollider2D>();
    private int currPickNumIdx
    {
        get
        {
            return IngredientManager.Instance.CurrentRecipeConsumer.CurrPickCount;
        }
    }

    public void Initialize(IngredientScriptableObject ingredient, CapsuleCollider2D potRectTransform, List<SpriteRenderer> ingredients)
    {
        ingredientScriptableObject = ingredient;
        potCollider = potRectTransform;
        ingredientsInPot.Clear();
        ingredientsInPot = ingredients;
        ingredientsInPotCollider.Clear();
        foreach(SpriteRenderer spriteRenderer in ingredientsInPot)
        {
            ingredientsInPotCollider.Add(spriteRenderer.gameObject.GetComponent<PolygonCollider2D>());
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.LogError($"재료 프리팹에 SpriteRenderer가 없습니다. 원본 프리팹을 확인해주세요.");
        }

        material = spriteRenderer.material;
        foreach(SpriteRenderer spriteRenderer in ingredientsInPot)
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
        Debug.LogError($"localPoint = {Input.mousePosition}, potRect = {potCollider.transform}");
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Click);
        StartCoroutine(ShaderEffectHelper.SpringAnimation(material));
        SetCursor();

        if (IsPointerOverPot(Input.mousePosition))
        {
            Debug.LogError("빼기 준비");
            // 냄비에서 시작 : 빼기 준비
            currentMode = DragMode.Remove;
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
            Debug.LogError("여기");
            if (currentMode == DragMode.Add)
            {
                AddIngredientToPot(currPickNumIdx);
                IngredientManager.Instance.SendIngredientToCorrectConsumer(ingredientScriptableObject);
                if (IsCorrectIngredient())
                {
                    ShaderEffectHelper.SetOutlineColor(material, answerColor);
                    ShaderEffectHelper.SetOutlineColor(ingredientsInPot[currPickNumIdx - 1].material, answerColor);
                }
                else
                {
                    ShaderEffectHelper.SetOutlineColor(material, wrongColor);
                    ShaderEffectHelper.SetOutlineColor(ingredientsInPot[currPickNumIdx - 1].material, wrongColor);
                }
            }
            else if (currentMode == DragMode.Remove)
            {
                Debug.LogError("Remove 모드");
                int index = GetPickedIngredientIndex(Input.mousePosition);
                Debug.LogError(index);
                RemoveIngredientFromPot(index);
            }
        }

        if(currPickNumIdx >= 4)
        {
            ShaderEffectHelper.SetOutlineColor(material, clickableColor);
        }

        currentMode = DragMode.None;
    }

    private void AddIngredientToPot(int ingredientCount)
    {
        ingredientsInPot[ingredientCount].sprite = ingredientScriptableObject.Icon;
        ingredientsInPot[ingredientCount].gameObject.SetActive(true);
        // 콜라이더 추가
        var collider = ingredientsInPot[ingredientCount].gameObject.GetComponent<PolygonCollider2D>();
        if (collider == null)
        {
            collider = ingredientsInPot[ingredientCount].gameObject.AddComponent<PolygonCollider2D>();
            collider.autoTiling = true;
            collider.isTrigger = true; // 충돌되지 않도록 trigger on
        }
    }

    private void RemoveIngredientFromPot(int Index)
    {
        // 냄비 속 재료 안 보이게 없애기
        ingredientsInPot[Index].gameObject.SetActive(false);

        // 가지고 올 재료 리스트와 가지고 있는 재료 리스트에서 제거
        IngredientManager.Instance.RemoveIngredientFromCorrectCunsumer(ingredientScriptableObject);
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

        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);

        return potCollider.OverlapPoint(worldPoint);
    }


    private int GetPickedIngredientIndex(Vector2 screenPoint)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
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

    private bool IsCorrectIngredient()
    {
        return IngredientManager.Instance.IsCorrectIngredient(ingredientScriptableObject);
    }

    // 손님 다 도와줬을 때
    private void OnConsumerHelpFinished()
    {
        ShaderEffectHelper.SetOutlineColor(material, clickableColor);
    }


    // @anditsoon TODO: 
    // 게임 종료될 때 : IngredientInPot.Clear(); 
}

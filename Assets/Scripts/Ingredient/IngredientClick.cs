using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IngredientClick : MonoBehaviour, IDraggableSprite
{
    public bool IsDraggable => isDraggable;
    private bool isDraggable = false;
    private DragMode currentMode = DragMode.None;
    private enum DragMode { None, Add, Remove }

    private IngredientScriptableObject ingredientScriptableObject;
    private SpriteRenderer spriteRenderer;

    private Material material;
    private readonly Color clickableColor = Color.white;
    private readonly Color answerColor = Color.green;
    private readonly Color wrongColor = Color.red;
    private readonly float outlineWidth = 4f;

    [SerializeField] private RectTransform potRect;
    private List<Image> ingredientsInPot = new List<Image>();
    private int currPickNumIdx
    {
        get
        {
            return IngredientManager.Instance.CurrentRecipeConsumer.CurrPickCount;
        }
    }

    public void Initialize(IngredientScriptableObject ingredient, RectTransform potRectTransform, List<Image> ingredients)
    {
        ingredientScriptableObject = ingredient;
        potRect = potRectTransform;
        ingredientsInPot.Clear();
        ingredientsInPot = ingredients;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.LogError($"재료 프리팹에 SpriteRenderer가 없습니다. 원본 프리팹을 확인해주세요.");
        }

        material = spriteRenderer.material;
        foreach(Image image in ingredientsInPot)
        {
            image.material = new Material(material);
        }

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
            ShaderEffectHelper.SetOutlineColor(material, clickableColor);
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

        if (IsPointerOverPot())
        {
            // 냄비에서 시작 → 빼기 준비
            currentMode = DragMode.Remove;
        }
        else
        {
            // 재료에서 시작 → 더하기 준비
            currentMode = DragMode.Add;
        }
    }

    public void OnSpriteDragging()
    {

    }

    public void OnSpriteUp()
    {
        ResetCursor();

        if (IsPointerOverPot())
        {
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
            // @anditsoon TODO:
            //else if (currentMode == DragMode.Remove)
            //{
            //    RemoveIngredientFromPot();
            //}
        }

        currentMode = DragMode.None;
    }

    private void SetCursor()
    {
        GameManager.Instance.SetCursor(ingredientScriptableObject.CursorIcon);
    }

    private void ResetCursor()
    {
        GameManager.Instance.ResetCursor();
    }

    private bool IsPointerOverPot()
    {
        if (potRect == null) return false;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            potRect,
            Input.mousePosition,
            null, // Overlay 모드이므로 카메라 필요 없음
            out localPoint
        );

        return potRect.rect.Contains(localPoint);
    }

    private bool IsCorrectIngredient()
    {
        return IngredientManager.Instance.IsCorrectIngredient(ingredientScriptableObject);
    }

    private void AddIngredientToPot(int ingredientCount)
    {
        ingredientsInPot[ingredientCount].sprite = ingredientScriptableObject.Icon;
        UIEffectControl.Instance.SetAlpha(ingredientsInPot[ingredientCount], 1f);
    }

    // @anditsoon TODO:
    //private void RemoveIngredientFromPot()
    //{
    //    
    //}

    // @anditsoon TODO: 
    // 게임 종료될 때 : IngredientInPot.Clear(); 
}

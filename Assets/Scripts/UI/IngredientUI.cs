using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour, IClickableSprite
{
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;

    [SerializeField] private Image ingredientImage;
    [SerializeField] private Image feedbackImage;

    public bool IsClickable => isClickable;
    private bool isClickable = false;

    private const int CLICK_TOLERANCE = 15;

    public void Initialize()
    {
        // 박스 콜라이더 추가
        var boxCollider = gameObject.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        // 충돌되지 않도록 trigger on
        boxCollider.isTrigger = true;

        // 사이즈 조정
        var rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            throw new System.Exception($"UI는 늘 RectTransform을 가지고 있어야 합니다.");
        }
        var newSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height) 
            + new Vector2(CLICK_TOLERANCE, CLICK_TOLERANCE);
        boxCollider.size = newSize;
    }

    public void OnSpriteClicked()
    {
        Debug.Log($"재료가 클릭되었습니다");
        // @charotiti9 TODO: 재료를 isCorrect = true로 만듭니다

        feedbackImage.sprite = correctSprite;
        isClickable = false;
    }

    public void OnSpriteDeselected() { }

    /// <summary>
    /// 초기에 재료 이미지를 셋팅합니다.
    /// </summary>
    /// <param name="icon"></param>
    public void SetIngredientImage(Sprite icon)
    {
        ingredientImage.sprite = icon;
    }

    public void ActivateFeedbackUI(bool isCorrect)
    {
        feedbackImage.sprite = isCorrect ? correctSprite : wrongSprite;
        isClickable = !isCorrect; // 맞는 재료라면 클릭하지 않게 합니다. 반대로 틀린 재료라면 클릭해야합니다.
        feedbackImage.gameObject.SetActive(true);
    }

    public void DeactivateFeedbackUI()
    {
        feedbackImage.gameObject.SetActive(false);
    }

}

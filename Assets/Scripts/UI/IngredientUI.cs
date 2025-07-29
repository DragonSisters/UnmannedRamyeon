using UnityEngine;
using UnityEngine.UI;

public class IngredientUI : MonoBehaviour, IClickableSprite
{
    [SerializeField] private ConsumerIngredientHandler ingredientHandler;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;

    [SerializeField] private Image ingredientImage;
    [SerializeField] private Image feedbackImage;

    public bool IsClickable => isClickable;
    private bool isClickable = false;

    public void OnSpriteClicked()
    {
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

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class IngredientUITmp : MonoBehaviour
{
    // @anditsoon TODO: 직전 PR 을 보니 IngredientUI.cs 를 따로 둔 것 같던데, 그 PR 이 머지되면 이 스크립트 전체를 해당 스크립트로 옮기겠습니다.

    [SerializeField] private Canvas canvas;

    void Start()
    {
        CreateIngredientImageOnPosition();
    }

    private void CreateIngredientImageOnPosition()
    {
        List<IngredientScriptableObject> ingredientList = IngredientManager.Instance.IngredientScriptableObject;
        foreach (IngredientScriptableObject ingredient in ingredientList)
        {
            GameObject imgObj = new GameObject("IngredientIcon", typeof(Image));
            imgObj.transform.SetParent(canvas.transform, false); // 생성할 때는 로컬 좌표 유지

            Vector3 screenPos = Camera.main.WorldToScreenPoint(ingredient.Point); // 월드좌표 -> 화면 좌표
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform imageRect = imgObj.GetComponent<RectTransform>();
            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out uiPos); // 화면 좌표 -> 로컬 좌표

            imageRect.anchoredPosition = uiPos;
            imgObj.GetComponent<Image>().sprite = ingredient.Icon;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

class ClickCandidate
{
    public GameObject Target;
    public int SortingLayer;
    public int SortingOrder;
    public float Z;
    public bool IsUI;
}
public class SpriteClickHandler : Singleton<SpriteClickHandler>
{
    public IClickableSprite CurrentClickedSprite => currentClickedSprite;
    private IClickableSprite currentClickedSprite;

    public void UpdateHandler()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleSpriteClick();
        }
    }

    public bool IsClickedObject(IClickableSprite sprite)
    {
        return currentClickedSprite == sprite;
    }

    private void HandleSpriteClick()
    {
        var candidates = new List<ClickCandidate>();

        // 1. UI 수집
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var uiResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, uiResults);

        foreach (var ui in uiResults)
        {
            var canvas = ui.gameObject.GetComponentInParent<Canvas>();
            if (canvas == null) continue;

            candidates.Add(new ClickCandidate
            {
                Target = ui.gameObject,
                SortingLayer = SortingLayer.GetLayerValueFromID(canvas.sortingLayerID),
                SortingOrder = canvas.sortingOrder,
                Z = ui.gameObject.transform.position.z,
                IsUI = true
            });
        }

        // 2. Sprite 수집
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        foreach (var hit in hits)
        {
            var sr = hit.collider.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            candidates.Add(new ClickCandidate
            {
                Target = hit.collider.gameObject,
                SortingLayer = SortingLayer.GetLayerValueFromID(sr.sortingLayerID),
                SortingOrder = sr.sortingOrder,
                Z = sr.transform.position.z,
                IsUI = false
            });
        }

        if (candidates.Count == 0)
            return;

        // 3. 공통된 기준으로 정렬
        candidates.Sort(CompareCandidates);

        // 4. 최상위 오브젝트 클릭 처리
        var top = candidates[0];
        var clickable = top.Target.GetComponent<IClickableSprite>();
        if (clickable != null && clickable.IsClickable)
        {
            clickable.OnSpriteClicked();
        }

    }

    private int CompareCandidates(ClickCandidate a, ClickCandidate b)
    {
        // 1. SortingLayer
        if (a.SortingLayer != b.SortingLayer)
            return b.SortingLayer.CompareTo(a.SortingLayer);

        // 2. SortingOrder
        if (a.SortingOrder != b.SortingOrder)
            return b.SortingOrder.CompareTo(a.SortingOrder);

        // 3. Z (앞쪽이 더 작음)
        return a.Z.CompareTo(b.Z);
    }
}
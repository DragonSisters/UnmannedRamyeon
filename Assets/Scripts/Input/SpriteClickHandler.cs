using System.Collections.Generic;
using UnityEngine;

public class SpriteClickHandler : Singleton<SpriteClickHandler>
{
    public void UpdateHandler()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleSpriteClick();
        }
    }

    private void HandleSpriteClick()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        if (hits.Length == 0)
        {
            return;
        }

        // 2D 렌더링 순서에 맞게 정렬 (앞쪽이 먼저)
        System.Array.Sort(hits, CompareSpriteRenderOrder);

        // IClickableSprite 인터페이스를 구현한 오브젝트들만 필터링
        var clickableSprites = new List<IClickableSprite>();

        foreach (var hit in hits)
        {
            IClickableSprite clickable = hit.collider.GetComponent<IClickableSprite>();
            if (clickable != null)
            {
                clickableSprites.Add(clickable);
            }
        }

        // 클릭할 수 있을 때만 클릭처리합니다.
        if (clickableSprites.Count > 0 && clickableSprites[0].IsClickable)
        {
            clickableSprites[0].OnSpriteClicked();
        }
    }

    int CompareSpriteRenderOrder(RaycastHit2D hit1, RaycastHit2D hit2)
    {
        var sr1 = hit1.collider.GetComponent<SpriteRenderer>();
        var sr2 = hit2.collider.GetComponent<SpriteRenderer>();

        // SpriteRenderer가 없으면 뒤쪽으로 보냄
        if (sr1 == null && sr2 == null)
        {
            return 0;
        }
        if (sr1 == null)
        {
            return 1;
        }
        if (sr2 == null)
        {
            return -1;
        }

        // 1. SortingLayer 비교 (높은 ID가 앞쪽)
        int layer1 = SortingLayer.GetLayerValueFromID(sr1.sortingLayerID);
        int layer2 = SortingLayer.GetLayerValueFromID(sr2.sortingLayerID);

        if (layer1 != layer2)
        {
            return layer2.CompareTo(layer1); // 높은 레이어가 앞쪽
        }

        // 2. 같은 레이어면 SortingOrder 비교 (높은 값이 앞쪽)
        if (sr1.sortingOrder != sr2.sortingOrder)
        {
            return sr2.sortingOrder.CompareTo(sr1.sortingOrder);
        }

        // 3. 같은 sortingOrder면 Y 위치로 비교 (높은 Y가 뒤쪽, 2D 기본 규칙)
        if (Mathf.Abs(sr1.transform.position.y - sr2.transform.position.y) > 0.01f)
        {
            return sr1.transform.position.y.CompareTo(sr2.transform.position.y);
        }

        // 4. 마지막으로 Z 위치로 비교 (혹시 모를 경우)
        return sr2.transform.position.z.CompareTo(sr1.transform.position.z);
    }
}
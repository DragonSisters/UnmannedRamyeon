using UnityEngine;

public static class InputUtility
{
    private static Camera mainCamera;

    // 메인카메라 계속 찾지 않도록 캐싱
    public static Camera MainCamera
    {
        get
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            return mainCamera;
        }
    }

    public static Vector2 ScreenToWorldPoint(Vector3 screenPosition)
    {
        var worldPos = MainCamera.ScreenToWorldPoint(screenPosition);
        return new Vector2(worldPos.x, worldPos.y);
    }

    public static Vector2 WorldToScreenPoint(Vector3 worldPosition)
    {
        var screenPos = MainCamera.WorldToScreenPoint(worldPosition);
        return new Vector2(screenPos.x, screenPos.y);
    }

    /// <summary>
    /// 클릭된 후보들을 비교하는 함수
    /// </summary>
    public static int CompareCandidates(ClickCandidate a, ClickCandidate b)
    {
        // SortingLayer
        if (a.SortingLayer != b.SortingLayer)
            return b.SortingLayer.CompareTo(a.SortingLayer);

        // SortingOrder
        if (a.SortingOrder != b.SortingOrder)
            return b.SortingOrder.CompareTo(a.SortingOrder);

        // Z비교 (앞쪽이 더 작음)
        return a.Z.CompareTo(b.Z);
    }
}

using UnityEngine;

/// <summary>
/// UI와 스프라이트를 둘 다 담을 수 있는 후보 클래스
/// </summary>
public class ClickCandidate
{
    public GameObject Target;
    public int SortingLayer;
    public int SortingOrder;
    public float Z;
    public bool IsUI;

    public void Initialize()
    {
        Target = null;
        SortingLayer = 0;
        SortingOrder = 0;
        Z = 0;
        IsUI = false;
    }
}

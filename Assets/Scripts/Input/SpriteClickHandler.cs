using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpriteClickHandler : Singleton<SpriteClickHandler>
{
    public IClickableSprite CurrentClickedSprite => currentClickedSprite;
    private IClickableSprite currentClickedSprite;

    private List<ClickCandidate> candidatesList;
    private Stack<ClickCandidate> candidatePool;
    private List<ClickCandidate> activeCandidates;

    private readonly int poolSize = 20;

    public delegate void SpriteClickedEvent(IClickableSprite sprite);
    public event SpriteClickedEvent OnSpriteClicked;

    protected override void Awake()
    {
        base.Awake();

        // 스택 기반 풀 초기화
        candidatesList = new List<ClickCandidate>(poolSize);
        candidatePool = new Stack<ClickCandidate>(poolSize);
        activeCandidates = new List<ClickCandidate>(poolSize);

        // 풀 미리 채우기
        for (int i = 0; i < poolSize; i++)
        {
            var candidate = new ClickCandidate();
            candidatePool.Push(candidate);
        }
    }

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

    // 스택에서 후보 가져오기
    private ClickCandidate GetCandidate()
    {
        ClickCandidate candidate;

        if (candidatePool.Count > 0)
        {
            candidate = candidatePool.Pop();
        }
        else
        {
            // 풀이 비었으면 새로 생성
            candidate = new ClickCandidate();
        }

        activeCandidates.Add(candidate);
        return candidate;
    }

    // 후보를 스택으로 반환
    private void ReturnCandidate(ClickCandidate candidate)
    {
        if (activeCandidates.Remove(candidate))
        {
            candidate.Initialize();
            candidatePool.Push(candidate);
        }
    }

    private void HandleSpriteClick()
    {
        candidatesList.Clear();

        // InputUtility 클래스를 수정된 함수로 호출
        CollectUIElements(out var uiCandidatesList);
        CollectSpriteElements(out var spriteCandidatesList);

        candidatesList.AddRange(uiCandidatesList);
        candidatesList.AddRange(spriteCandidatesList);

        // 클릭이 없었다면 종료
        if (candidatesList.Count == 0)
        {
            return;
        }

        candidatesList.Sort(InputUtility.CompareCandidates);

        // 가장 앞에 있는 클릭 가능한 스프라이트 찾기
        IClickableSprite selectedClickable = null;
        foreach (var candidate in candidatesList)
        {
            var spriteComponent = candidate.Target.GetComponent<IClickableSprite>();
            if (spriteComponent != null && spriteComponent.IsClickable)
            {
                selectedClickable = spriteComponent;
                break;
            }
        }

        // 클릭 가능한 스프라이트가 있다면 처리
        if (selectedClickable != null)
        {
            // 새로운 스프라이트 선택
            currentClickedSprite = selectedClickable;
            currentClickedSprite.OnSpriteClicked();

            // 이벤트 호출
            OnSpriteClicked?.Invoke(currentClickedSprite);
        }

        // 후보들 풀로 반환
        foreach (var candidate in candidatesList)
        {
            ReturnCandidate(candidate);
        }
    }

    /// <summary>
    /// UI 요소들을 수집합니다.
    /// </summary>
    /// <param name="candidates"></param>
    private void CollectUIElements(out List<ClickCandidate> candidates)
    {
        candidates = new List<ClickCandidate>();
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var uiResults = new List<RaycastResult>(10);
        EventSystem.current.RaycastAll(pointerData, uiResults);

        foreach (var ui in uiResults)
        {
            var canvas = ui.gameObject.GetComponentInParent<Canvas>();
            if (canvas == null) continue;

            var candidate = GetCandidate();
            candidate.Target = ui.gameObject;
            candidate.SortingLayer = SortingLayer.GetLayerValueFromID(canvas.sortingLayerID);
            candidate.SortingOrder = canvas.sortingOrder;
            candidate.Z = ui.gameObject.transform.position.z;
            candidate.IsUI = true;

            candidates.Add(candidate);
        }
    }

    /// <summary>
    /// 스프라이트 요소들을 수집합니다.
    /// </summary>
    /// <param name="candidates"></param>
    private void CollectSpriteElements(out List<ClickCandidate> candidates)
    {
        candidates = new List<ClickCandidate>();
        var mousePos = InputUtility.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            var sr = hit.collider.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            var candidate = GetCandidate();
            candidate.Target = hit.collider.gameObject;
            candidate.SortingLayer = SortingLayer.GetLayerValueFromID(sr.sortingLayerID);
            candidate.SortingOrder = sr.sortingOrder;
            candidate.Z = sr.transform.position.z;
            candidate.IsUI = false;

            candidates.Add(candidate);
        }
    }
}

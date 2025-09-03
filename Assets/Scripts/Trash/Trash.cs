using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour, IPoolable, IDraggableSprite
{
    [SerializeField] private List<Sprite> trashImages = new();

    public bool IsDraggable => isDraggable;
    private bool isDraggable = true;

    private SpriteRenderer spriteRenderer;
    private Color originColor = new Color(1f, 1f, 1f, 1f); // 초기 색상은 흰색, 알파값은 1로 설정합니다.
    private Vector2 colliderSize = new Vector2(1.5f, 1.5f);
    private float thredholdAlpha = 0.1f; // 알파값이 이 값 이하로 떨어지면 파괴됩니다.
    private Texture2D originCursorIcon;
    private Texture2D cleaningCursorIcon;
    private Vector2 mousePrePosition;
    private float mousePositionThredhold = 0.1f; // 마우스가 움직였다고 판단하는 최소 거리
    private float alphaDecreaseAmount = 0.03f; // 알파값 감소량

    private float moodDecreaseDelayTime = 2f; // Trash가 생성되고, 기분이 내려갈때까지 걸리는 딜레이
    public float affectRadius = 2f; // Trash 주변 영향 반경
    public int moodDecraseAmount = 31; // Trash 주변 영향 반경
    private HashSet<Consumer> affectedConsumers = new();
    private Coroutine decreaseMoodCoroutine;



    private void Initiailize()
    {
        //  초기화
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            throw new System.Exception("쓰레기 프리팹에 스프라이트 렌더러가 추가되어있지 않습니다. 확인해주세요");
        }
        spriteRenderer.sprite = trashImages[Random.Range(0, trashImages.Count)];
        spriteRenderer.color = originColor;

        var collider = gameObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = colliderSize;
        }

        originCursorIcon = UIManager.Instance.CursorIcon;
        cleaningCursorIcon = TrashManager.Instance.CleaningCursorIcon;
        affectedConsumers.Clear();
    }

    public void OnSpawn()
    {
        Initiailize();

        decreaseMoodCoroutine = StartCoroutine(DecreaseMood());
    }

    public void OnDespawn()
    {
        // 커서가 다시 손가락으로 바뀐다.
        UIManager.Instance.SetCursor(originCursorIcon);
        // 뽀득뽀득 소리가 멈춘다
        SoundManager.Instance.StopContinousSound(ContinousSoundType.TrashCleaning);
        // 성공 소리를 내준다
        SoundManager.Instance.PlayEffectSound(EffectSoundType.Success);
        
        if(decreaseMoodCoroutine != null)
        {
            StopCoroutine(decreaseMoodCoroutine);
            decreaseMoodCoroutine = null;
        }
    }

    public bool ShouldDespawn()
    {
        // spriteRenderer의 알파값이 0이거나 0과 가깝게 되었을때
        return spriteRenderer.color.a <= thredholdAlpha;
    }

    public void OnSpriteDown()
    {
        // 커서가 행주로 바뀐다.
        UIManager.Instance.SetCursor(cleaningCursorIcon);
    }

    public void OnSpriteDragging()
    {
        if(ShouldDespawn())
        {
            return;
        }

        // 마우스가 움직이고 있다면 알파값을 조절한다
        // 이전 마우스 값과 비교해서 얼만큼 움직였는지 계산하여 distance가 일정이상일 때 움직였다고 판단한다
        Vector2 mousePos = InputUtility.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(mousePrePosition, mousePos);
        if (distance > mousePositionThredhold)
        {
            // 알파값을 감소시킨다. 0보다 작아지지 않게 Clamp
            Color color = spriteRenderer.color;
            color.a = Mathf.Clamp01(color.a - alphaDecreaseAmount);
            spriteRenderer.color = color;

            // 뽀득뽀득 소리가 들린다
            SoundManager.Instance.PlayContinousSound(ContinousSoundType.TrashCleaning);
        }
        mousePrePosition = mousePos;
    }

    public void OnSpriteUp()
    {
        // 커서가 다시 손가락으로 바뀐다.
        UIManager.Instance.SetCursor(originCursorIcon);
        // 뽀득뽀득 소리가 멈춘다
        SoundManager.Instance.StopContinousSound(ContinousSoundType.TrashCleaning);
    }

    private IEnumerator DecreaseMood()
    {
        // 쓰레기가 생겼을 때 바로 기분 깎지 말고 한 1초뒤에 깎게 하기
        yield return new WaitForSeconds(moodDecreaseDelayTime);

        // Trash가 생성된 후 일정 시간마다 주변 Consumer의 기분을 감소시킵니다.
        while (!ShouldDespawn())
        {
            AffectNearbyConsumers();
            yield return null;
        }
    }

    private void AffectNearbyConsumers()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, affectRadius);

        for (int i = 0; i < colliders.Length; i++)
        {
            Consumer consumer = colliders[i].GetComponentInParent<Consumer>();
            if (consumer != null && !affectedConsumers.Contains(consumer))
            {
                ConsumerMood moodScript = consumer.moodScript;
                if (moodScript != null)
                {
                    moodScript.DecreaseMood(moodDecraseAmount);
                    affectedConsumers.Add(consumer); // 이미 영향을 준 Consumer로 등록
                }
            }
        }
    }
}

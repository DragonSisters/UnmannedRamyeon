using Unity.VisualScripting;
using UnityEngine;

public class Trash : MonoBehaviour, IPoolable, IDraggableSprite
{
    public bool IsDraggable => isDraggable;
    private bool isDraggable = true;

    private SpriteRenderer spriteRenderer;
    private Color originColor = new Color(1f, 1f, 1f, 1f); // 초기 색상은 흰색, 알파값은 1로 설정합니다.
    private float thredholdAlpha = 0.1f; // 알파값이 이 값 이하로 떨어지면 파괴됩니다.
    private Texture2D originCursorIcon;
    private Texture2D cleaningCursorIcon;


    public void OnSpawn()
    {
        //  초기화
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            throw new System.Exception("쓰레기 프리팹에 스프라이트 렌더러가 추가되어있지 않습니다. 확인해주세요");
        }
        var collider = gameObject.GetOrAddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        originCursorIcon = GameManager.Instance.CursorIcon;
        cleaningCursorIcon = TrashManager.Instance.CleaningCursorIcon;


        spriteRenderer.color = originColor;
    }

    public void OnDespawn()
    {
        
    }

    public bool ShouldDespawn()
    {
        // spriteRenderer의 알파값이 0이거나 0과 가깝게 되었을때
        return spriteRenderer.color.a <= thredholdAlpha;
    }

    public void OnSpriteDown()
    {
        // 커서가 행주로 바뀐다.
        GameManager.Instance.SetCursor(cleaningCursorIcon);
    }

    public void OnSpriteDragging()
    {
        // 마우스가 움직이고 있다면 알파값을 조절한다
        // 뽀득뽀득 소리가 들린다
        Debug.Log("닦는중~");
    }

    public void OnSpriteUp()
    {
        // 커서가 다시 손가락으로 바뀐다.
        GameManager.Instance.SetCursor(originCursorIcon);
    }

}

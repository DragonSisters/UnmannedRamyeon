public interface IDraggableSprite
{
    /// <summary>
    /// 클릭이 가능한 타이밍에만 True
    /// </summary>
    public bool IsDraggable { get; }
    public void OnSpriteDown();
    public void OnSpriteDragging();
    public void OnSpriteUp();
}

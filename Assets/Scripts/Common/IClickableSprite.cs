public interface IClickableSprite
{
    /// <summary>
    /// 클릭이 가능한 타이밍에만 True
    /// </summary>
    public bool IsClickable { get; }
    /// <summary>
    /// 클릭되면 실행할 로직을 이 안에 넣습니다
    /// </summary>
    public void OnSpriteClicked();
}
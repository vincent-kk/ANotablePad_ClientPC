/// <summary>
/// Scroll 가능한 영역에 만들 아이템에 대한 인터페이스
/// </summary>
interface Item
{
    void SetItemData(string name);
    void TouchEvent();
    void ShowItem();
    void HideItem();
    void DeleteItem();
}

using UnityEngine;
/// <summary>
/// 메뉴 화면을 보여줄 때 사용되는 클래스
/// </summary>
public class MenuView : MonoBehaviour, IView
{
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
    }
}

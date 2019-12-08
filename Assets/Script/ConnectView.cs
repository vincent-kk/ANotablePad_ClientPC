using UnityEngine;
/// <summary>
/// 최초에 서버에 접속을 시도하는 화면에서의 동작을 정의한다.
/// </summary>
public class ConnectView : MonoBehaviour, IView
{
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
    }
}

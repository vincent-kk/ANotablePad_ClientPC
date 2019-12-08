using UnityEngine;

/// <summary>
/// 방을 선택하는 화면에 대한 기능
/// 뷰가 전환될 때 마다 자동으로 방의 리스트를 새로 전송받는다.
/// </summary>
public class SelectView : MonoBehaviour, IView
{
    [SerializeField] private ApplicationManager _applicationManager;
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
        if(show)  _applicationManager.GetRoomList();
    }
}

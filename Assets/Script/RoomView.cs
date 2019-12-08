using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방을 생성하는 View의 기능을 정의한 클래스
/// </summary>
public class RoomView : MonoBehaviour, IView
{
    [SerializeField] private InputField room;
    [SerializeField] private InputField pw;
    [SerializeField] private Button connect;
    [SerializeField] private Text roomName;
    [SerializeField] private Text roomState;
    [SerializeField] private GameObject back;
    [SerializeField] private ApplicationManager _applicationManager;

    private string _roomName;
    private string _roomPassword;

    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
        room.text = "";
        pw.text = "";
        roomName.text = "";
        roomState.text = "";
        connect.interactable = true;
        back.SetActive(true);
    }

    /// <summary>
    /// 방에 대한 정보를 입력받으면 간단한 유효성 검증을 실시하고 방의 생성 정보를 전송한다.
    /// </summary>
    public void TryToCreateRoom()
    {
        _roomName = room.text;
        _roomPassword = pw.text;
        if (_roomName.Length > 0 && _roomPassword.Length > 0)
        {
            if (AppData.RoomNameRegex.IsMatch(_roomName))
            {
                connect.interactable = false;
                _applicationManager.CreateRoom(_roomName, _roomPassword);
                room.text = "";
                pw.text = "";
                return;
            }
        }
        connect.interactable = true;
        _applicationManager.ShowWaringModal("Invalid-Name");
    }

    /// <summary>
    /// 방이 생성되면 Wait상태를 유지하기 위해 사용자의 입력을 제한한다.
    /// </summary>
    /// <param name="createRoom"></param>
    public void ReadyToStartDrawing(string createRoom)
    {
        if (_roomName != createRoom)
        {
            roomState.text = "ROOM CREATE ERROR";
            connect.interactable = true;
        }
        else
        {
            roomName.text = _roomName;
            roomState.text = "Ready To Start";
            back.SetActive(false);
        }
    }
}
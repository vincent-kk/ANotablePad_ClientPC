
using UnityEngine;
using UnityEngine.UI;

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

    public void TryToCreateRoom()
    {
        _roomName = room.text;
        _roomPassword = pw.text;

        room.text = "";
        pw.text = "";
        connect.interactable = false;
        _applicationManager.CreateRoom(_roomName, _roomPassword);
    }

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

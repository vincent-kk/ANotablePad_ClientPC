using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private GameObject[] viewObjects;
    [SerializeField] private Drawable _drawable;
    [SerializeField] private DrawingSettings _drawingSettings;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private ScrollManager _scrollManager;
    [SerializeField] private RoomView _roomView;
    [SerializeField] private WarningOverlayManager _warningOverlayManager;
    [SerializeField] private DrawingCover _drawingCover;

    private IView[] views;

    private readonly Dictionary<string, int> _viewName = new Dictionary<string, int>(5)
    {
        {"connection", 0},
        {"menu", 1},
        {"room", 2},
        {"select", 3},
        {"draw", 4}
    };

    private void Start()
    {
        views = new IView[viewObjects.Length];
        for (int i = 0; i < viewObjects.Length; i++)
            views[i] = viewObjects[i].GetComponent<IView>();
        ChangeView("connection");
    }

    private void Awake()
    {
        Screen.SetResolution(1040, 715, false);
    }

    public void ChangeView(string view)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].ShowView(i == target);

        _networkManager.ChangeState(view);
        StartDrawing(target == 4);
    }

    private void StartDrawing(bool start)
    {
        _drawable.SetNewDrawing(start);
    }

    public void ShowWaringModal(string type)
    {
        _warningOverlayManager.ShowOverlay(type);
    }

    public void SendCoordinateData(Vector2 data)
    {
        _networkManager.Send(data.ToString());
    }

    public void StartDrawing()
    {
        _drawingCover.StartDrawing();
    }

    public void ReceiveDrawingData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;

        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(AppData.Delimiter);

        foreach (var token in tokens)
        {
            if (token == "") continue;

            if (token.Contains(char.ToString(AppData.ServerCommand)))
            {
                if (token == CommendBook.ROOM_CLOSED)
                {
                    _warningOverlayManager.ShowOverlay("RoomServer-Closed");
                    _drawingCover.StopDrawing();
                    _networkManager.ReconnectToNameServer();
                }
            }
            else if (token.Contains(char.ToString(AppData.ClientCommand)))
            {
                Debug.Log(token);
                if (token == CommendBook.END_OF_LINE)
                    _drawable.RemoteRelease();
                else if (token == CommendBook.COLOR_COMMEND + "RED")
                    _drawingSettings.SetMarkerRed();
                else if (token == CommendBook.COLOR_COMMEND + "BLUE")
                    _drawingSettings.SetMarkerBlue();
                else if (token == CommendBook.COLOR_COMMEND + "GREEN")
                    _drawingSettings.SetMarkerGreen();
                else if (token == CommendBook.COLOR_COMMEND + "BLACK")
                    _drawingSettings.SetMarkerBlack();
                else if (token == CommendBook.COLOR_COMMEND + "ERASE")
                    _drawingSettings.SetEraser();
                else if (token == CommendBook.CLEAR_BACKGROUND_COMMEND)
                    _drawable.ResetCanvas();
            }
            else
            {
                var pos = token.Split(',');
                //var vec = new Vector2(float.Parse(pos[0]) * resolutionX, float.Parse(pos[1]) * resolutionY);
                //FIXME : 상하 240, 좌우 5 정도의 크기차 발생 -> 1920-240*2 이후 240만큼을 보상함
                //현재는 아주 정확하게 맞지만 하드코딩상태. 여유가 있으면 바꾸자.
                var vec = new Vector2(float.Parse(pos[0]), float.Parse(pos[1]) - 240);
                _drawable.ReceiveCoordinateData(vec);
                _drawable.RemoteDrag();
            }
        }
    }

    public void ReceiveRoomData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;
        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(AppData.Delimiter);
        foreach (var token in tokens)
        {
            if (token == "") continue;
            if (token.Contains(char.ToString(AppData.ServerCommand)))
            {
                Debug.Log(token);
                if (token.Contains(CommendBook.HEADER_ROOMLIST))
                {
                    var roomList = token.Split(AppData.DelimiterUI).ToList();
                    roomList.Remove(CommendBook.HEADER_ROOMLIST);
                    roomList.Remove("");
                    _scrollManager.AddItemsFromList(roomList);
                }
                else if (token.Contains(CommendBook.CREATE_ROOM))
                {
                    var room = token.Split(AppData.DelimiterUI);
                    _roomView.ReadyToStartDrawing(room[1]);
                    Debug.Log(room[1] + " is Created Successfully!!");
                }
                else if (token == CommendBook.START_DRAWING)
                {
                    _networkManager.PauseNetworkThread();
                    _networkManager.SwitchRoomServer(true);

                }
                else if (token == CommendBook.GUEST_DRAWING)
                {
                    _networkManager.PauseNetworkThread();
                    _networkManager.SwitchRoomServer(false);

                }
                else if (token == CommendBook.ERROR_MESSAGE)
                {
                    _warningOverlayManager.ShowOverlay("");
                }
                else if (token == CommendBook.COMMEND_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Invalid-Commend");
                }
                else if (token == CommendBook.ROOM_CREATE_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Already-RoomName");
                }
                else if (token == CommendBook.PASSWORD_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Wrong-Pw");
                }
                else if (token == CommendBook.NO_ROOM_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("No-Room");
                }
            }
        }
    }

    public void GetRoomList()
    {
        _networkManager.Send(CommendBook.FIND_ROOM);
    }

    public void EnterRoom(string room, string pw)
    {
        _networkManager.Send(CommendBook.ENTER_ROOM + AppData.DelimiterUI + room + AppData.DelimiterUI + pw);
    }

    public void CreateRoom(string room, string pw)
    {
        _networkManager.Send(CommendBook.CREATE_ROOM + AppData.DelimiterUI + room + AppData.DelimiterUI + pw);
    }


    public void ExitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            _networkManager.TcpDisconnect();
            Application.Quit();
        #endif
    }
}
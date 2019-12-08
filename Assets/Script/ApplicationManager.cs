using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 어플리케이션의 모든 기능을 총괄하는 부분.
/// UI를 비롯해서 대부분의 기능을 실행하기 위한 컨트롤러 역할을 한다.
/// </summary>
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
        Screen.SetResolution(16*50, 11*50, false);
    }
    /// <summary>
    /// 어플리케이션에서 뷰의 변경을 총괄한다.
    /// 모든 뷰는 해당 매소드를 통해서 변경된다.
    /// </summary>
    /// <param name="view"></param>
    public void ChangeView(string view)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].ShowView(i == target);

        _networkManager.ChangeState(view);
        StartDrawing(target == 4);
    }
    /// <summary>
    /// 그림을 그리기 시작하도록 설정.
    /// </summary>
    /// <param name="start"></param>
    private void StartDrawing(bool start)
    {
        _drawable.SetNewDrawing(start);
    }
    /// <summary>
    /// 사용자에게 메시지를 통지하는 모달을 띄운다.
    /// 모달의 내용은 WarningOverlayManager에 정의되어 있다.
    /// </summary>
    /// <param name="type"></param>
    public void ShowWaringModal(string type)
    {
        _warningOverlayManager.ShowOverlay(type);
    }
    /// <summary>
    /// 그림을 그리기 시작하도록 설정한다.
    /// </summary>
    public void StartDrawing()
    {
        _drawingCover.StartDrawing();
    }

    /// <summary>
    /// 그림을 그리기 위한 데이터를 수신한다.
    /// 추가적으로 Drawing Room에 존재할 때 처리해야 하는 모든 동작을 포함한다.
    /// </summary>
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
                if (token == CommendBook.END_OF_LINE)
                {
                    _drawable.RemoteRelease();
                    continue;
                }
                else if (token == CommendBook.COLOR_COMMEND + "RED")
                {
                    _drawingSettings.SetMarkerRed();
                    continue;
                }
                else if (token == CommendBook.COLOR_COMMEND + "BLUE")
                {
                    _drawingSettings.SetMarkerBlue();
                    continue;
                }
                else if (token == CommendBook.COLOR_COMMEND + "GREEN")
                {
                    _drawingSettings.SetMarkerGreen();
                    continue;
                }
                else if (token == CommendBook.COLOR_COMMEND + "BLACK")
                {
                    _drawingSettings.SetMarkerBlack();
                    continue;
                }
                else if (token == CommendBook.COLOR_COMMEND + "ERASE")
                {
                    _drawingSettings.SetEraser();
                    continue;
                }
                else if (token == CommendBook.CLEAR_BACKGROUND_COMMEND)
                {
                    _drawable.ResetCanvas();
                    continue;
                }
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
    /// <summary>
    /// 프로그램이 Name Server에서 처리해야 하는 모든 동작과 명령을 포함한다.
    /// </summary>
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
                if (token.Contains(CommendBook.HEADER_ROOMLIST))
                {
                    var roomList = token.Split(AppData.DelimiterUI).ToList();
                    roomList.Remove(CommendBook.HEADER_ROOMLIST);
                    roomList.Remove("");
                    _scrollManager.AddItemsFromList(roomList);
                    continue;
                }
                else if (token.Contains(CommendBook.CREATE_ROOM))
                {
                    var room = token.Split(AppData.DelimiterUI);
                    _roomView.ReadyToStartDrawing(room[1]);
                    Debug.Log(room[1] + " is Created Successfully!!");
                    continue;
                }
                else if (token == CommendBook.START_DRAWING)
                {
                    _networkManager.PauseNetworkThread();
                    _networkManager.SwitchRoomServer(true);
                    continue;
                }
                else if (token == CommendBook.GUEST_DRAWING)
                {
                    _networkManager.PauseNetworkThread();
                    _networkManager.SwitchRoomServer(false);
                    continue;
                }
                else if (token == CommendBook.ERROR_MESSAGE)
                {
                    _warningOverlayManager.ShowOverlay("");
                    continue;
                }
                else if (token == CommendBook.COMMEND_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Invalid-Commend");
                    continue;
                }
                else if (token == CommendBook.ROOM_CREATE_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Already-RoomName");
                    continue;
                }
                else if (token == CommendBook.PASSWORD_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("Wrong-Pw");
                    continue;
                }
                else if (token == CommendBook.NO_ROOM_ERROR)
                {
                    _warningOverlayManager.ShowOverlay("No-Room");
                    continue;
                }
            }
        }
    }
    /// <summary>
    /// 방의 리스트를 요청한다.
    /// </summary>
    public void GetRoomList()
    {
        _networkManager.Send(CommendBook.FIND_ROOM);
    }
    /// <summary>
    /// 방에 접속을 시도한다.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="pw"></param>
    public void EnterRoom(string room, string pw)
    {
        _networkManager.Send(CommendBook.ENTER_ROOM + AppData.DelimiterUI + room + AppData.DelimiterUI + pw);
    }
    /// <summary>
    /// 방 생성을 시도한다.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="pw"></param>
    public void CreateRoom(string room, string pw)
    {
        _networkManager.Send(CommendBook.CREATE_ROOM + AppData.DelimiterUI + room + AppData.DelimiterUI + pw);
    }


    public void ExitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            _networkManager.TcpDisconnect(true);
        #endif
        Application.Quit();
    }
}
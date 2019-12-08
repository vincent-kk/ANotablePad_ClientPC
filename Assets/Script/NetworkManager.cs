using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 어플리케이션에서 사용하는 모든 네트워크 기능을 총괄하는 클래스
/// TcpManager를 사용하여 서버에 접속, 서버 전환, 재접속 등 네트워크 연결에 대한 모든 기능을 담당한다.
/// 또한 매 Update마다 데이터를 패치하는 기능도 담당한다.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    [SerializeField] private InputField serverIp = null;
    [SerializeField] private InputField serverPort = null;
    [SerializeField] private ApplicationManager _applicationManager;
    [SerializeField] private TcpManager _tcpManager;
    private byte[] _receiveBuffer;

    private static State _state = State.CONNECTION;
    /// <summary>
    /// 어플리케이션에 존재하는 네트워크의 상태.
    /// 상태에 따라 다른 기능을 요구한다.
    /// </summary>
    enum State
    {
        CONNECTION, // 연결 준비
        MENU, //메뉴 화면
        ROOM, //방에서 대기중
        SELECT, //방 선택
        DRAW, //실제로 그리는중
        PAUSE, //데이터 패치 중지
        ERROR, // 오류.
    };

    private void Awake()
    {
        _tcpManager.RegisterEventHandler(OnServerDisconnectedEvent);
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case State.CONNECTION:
                break;
            case State.MENU:
                break;
            case State.ROOM:
                _applicationManager.ReceiveRoomData();
                break;
            case State.SELECT:
                _applicationManager.ReceiveRoomData();
                break;
            case State.DRAW:
                if (!_tcpManager.Sock.Connected) TcpDisconnect(false);
                _applicationManager.ReceiveDrawingData();
                break;
            case State.PAUSE:
            case State.ERROR:
                break;
        }
    }
    /// <summary>
    /// NameServer의 정보를 이용하여 다시 접속을 시도한다.
    /// Room에서 이탈할때 자동으로 호출되어 Name Server로 돌아가게 한다.
    /// </summary>
    public void ReconnectToNameServer()
    {
        TcpDisconnect(true);
        Thread.Sleep(100);

        if (TcpConnection(AppData.ServerIp, AppData.ServerPort))
        {
            Send(AppData.ServerCommand + "PC");
            var returnData = new byte[64];
            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
            if (recvSize > 0)
            {
                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                if (msg.Equals(CommendBook.CONNECTION))
                {
                    _applicationManager.ChangeView("menu");
                    return;
                }
            }
        }

        TcpDisconnect(false);
    }

    /// <summary>
    /// Name Server에서 Room Server로 전환하는 매소드
    /// Host인지 Guest인지에 따라서 약간의 차이를 보이지만 기본적인 기능은 동일하다.
    /// 접속에 성공하면 Drawing을 시작한다.
    /// </summary>
    /// <param name="host"></param>
    public void SwitchRoomServer(bool host)
    {
        var returnData = new byte[64];
        var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
        if (recvSize > 0)
        {
            TcpDisconnect(true);
            var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
            var port = Convert.ToInt32(msg);
            ConsoleLogger(AppData.ServerIp + ":" + port + " Reconnect");
            if (TcpConnection(AppData.ServerIp, port))
            {
                Send(AppData.ServerCommand + (host ? "Host" : "Guest"));
                recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
                if (recvSize > 0)
                {
                    msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                    if (msg.Equals(CommendBook.CONNECTION))
                    {
                        _applicationManager.StartDrawing();
                        _applicationManager.ChangeView("draw");
                        return;
                    }
                    else if (msg.Equals(CommendBook.DRAWING_ROOM_FULL))
                    {
                        _applicationManager.ShowWaringModal("Full-Room");
                        ReconnectToNameServer();
                        return;
                    }
                }
            }
        }

        TcpDisconnect(false);
    }

    /// <summary>
    /// 최초에 Name Server에 접속을 시도하는 클래스.
    /// 기본적인 유효성 검증을 하며 접속에 성공하면 Name Server의 정보를 저장하여 이후 재접속이 가능하게 한다.
    /// </summary>
    public void ConnectToServer()
    {
        if (!AppData.IpRegex.IsMatch(serverIp.text))
        {
            _applicationManager.ShowWaringModal("Invalid-Ip");
            return;
        }

        AppData.ServerIp = serverIp.text;
        AppData.ServerPort = Convert.ToInt32(serverPort.text);
        Debug.Log("server : " + AppData.ServerIp + " : " + AppData.ServerPort);

        serverIp.text = "";
        serverPort.text = "";

        if (TcpConnection(AppData.ServerIp, AppData.ServerPort))
        {
            Send(AppData.ServerCommand + "PC");
            var returnData = new byte[64];
            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
            if (recvSize > 0)
            {
                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                if (msg.Equals(CommendBook.CONNECTION))
                {
                    _applicationManager.ChangeView("menu");
                    return;
                }
            }
        }

        _applicationManager.ShowWaringModal("Server-Not-Found");
        TcpDisconnect(false);
    }
    /// <summary>
    /// TcpManager를 사용하여 실제로 접속을 시도한다.
    /// TcpManager에 대한 일종의 캡슐화
    /// </summary>
    /// <param name="serverIp"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    private bool TcpConnection(string serverIp, int port)
    {
        return _tcpManager.Connect(serverIp, port);
    }
    /// <summary>
    /// TcpManager에 접속 종료를 통지한다.
    /// TcpManager에 대한 일종의 캡슐화
    /// </summary>
    /// <param name="switchServer"></param>
    public void TcpDisconnect(bool switchServer)
    {
        _tcpManager.Disconnect(switchServer);
    }
    /// <summary>
    /// Dispatch Thread를 일시 정지한다.
    /// </summary>
    public void PauseNetworkThread()
    {
        _tcpManager.Pause();
    }
    /// <summary>
    /// Dispatch Thread를 재개한다.
    /// </summary>
    public void ResumeNetworkThread()
    {
        _tcpManager.Resume();
    }
    /// <summary>
    /// 메시지를 받아서 송신을 시도한다.
    /// </summary>
    /// <param name="msg"></param>
    public void Send(string msg)
    {
        msg += AppData.Delimiter;
        var buffer = System.Text.Encoding.UTF8.GetBytes(msg);
        _tcpManager.Send(buffer, buffer.Length);
    }
    /// <summary>
    /// 메시지의 수신을 시도한다.
    /// </summary>
    /// <returns></returns>
    public string Receive()
    {
        var returnData = new byte[AppData.BufferSize];
        var recvSize = _tcpManager.Receive(ref returnData, returnData.Length);
        if (recvSize > 0)
        {
            var msg = System.Text.Encoding.UTF8.GetString(returnData);
            return msg;
        }

        return null;
    }

    private void ConsoleLogger(string log)
    {
        Debug.Log(log);
    }
    /// <summary>
    /// 네트워크의 상태를 변경한다.
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(string state)
    {
        switch (state)
        {
            case "connection":
                _state = State.CONNECTION;
                break;
            case "menu":
                _state = State.MENU;
                break;
            case "room":
                _state = State.ROOM;
                break;
            case "select":
                _state = State.SELECT;
                break;
            case "draw":
                _state = State.DRAW;
                break;
            case "pause":
                _state = State.PAUSE;
                break;
            case "error":
            default:
                _state = State.ERROR;
                break;
        }
    }
    /// <summary>
    /// 어플리케이션이 종료될때 실행되어 소켓의 연결을 끊는다.
    /// </summary>
    private void OnApplicationQuit()
    {
        if (_tcpManager != null)
        {
            _tcpManager.Disconnect(false);
        }
    }
    /// <summary>
    /// True Disconnection에서 호출되는 델리게이트 함수
    /// </summary>
    private void OnServerDisconnectedEvent()
    {
        _applicationManager.ShowWaringModal("Network-Disconnection");
        _applicationManager.ChangeView("connection");
        ConsoleLogger("Fail To Connect Server");
    }
}
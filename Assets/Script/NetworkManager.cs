using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private InputField serverIp = null;
    [SerializeField] private InputField serverPort = null;
    [SerializeField] private ApplicationManager _applicationManager;
    [SerializeField] private TcpManager _tcpManager;
    private byte[] _receiveBuffer;

    private static State _state = State.CONNECTION;

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

    private bool TcpConnection(string serverIp, int port)
    {
        return _tcpManager.Connect(serverIp, port);
    }

    public void TcpDisconnect(bool switchServer)
    {
        _tcpManager.Disconnect(switchServer);
    }

    public void PauseNetworkThread()
    {
        _tcpManager.Pause();
    }

    public void ResumeNetworkThread()
    {
        _tcpManager.Resume();
    }

    public void Send(string msg)
    {
        msg += AppData.Delimiter;
        var buffer = System.Text.Encoding.UTF8.GetBytes(msg);
        _tcpManager.Send(buffer, buffer.Length);
    }

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

    private void OnApplicationQuit()
    {
        if (_tcpManager != null)
        {
            _tcpManager.Disconnect(false);
        }
    }

    private void OnServerDisconnectedEvent()
    {
        _applicationManager.ShowWaringModal("Network-Disconnection");
        _applicationManager.ChangeView("connection");
        ConsoleLogger("Fail To Connect Server");
    }
}
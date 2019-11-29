using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private InputField serverIp = null;
    [SerializeField] private InputField serverPort = null;
    [SerializeField] private ApplicationManager _applicationManager;

    public int bufferSize = 1024;
    private char _delimiter;
    private char _serverCommand;


    private byte[] _receiveBuffer;

    private TcpManager _tcpManager = null;
    private Encoding _encode;

    private static State _state = State.CONNECTION;

    enum State
    {
        CONNECTION, // 연결 준비
        MENU, //메뉴 화면
        ROOM, //방에서 대기중
        DRAW, //실제로 그리는중
        ERROR, // 오류.
    };


    // Start is called before the first frame update
    void Start()
    {
        _tcpManager = gameObject.AddComponent<TcpManager>();
        _delimiter = _applicationManager.GetDelimiter();
        _serverCommand = _applicationManager.GetServerCommand();
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
                break;
            case State.DRAW:
                _applicationManager.ReceiveDrawingData();
                break;
            case State.ERROR:
                break;
        }
    }

    public void ConnectToServer()
    {
        var ip = serverIp.text;
        var port = Convert.ToInt32(serverPort.text);

        Debug.Log("server : " + ip + " : " + port);

        serverIp.text = "";
        serverPort.text = "";

//        if (TcpConnection(ip, port))
//        {
//            Send(_serverCommand + "Host-PC");
//            var returnData = new byte[64];
//            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
//            if (recvSize > 0)
//            {
//                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
//                if (msg.Equals(_serverCommand + "CONNECTION"))
//                {
//                    _applicationManager.ChangeView("draw","draw");
//                }
//                else
//                {
//                    ConsoleLogger("Fail To Connect Server");
//                }
//            }
//            else
//            {
//                ConsoleLogger("Fail To Connect Server");
//            }
//        }
        
        if (TcpConnection(ip, port))
        {
            Send(_serverCommand + "Host-PC");
            var returnData = new byte[64];
            var recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
            if (recvSize > 0)
            {
                TcpDisconnect();
                var msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                port = Convert.ToInt32(msg);
                ConsoleLogger(ip +":"+port+" Reconnect");
                if (TcpConnection(ip, port))
                {
                    Send(_serverCommand + "Host-PC");
                    recvSize = _tcpManager.BlockingReceive(ref returnData, returnData.Length);
                    if (recvSize > 0)
                    {
                        msg = Encoding.UTF8.GetString(returnData).TrimEnd('\0');
                        if (msg.Equals(_serverCommand + "CONNECTION"))
                        {
                            _applicationManager.ChangeView("draw","draw");
                        }
                    }
                    else
                    {
                        ConsoleLogger("Fail To Connect Server");
                    }
                }
            }
        }
        else
        {
            ConsoleLogger("Fail To Connect Server");
        }
    }

    private bool TcpConnection(string serverIp, int port)
    {
        return _tcpManager.Connect(serverIp, port);
    }

    private void TcpDisconnect()
    {
        _tcpManager.Disconnect();
    }

    public void Send(string msg)
    {
        msg += _delimiter;
        var buffer = System.Text.Encoding.UTF8.GetBytes(msg);
        _tcpManager.Send(buffer, buffer.Length);
        ConsoleLogger(msg);
    }

    public string Receive()
    {
        var returnData = new byte[bufferSize];
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

    private void OnApplicationQuit()
    {
        if (_tcpManager != null)
        {
            _tcpManager.StopServer();
        }
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
            case "draw":
                _state = State.DRAW;
                break;
            case "error":
            default:
                _state = State.ERROR;
                break;
        }
    }

    public void OnEventHandling(NetEventState state)
    {
        switch (state.type)
        {
            case NetEventType.Connect:
                if (_tcpManager.IsServer())
                {
                }
                else
                {
                }

                break;

            case NetEventType.Disconnect:
                if (_tcpManager.IsServer())
                {
                }
                else
                {
                }

                break;
        }
    }
}
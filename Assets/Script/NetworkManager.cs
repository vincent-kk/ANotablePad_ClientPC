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
    [SerializeField] private ApplicationManager app;

    public int bufferSize = 1024;
    private readonly string _delimiter = "|";


    private byte[] _receiveBuffer;

    private TcpManager _tcpManager = null;
    private Encoding _encode;

    private static ChatState _state = ChatState.HOST_TYPE_SELECT;

    enum ChatState
    {
        HOST_TYPE_SELECT = 0, // 방 선택.
        CONNECTION, // 연결.
        LEAVE, // 나가기.
        ERROR, // 오류.
    };


    // Start is called before the first frame update
    void Start()
    {
        _tcpManager = gameObject.AddComponent<TcpManager>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case ChatState.HOST_TYPE_SELECT:
                break;
            case ChatState.CONNECTION:
                Receive();
                break;
            case ChatState.LEAVE:
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

        if (TcpConnection(ip, port))
        {
            Send("@Host-PC");
            app.ChangeView("draw");
        }
        else
        {
            ConsoleLogger("Fail To Connect Server");
        }
    }

    private bool TcpConnection(string serverIp, int port)
    {
        var ret = _tcpManager.Connect(serverIp, port);
        _state = ret ? ChatState.CONNECTION : ChatState.ERROR;
        return ret;
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
            ConsoleLogger(msg);
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
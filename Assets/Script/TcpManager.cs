using System;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class TcpManager : MonoBehaviour
{
    // 클라이언트와의 접속용 소켓.
    private Socket _socket = null;
    public Socket Sock => _socket;

    // 송신 버퍼.
    private PacketQueue _sendQueue;

    // 수신 버퍼.
    private PacketQueue _recvQueue;

    // 접속 플래그.
    private bool _isConnected = false;

    public bool isConnected => _isConnected;

    // 이벤트 통지 델리게이트.
    public delegate void EventHandler();

    private EventHandler _handler;

    private bool _threadLoop = false;

    private Thread _thread = null;

    private Thread _garbagecollector;

    // Use this for initialization
    void Start()
    {
        // 송수신 버퍼를 작성합니다.
        _sendQueue = new PacketQueue();
        _recvQueue = new PacketQueue();
        _garbagecollector = new Thread(new ThreadStart(Observing));
    }

    // 접속.
    public bool Connect(string address, int port)
    {
        Debug.Log("TransportTCP connect called.");

        bool ret = false;
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            _socket.SendBufferSize = 0;
            _socket.Connect(address, port);
            ret = LaunchThread();
        }
        catch
        {
            _socket = null;
        }

        if (ret == true)
        {
            _isConnected = true;
            Debug.Log("Connection success.");
        }
        else
        {
            _isConnected = false;
            Debug.Log("Connect fail");
        }

        return _isConnected;
    }

    // 끊기.
    public void Disconnect(bool switchServer)
    {
        if (_socket == null) return;
        _isConnected = false;
        _threadLoop = false;
        // 소켓 클로즈.
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException)
        {
            Debug.Log("Server Disconnection");
        }
        finally
        {
            _socket.Close();
            _socket = null;
        }

        if (!switchServer) _handler?.Invoke();
    }

    private void Observing()
    {
        while (true)
        {
            if (_thread != null) _thread.Join();
            else Thread.Sleep(1000);
        }
    }

    // 송신처리.
    public int Send(byte[] data, int size)
    {
        if (_sendQueue == null)
        {
            return 0;
        }

        return _sendQueue.Enqueue(data, size);
    }

    // 수신처리.
    public int Receive(ref byte[] buffer, int size)
    {
        if (_recvQueue == null)
        {
            return 0;
        }

        return _recvQueue.Dequeue(ref buffer, size);
    }

    public int BlockingReceive(ref byte[] buffer, int size)
    {
        return _socket.Receive(buffer, size, SocketFlags.None);
    }

    // 이벤트 통지함수 등록.
    public void RegisterEventHandler(EventHandler handler)
    {
        _handler += handler;
    }

    // 이벤트 통지함수 삭제.
    public void UnregisterEventHandler(EventHandler handler)
    {
        _handler -= handler;
    }

    // 스레드 실행 함수.
    bool LaunchThread()
    {
        try
        {
            // Dispatch용 스레드 시작.
            _threadLoop = true;
            _thread = new Thread(new ThreadStart(Dispatch));
            _thread.Start();
        }
        catch
        {
            Debug.Log("Cannot launch thread.");
            return false;
        }

        return true;
    }

    // 스레드 측의 송수신 처리.
    public void Dispatch()
    {
        Debug.Log("Dispatch thread started.");

        while (_threadLoop)
        {
            // 클라이언트와의 송수신을 처리합니다.
            if (_socket != null && _isConnected == true)
            {
                // 송신처리.
                DispatchSend();
                // 수신처리.
                DispatchReceive();
            }

            Thread.Sleep(5);
        }

        Debug.Log("Dispatch thread ended.");
    }

    // 스레드 측 송신처리 .
    void DispatchSend()
    {
        try
        {
            // 송신처리.
            if (_socket.Poll(0, SelectMode.SelectWrite))
            {
                byte[] buffer = new byte[AppData.BufferSize];

                int sendSize = _sendQueue.Dequeue(ref buffer, buffer.Length);
                while (sendSize > 0)
                {
                    _socket.Send(buffer, sendSize, SocketFlags.None);
                    sendSize = _sendQueue.Dequeue(ref buffer, buffer.Length);
                }
            }
        }
        catch
        {
            return;
        }
    }

    // 스레드 측의 수신처리.
    void DispatchReceive()
    {
        // 수신처리.
        try
        {
            while (_socket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[AppData.BufferSize];

                int recvSize = _socket.Receive(buffer, buffer.Length, SocketFlags.None);
                if (recvSize == 0)
                {
                    // 끊기.
                    Debug.Log("Disconnect recv from Server.");
                    Disconnect(false);
                }
                else if (recvSize > 0)
                {
                    _recvQueue.Enqueue(buffer, recvSize);
                }
            }
        }
        catch
        {
            return;
        }
    }
    public void Pause()
    {
        try
        {
            _thread.Suspend();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void Resume()
    {
        try
        {
            _thread.Resume();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
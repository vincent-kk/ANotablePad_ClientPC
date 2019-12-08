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

    /// <summary>
    /// 접속 요청을 처리. 접속에 성공하면 Dispatch Thread를 동작한다.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 접속 종료 통지.
    /// 서버를 전환하기 위한 것인지, 완전히 종료하기 위함인지를 구분한다.
    /// </summary>
    /// <param name="switchServer"></param>
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
    /// <summary>
    /// Dispatch Thread를 Join시키기 위한 Thread.
    /// Thread가 없을 경우엔 1초씩 대기하며 Thread를 기다린다.
    /// </summary>
    private void Observing()
    {
        while (true)
        {
            if (_thread != null) _thread.Join();
            else Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// byte array 전송 요청.
    /// Send Queue에 저장하고 다음 Dispatch에서 실제 전송이 발생한다.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public int Send(byte[] data, int size)
    {
        if (_sendQueue == null)
        {
            return 0;
        }

        return _sendQueue.Enqueue(data, size);
    }

    /// <summary>
    /// 현재 Receive Queue에 있는 데이터를 추출하여 반환한다.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public int Receive(ref byte[] buffer, int size)
    {
        if (_recvQueue == null)
        {
            return 0;
        }

        return _recvQueue.Dequeue(ref buffer, size);
    }
    /// <summary>
    /// Receive Queue를 쓰지 않고 Blocking으로 데이터를 수신하기 위한 함수.
    /// 동기화를 위해 사용된다.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public int BlockingReceive(ref byte[] buffer, int size)
    {
        return _socket.Receive(buffer, size, SocketFlags.None);
    }

    /// <summary>
    /// 델리게이트를 등록하여 사용한다.
    /// </summary>
    /// <param name="handler"></param>
    public void RegisterEventHandler(EventHandler handler)
    {
        _handler += handler;
    }

    /// <summary>
    /// 델리게이트를 삭제한다.
    /// </summary>
    /// <param name="handler"></param>
    public void UnregisterEventHandler(EventHandler handler)
    {
        _handler -= handler;
    }

    /// <summary>
    /// Dispatch Thread를 동작시킨다.
    /// 이후 Connection이 유지되는 동안에 계속 데이터를 수신/송신한다.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Loop를 돌며 Dispatch 동작을 수행한다.
    /// Non-Blocking 수신/송신을 가능하게 한다.
    /// </summary>
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

    /// <summary>
    /// Dispatch Thread의 송신 처리부.
    /// 큐의 데이터를 꺼내서 실제 송신 버퍼에 데이터를 전달한다.
    /// </summary>
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

    /// <summary>
    /// Dispatch Thread의 수신 처리부
    /// 수신 데이터가 있으면 이를 받아서 버퍼 큐에 저장한다.
    /// 전송되는 데이터가 없으면 끊겼다고 판단하여 끊김을 통지한다.
    /// </summary>
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
    /// <summary>
    /// Dispatch Thread를 일시 정지한다.
    /// </summary>
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
    /// <summary>
    /// Dispatch Thread를 재개한다.
    /// </summary>
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeDraw;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField] private GameObject[] views;
    [SerializeField] private Drawable _drawable;
    [SerializeField] private DrawingSettings _drawingSettings;
    [SerializeField] private NetworkManager _networkManager;

    private readonly char _delimiter = '|';
    private readonly char _clientCommand = '#';
    private readonly char _serverCommand = '@';


    private readonly int resolutionX = 1080;
    private readonly int resolutionY = 1920;


    private readonly Dictionary<string, int> _viewName = new Dictionary<string, int>(4)
    {
        {"connection", 0},
        {"menu", 1},
        {"room", 2},
        {"draw", 3}
    };

    private void Start()
    {
        ChangeView("connection", "connection");
    }

    public void ChangeView(string view, string state)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].SetActive(i == target);

        _networkManager.ChangeState(state);
        StartDrawing(target == 3);
    }

    private void StartDrawing(bool start)
    {
        _drawable.SetNewDrawing(start);
    }

    public char GetDelimiter()
    {
        return _delimiter;
    }

    public char GetClientCommand()
    {
        return _clientCommand;
    }

    public char GetServerCommand()
    {
        return _serverCommand;
    }


    public void SendCoordinateData(Vector2 data)
    {
        _networkManager.Send(data.ToString());
    }

    public void ReceiveDrawingData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;

        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(_delimiter);

        foreach (var token in tokens)
        {
            if (token == "") continue;

            if (token.Contains(char.ToString(_serverCommand))) ;
            else if (token.Contains(char.ToString(_clientCommand)))
            {
//                StringBuilder sb = new StringBuilder(token);
//                sb.Remove(0, 1);
//                var commend = sb.ToString();
                Debug.Log(token);
                if (token == _clientCommand + "EOL")
                    _drawable.RemoteRelease();
                else if (token == _clientCommand + "CC->RED")
                    _drawingSettings.SetMarkerRed();
                else if (token == _clientCommand + "CC->BLUE")
                    _drawingSettings.SetMarkerBlue();
                else if (token == _clientCommand + "CC->GREEN")
                    _drawingSettings.SetMarkerGreen();
                else if (token == _clientCommand + "CC->BLACK")
                    _drawingSettings.SetMarkerBlack();
                else if (token == _clientCommand + "CC->ERASE")
                    _drawingSettings.SetEraser();
                else if (token == _clientCommand + "BG->CLEAR")
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
}
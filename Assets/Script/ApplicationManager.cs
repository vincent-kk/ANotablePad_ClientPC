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

    private char _delimiter;

    private readonly Dictionary<string, int> _viewName = new Dictionary<string, int>(4)
    {
        {"connection", 0},
        {"menu", 1},
        {"room", 2},
        {"draw", 3}
    };

    private void Start()
    {
        ChangeView("connection");
        _delimiter = _networkManager.GetDelimiter();
    }

    public void ChangeView(string view)
    {
        var target = _viewName[view];
        for (var i = 0; i < views.Length; i++)
            views[i].SetActive(i == target);

        StartDrawing(target == 3);
    }

    private void StartDrawing(bool start)
    {
        _drawable.SetNewDrawing(start);
    }

    public void ChangeColor(string color)
    {
    }

    public void SendCoordinateData(Vector2 data)
    {
        _networkManager.Send(data.ToString());
    }

    public void ReceiveDrawingData()
    {
        var msg = _networkManager.Receive();
        if (msg == null) return;

//        msg = msg.TrimEnd('\0');
        var tokens = msg.Split(_delimiter);

        foreach (var token in tokens)
        {
            if (token.Contains("\0")) continue;

            if (msg.Contains("@")) ;
            else if (msg.Contains("#"))
            {
                StringBuilder sb = new StringBuilder(token);
                sb.Remove(0, 1);
                var commend = sb.ToString();
                Console.WriteLine(commend);
                if (commend == "EOF") _drawable.RemoteRelease();
            }
            else
            {
                var pos = token.Split(',');
                var vec2 = new Vector2(float.Parse(pos[0]), float.Parse(pos[1]));
                _drawable.ReceiveCoordinateData(vec2);
                _drawable.RemoteDrag();
            }
        }
    }
}
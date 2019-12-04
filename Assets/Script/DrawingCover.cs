using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingCover : MonoBehaviour
{
    private void Start()
    {
        StopDrawing();
    }

    public void StartDrawing()
    {
        this.gameObject.SetActive(false);
    }

    public void StopDrawing()
    {
        this.gameObject.SetActive(true);
    }
}
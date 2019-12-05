using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Helper methods used to set drawing settings
public class DrawingSettings : MonoBehaviour
{
    public static bool isCursorOverUI = false;
    public float Transparency = 1f;
    public Color Reset_Colour = new Color(0.94f, 0.94f, 0.973f, 1);
    public int markerSize = 2;
    public int eraserSize = 20;

    // Changing pen settings is easy as changing the static properties Drawable.Pen_Colour and Drawable.Pen_Width
    public void SetMarkerColour(Color new_color)
    {
        Drawable.Pen_Colour = new_color;
    }

    // new_width is radius in pixels
    public void SetMarkerWidth(int new_width)
    {
        Drawable.Pen_Width = new_width;
    }

    public void SetMarkerWidth(float new_width)
    {
        SetMarkerWidth((int) new_width);
    }

    public void SetTransparency(float amount)
    {
        Transparency = amount;
        Color c = Drawable.Pen_Colour;
        c.a = amount;
        Drawable.Pen_Colour = c;
    }


    // Call these these to change the pen settings
    public void SetMarkerRed()
    {
        SetMarkerWidth(markerSize);
        Color c = Color.red;
        c.a = Transparency;
        SetMarkerColour(c);
        Drawable.drawable.SetPenBrush();
    }

    public void SetMarkerGreen()
    {
        SetMarkerWidth(markerSize);
        Color c = Color.green;
        c.a = Transparency;
        SetMarkerColour(c);
        Drawable.drawable.SetPenBrush();
    }

    public void SetMarkerBlue()
    {
        SetMarkerWidth(markerSize);
        Color c = Color.blue;
        c.a = Transparency;
        SetMarkerColour(c);
        Drawable.drawable.SetPenBrush();
    }

    public void SetMarkerBlack()
    {
        SetMarkerWidth(markerSize);
        Color c = Color.black;
        c.a = Transparency;
        SetMarkerColour(c);
        Drawable.drawable.SetPenBrush();
    }

    public void SetEraser()
    {
        SetMarkerWidth(eraserSize);
        SetMarkerColour(Reset_Colour);
    }

    public void PartialSetEraser()
    {
        SetMarkerColour(Reset_Colour);
    }
}
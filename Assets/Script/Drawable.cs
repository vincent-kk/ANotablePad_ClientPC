using UnityEngine;

/// <summary>
/// Free Draw라는 Asset을 기반으로 내부를 수정하여 구현한 부분.
/// 대부분의 코드를 재사용하였다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] 

public class Drawable : MonoBehaviour
{
    [SerializeField] private ApplicationManager _appicationManager;

    private bool _nowDrawing;

    // PEN COLOUR
    public static Color Pen_Colour = Color.black; 


    public static int Pen_Width = 2;


    public delegate void Brush_Function(Vector2 world_position);

    public Brush_Function current_brush;

    public LayerMask Drawing_Layers;

    public bool Reset_Canvas_On_Play = true;

    public Color Reset_Colour = new Color(0.94f, 0.94f, 0.973f, 1); 

    public static Drawable drawable;
    Sprite drawable_sprite;
    Texture2D drawable_texture;

    Vector2 previous_drag_position;
    Color[] clean_colours_array;
    Color transparent;
    Color32[] cur_colors;
    bool mouse_was_previously_held_down = false;
    bool no_drawing_on_current_drag = false;

    public void BrushTemplate(Vector2 world_position)
    {
        Vector2 pixel_pos = WorldToPixelCoordinates(world_position);

        cur_colors = drawable_texture.GetPixels32();

        if (previous_drag_position == Vector2.zero)
        {
            MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
        }
        else
        {
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
        }
        ApplyMarkedPixelChanges();
        previous_drag_position = pixel_pos;
    }

    private void PenBrush(Vector2 world_point)
    {
        Vector2 pixel_pos = WorldToPixelCoordinates(world_point);

        cur_colors = drawable_texture.GetPixels32();

        if (previous_drag_position == Vector2.zero)
        {
            MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
        }
        else
        {
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
        }

        ApplyMarkedPixelChanges();

        previous_drag_position = pixel_pos;
    }

    public void SetPenBrush()
    {
        current_brush = PenBrush;
    }

    public void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
    {
        // Get the distance from start to finish
        float distance = Vector2.Distance(start_point, end_point);
        Vector2 direction = (start_point - end_point).normalized;

        Vector2 cur_position = start_point;
        float lerp_steps = 1 / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }


    public void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        int center_x = (int) center_pixel.x;
        int center_y = (int) center_pixel.y;

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            if (x >= (int) drawable_sprite.rect.width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen);
            }
        }
    }

    public void MarkPixelToChange(int x, int y, Color color)
    {
        int array_pos = y * (int) drawable_sprite.rect.width + x;
        if (array_pos > cur_colors.Length || array_pos < 0)
            return;

        cur_colors[array_pos] = color;
    }

    public void ApplyMarkedPixelChanges()
    {
        drawable_texture.SetPixels32(cur_colors);
        drawable_texture.Apply();
    }

    public void ColourPixels(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        int center_x = (int) center_pixel.x;
        int center_y = (int) center_pixel.y;

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                drawable_texture.SetPixel(x, y, color_of_pen);
            }
        }

        drawable_texture.Apply();
    }


    public Vector2 WorldToPixelCoordinates(Vector2 world_position)
    {
        Vector3 local_pos = transform.InverseTransformPoint(world_position);

        float pixelWidth = drawable_sprite.rect.width;
        float pixelHeight = drawable_sprite.rect.height;
        float unitsToPixels = pixelWidth / drawable_sprite.bounds.size.x * transform.localScale.x;

        float centered_x = local_pos.x * unitsToPixels + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixels + pixelHeight / 2;

        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));

        return pixel_pos;
    }

    public void ResetCanvas()
    {
        drawable_texture.SetPixels(clean_colours_array);
        drawable_texture.Apply();
    }

    public void SetNewDrawing(bool drawing)
    {
        if (_nowDrawing != drawing) ResetCanvas();
        this._nowDrawing = drawing;

        Pen_Colour = Color.black;
        Pen_Width = 2;
    }

    private void RemoteDraw(Vector2 pixel_pos)
    {
        cur_colors = drawable_texture.GetPixels32();
        if (previous_drag_position == Vector2.zero)
            MarkPixelsToColour(pixel_pos, Pen_Width, Pen_Colour);
        else
            ColourBetween(previous_drag_position, pixel_pos, Pen_Width, Pen_Colour);
        ApplyMarkedPixelChanges();
        previous_drag_position = pixel_pos;
    }

    public void ReceiveCoordinateData(Vector2 data)
    {
        RemoteDraw(data);
    }

    public void RemoteRelease()
    {
        previous_drag_position = Vector2.zero;
        no_drawing_on_current_drag = false;
    }

    public void RemoteDrag()
    {
        no_drawing_on_current_drag = true;
    }


    void Awake()
    {
        _nowDrawing = false;
        drawable = this;
        current_brush = PenBrush;

        drawable_sprite = this.GetComponent<SpriteRenderer>().sprite;
        drawable_texture = drawable_sprite.texture;
        clean_colours_array = new Color[(int) drawable_sprite.rect.width * (int) drawable_sprite.rect.height];
        for (int x = 0; x < clean_colours_array.Length; x++)
            clean_colours_array[x] = Reset_Colour;
        if (Reset_Canvas_On_Play)
            ResetCanvas();
    }
}
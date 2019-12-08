using UnityEngine;
/// <summary>
/// 그림을 그리는 화면을 전환하는 클래스
/// </summary>
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
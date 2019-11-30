
using UnityEngine;

public class ImageView : MonoBehaviour,IView
{
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
    }
}

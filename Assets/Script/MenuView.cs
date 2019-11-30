
using UnityEngine;

public class MenuView : MonoBehaviour, IView
{
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
    }
}

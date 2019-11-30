
using UnityEngine;

public class ConnectView : MonoBehaviour, IView
{
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
    }
}

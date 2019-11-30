
using UnityEngine;
using UnityEngine.UI;

public class ReadyView : MonoBehaviour, IView
{
    [SerializeField] private InputField room;
    [SerializeField] private InputField pw;

    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
        room.text = "";
        pw.text = "";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectView : MonoBehaviour, IView
{
    [SerializeField] private ApplicationManager _applicationManager;
    public void ShowView(bool show)
    {
        this.gameObject.SetActive(show);
        if(show)  _applicationManager.GetRoomList();
    }
}

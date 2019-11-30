using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour, Item
{
    [SerializeField] private GameObject background;
    [SerializeField] private Text nameTag;
    private OverlayManager _overlayManager;

    private string _name;

    public void SetItemData(string name)
    {
        _name = name;
        nameTag.text = name;
    }

    public void TouchEvent()
    {
        if (_overlayManager == null)
            _overlayManager = GameObject.Find("RoomModalOverlay").GetComponent<OverlayManager>();
        _overlayManager.ShowOverlay(_name);
    }

    public void ShowItem()
    {
        this.gameObject.SetActive(true);
    }

    public void HideItem()
    {
        this.gameObject.SetActive(false);
    }

    public void DeleteItem()
    {
        Destroy(this.gameObject);
    }
}
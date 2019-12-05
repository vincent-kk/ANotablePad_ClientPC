using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour, Item
{
    [SerializeField] private Image background;
    [SerializeField] private Text nameTag;
    [SerializeField] private Sprite[] roomBGI;
    private OverlayManager _overlayManager;
    private string _name;

    public void SetItemData(string name)
    {
        int num = Random.Range(0, 6);
        background.sprite = roomBGI[num];
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
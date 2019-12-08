using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Room List에 등장하는 방 하나하나에 적용되는 클래스.
/// 방을 눌렀을 때의 동작 등을 설정한다.
/// </summary>
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
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 방에 입장할때 등장하는 모달을 정의한다.
/// 오버레이 매니저는 Disable되지 않고 이벤트를 통지받아서 모달을 생성한다.
/// </summary>
public class OverlayManager : MonoBehaviour
{
    [SerializeField] private GameObject modal;
    [SerializeField] private Text roomNameField;
    [SerializeField] private InputField password;
    [SerializeField] private ScrollRect blockBoxScroll;
    [SerializeField] private Image overlayAlpha;
    [SerializeField] private Button overlayButton;
    [SerializeField] private ApplicationManager _applicationManager;

    private string _roomName;
    // Start is called before the first frame update
    void Start()
    {
        ShowOverlayElements(false);
    }

    public void ShowOverlay(string room)
    {
        _roomName = room;
        ShowOverlayElements(true);
        roomNameField.text = room;
    }

    public void HideOverlay()
    {
        ShowOverlayElements(false);
    }

    private void ShowOverlayElements(bool forShow)
    {
        modal.SetActive(forShow);
        overlayAlpha.raycastTarget = forShow;
        overlayButton.interactable = forShow;

        blockBoxScroll.enabled = !forShow;
    }

    public void TryToEnterRoom()
    {
        var pw = password.text;
        password.text = "";
        ShowOverlayElements(false);
        _applicationManager.EnterRoom(_roomName, pw);
    }
}

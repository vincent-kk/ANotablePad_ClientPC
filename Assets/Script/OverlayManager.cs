using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayManager : MonoBehaviour
{
    [SerializeField] private GameObject modal;
    [SerializeField] private Text roomNameField;
    [SerializeField] private InputField password;

    [SerializeField] private ScrollRect blockBoxScroll;
    [SerializeField] private Image overlayAlpha;
    [SerializeField] private Button overlayButton;

    [SerializeField] private ApplicationManager _applicationManager;

    private readonly Color _disableColor = new Color(0, 0, 0, 0);
    private readonly Color _activeColor = new Color(0, 0, 0, 0.45f);

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
        overlayAlpha.color = forShow ? _activeColor : _disableColor;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningOverlayManager : MonoBehaviour
{
    [SerializeField] private GameObject modal;
    [SerializeField] private Text title;
    [SerializeField] private Text body;

    [SerializeField] private Image overlayAlpha;
    [SerializeField] private Button overlayButton;


    private readonly Color _disableColor = new Color(0, 0, 0, 0);
    private readonly Color _activeColor = new Color(0, 0, 0, 0.45f);

    // Start is called before the first frame update
    void Start()
    {
        ShowOverlayElements(false);
    }

    public void ShowOverlay(string type)
    {
        MakeModalMessage(type);
        ShowOverlayElements(true);
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
    }

    private void MakeModalMessage(string type)
    {
        switch (type)
        {
            case "Wrong-Pw":
                title.text = "Invalid Password";
                body.text = "Please check your password";
                break;
            case "Invalid-Name":
                title.text = "Invalid Name";
                body.text = "Do not use special characters";
                break;
            case "Invalid-Ip":
                title.text = "Invalid IP";
                body.text = "IP has the following format:\n[0,255].[0,255].[0,255].[0,255]";
                break;
            case "Network-Disconnection":
                title.text = "Disconnection";
                body.text = "Name Server Error";
                break;
            case "RoomServer-Closed":
                title.text = "Room Closed";
                body.text = "Room Server is closed";
                break;
            case "Invalid-Commend":
                title.text = "Invalid Commend";
                body.text = "Commend Error!";
                break;
            case "No-Room":
                title.text = "Room Disappears";
                body.text = "Where is your Room??";
                break;
            case "Already-RoomName":
                title.text = "Room Name in Use";
                body.text = "The Room Name is already in use";
                break;
            default:
                title.text = "Waring";
                body.text = "A fatal problem has occurred";
                break;
        }
    }
}
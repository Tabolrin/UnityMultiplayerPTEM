using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonTextRefHolder : MonoBehaviour
{
    public string SessionName;
    public TMP_Text buttonText;
    public Button thisButton;
    
    public UnityEvent<string> onButtonClick;
    
    public void InvokeButtonClickEvent()
    {
        onButtonClick.Invoke(SessionName);
    }
}

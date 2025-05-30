using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonTextRefHolder : MonoBehaviour
{
    public TMP_Text buttonText;
    public Button thisButton;
    
    public UnityEvent<TMP_Text> onButtonClick;
    
    public void InvokeButtonClickEvent()
    {
        Debug.Log("weeeee");
        onButtonClick.Invoke(buttonText);
    }
}

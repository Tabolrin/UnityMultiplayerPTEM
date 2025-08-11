using UnityEngine;

[CreateAssetMenu(fileName = "UiNotificationTexts", menuName = "Scriptable Objects/UiNotificationTexts")]
public class UiNotificationTexts : ScriptableObject
{
    [field: SerializeField] public string LockedSession { get; private set; }
    [field: SerializeField] public string MinimalPlayerCountNotReached { get; private set; }
    [field: SerializeField] public string MaximalPlayerCountExceeded { get; private set; }
}
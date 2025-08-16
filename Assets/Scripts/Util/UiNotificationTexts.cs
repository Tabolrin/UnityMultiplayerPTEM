using UnityEngine;

[CreateAssetMenu(fileName = "UiNotificationTexts", menuName = "Scriptable Objects/UiNotificationTexts")]
public class UiNotificationTexts : ScriptableObject
{
    [field: SerializeField] public string SessionLocked { get; private set; }
    [field: SerializeField] public string SessionFull { get; private set; }
    [field: SerializeField] public string FailedToJoinSession { get; private set; }
    [field: SerializeField] public string MinimalPlayerCountNotReached { get; private set; }
    [field: SerializeField] public string MaximalPlayerCountExceeded { get; private set; }
    [field: SerializeField] public string InvalidSessionName { get; private set; }
    [field: SerializeField] public string SessionNameAlreadyExists { get; private set; }
    [field: SerializeField] public string FailedToStartSession { get; private set; }
    [field: SerializeField] public string NicknameEmptyOrNull { get; private set; }
    [field: SerializeField] public string NicknameLengthError { get; private set; } 
    [field: SerializeField] public string PlayersStillJoining { get; private set; }
    [field: SerializeField] public string PlayerLeftMatch { get; private set; }
}
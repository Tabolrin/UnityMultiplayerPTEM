using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [field: SerializeField] public float LookSpeed { get; private set; }
    [field: SerializeField] public float MoveSpeed { get; private set; }
}

using UnityEngine;

public class SaveWinningTeam : MonoBehaviour
{
    static public SaveWinningTeam Instance;
    public byte WinningTeam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }
}

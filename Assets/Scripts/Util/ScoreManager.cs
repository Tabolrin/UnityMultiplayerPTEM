using UnityEngine;

public struct ScoreData
{
    public int score;
    public int deaths;

    public ScoreData(int score, int kills, int deaths)
    {
        this.score = score;
        this.deaths = deaths;
    }
}


public class ScoreManager : MonoBehaviour
{
    
    
}

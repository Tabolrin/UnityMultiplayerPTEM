[System.Serializable]
public struct ScoreData
{
    public int score;
    public string name;
    public AstronautColor teamColor;

    public ScoreData(string name, int score, AstronautColor teamColor)
    {
        this.name = name;
        this.score = score;
        this.teamColor = teamColor;
    }
}

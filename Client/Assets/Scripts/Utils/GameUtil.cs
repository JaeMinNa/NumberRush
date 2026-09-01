using UnityEngine;

public static class GameUtil 
{
    public static int GetLevel(int score)
    {
        return (score / 1000) + 1;
    }
}

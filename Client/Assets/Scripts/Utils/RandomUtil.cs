using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RandomUtil
{
    public static bool RandomTrueByPercent(float fPercentage)
    {
        return Random.Range(0, 100) < fPercentage;
    }

    public static bool RandomTrueByFraction(float fFraction)
    {
        return Random.Range(0f, 1f) < fFraction;
    }

    public static int GetRandomIndex(List<float> fProbList)
    {
        if (fProbList == null || fProbList.Count == 0)
        {
            Debug.LogError("RandomUtil: GetRandomIndex() - Invalid Probability List");
            return -1;
        }

        float fTotal = fProbList.Sum();
        var fRandom = Random.Range(0, fTotal);

        for (int i = 0; i < fProbList.Count; i++)
        {
            if (fRandom < fProbList[i])
            {
                return i;
            }
            fRandom -= fProbList[i];
        }

        // this should not happen
        return -1;
    }

    public static int GetRandomIndex(List<int> iProbList)
    {
        if (iProbList == null || iProbList.Count == 0)
        {
            Debug.LogError("RandomUtil::" +
                           "GetRandomIndex() - Invalid Probability List");
            return -1;
        }

        int iTotal = iProbList.Sum();
        var iRandom = Random.Range(0, iTotal);

        for (int i = 0; i < iProbList.Count; i++)
        {
            if (iRandom < iProbList[i])
            {
                return i;
            }
            iRandom -= iProbList[i];
        }

        // this should not happen
        return -1;
    }

    // startNum ~ endNum 숫자 중 하나의 랜덤 숫자를 가져옴
    public static int GetRandomIndex(int startNum, int endNum)
    {
        var randomValue = Random.Range(startNum, endNum + 1);
        return randomValue;
    }

    // startNum ~ endNum 숫자 중 하나의 랜덤 숫자를 가져옴
    public static float GetRandomfloat(float startNum, float endNum)
    {
        var randomValue = Random.Range(startNum, endNum);
        return randomValue;
    }
}
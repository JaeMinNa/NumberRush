using NUnit.Framework;
using System.Collections.Generic;

enum DataBaseKey
{
    User,

    //Social,
    //Data,
    //Manage,
    //Coupon,
    //ConnectReward,
    //Log,
    //Analytics,

    Max
}

public enum UserCollection
{
    None,

    UserCommonData,         // 유저 데이터
    UserNumberData,         // 유저 숫자 데이터
    UserGameData,           // 유저 게임 데이터

    Max
}

// 랭크 데이터
public class UserRankInfo
{
    public int Rank = 0;
    public string NickName = string.Empty;
    public int Score = 0;
    public float Time = 0f;
    public List<int> EquipNumber = new List<int>();
    public string ImageNum = string.Empty;
}
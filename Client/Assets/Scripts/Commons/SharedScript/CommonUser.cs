using NUnit.Framework;
using System.Collections.Generic;

public enum eUserData_Common
{
    AccountCode,        // ID 개념, 이 값으로 유저 데이터를 조회 한다.
    UID,                // 유저마다 가진 고유값, 필요 시 사용
    NickName,
    ImageNum,

    Max,
}

public enum eUserData_Number
{
    AccountCode,
    NumberInventory,        
    EquipNumber,              

    Max,
}

public enum eUserData_Game
{
    AccountCode,
    Score,
    Gold,
    Time,

    Max,
}

public class UserData_Common
{
    public string AccountCode = string.Empty;
    public string UID = string.Empty;
    public string NickName = string.Empty;
    public string ImageNum = string.Empty;
}

public class UserData_Number
{
    public string AccountCode = string.Empty;
    public List<int> NumberInventory = null;
    public List<int> EquipNumber = null;
}

public class UserData_Game
{
    public string AccountCode = string.Empty;
    public int Score = 0;
    public int Gold = 0;
    public float Time = 0f;
}
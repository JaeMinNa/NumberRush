public class User
{
    #region User Data
    // 유저의 기본 정보
    public static UserData_Common UserCommonData { get; set; } = null;

    // 유저의 숫자 정보
    public static UserData_Number UserNumberData { get; set; } = null;

    // 유저의 게임 정보
    public static UserData_Game UserGameData { get; set; } = null;
    #endregion

    #region Set
    public static void SetUserCommonData(UserData_Common Data)
    {
        UserCommonData = Data;
    }

    public static void SetUserNumberData(UserData_Number Data)
    {
        UserNumberData = Data;
    }

    public static void SetUserGameData(UserData_Game Data)
    {
        UserGameData = Data;
    }
    #endregion
}
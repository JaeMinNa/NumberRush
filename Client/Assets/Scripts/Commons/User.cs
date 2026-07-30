public class User
{
    #region User Data
    // 유저의 기본 정보
    public static UserData_Common UserCommonData { get; set; } = null;
    #endregion

    #region Set
    public static void SetUserCommonData(UserData_Common Data)
    {
        UserCommonData = Data;
    }
    #endregion
}
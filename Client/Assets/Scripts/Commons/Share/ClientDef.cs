using UnityEngine.Events;

public static class ClientDef
{
    public static readonly int MAX_PARTYMEMBER = 3;

    #region Player Preference
    public static readonly string PomeSortOrderKey = "PomeSortOrder";
    public static readonly string LastTutorialGroupKey = "LastTutorialGroup";
    public static readonly string IsPomeSavedKey = "IsPomeSaved";
    public static readonly string PomeGameOptionKey = "PomeGameOption";
    #endregion
}

public class MessageData
{
    public PopupType Type;
    public string Title;
    public string Message;
    public UnityAction OkAction;
}

public enum PopupType
{
    None,

    OkOnly,
    OkCancel,

    Max
}

public enum eItemType
{
    None,

    Goods = 1,
    Consume = 2,
    Material = 3,
    Equip = 4,
    ProfileIcon = 5,

    Max
}

public enum eTutorialStep
{
    None,

    String_Welcome,
    String_Intro,
    String_PomeCheck_Start,
    Click_SubMenu,
}
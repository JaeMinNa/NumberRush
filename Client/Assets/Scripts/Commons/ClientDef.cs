using UnityEngine.Events;

public static class ClientDef
{
    // Game
    public static readonly int GAME_SKIPTOTALCOUNT = 5;
    public static readonly int GAME_DEFAULTHP = 5;
    public static readonly float GAME_MAX_DIFFICULTY_TIME = 300.0f; // 5분에 최대 난이도

    // Normal Block
    public static readonly int BLOCK_DEFAULTHP = 1;
    public static readonly int BLOCK_DEFAULTMAXNUM = 10;
    public static readonly int BLOCK_MAXNUM_LIMIT = 50;
    public static readonly float BLOCK_DEFAULTDOWNSPEED = 0.1f;
    public static readonly float BLOCK_DEFAULTHORIZONSPEED = 0.5f;
    public static readonly float BLOCK_DEFAULTROTATIONSPEED = 50f;
    public static readonly float BLOCK_MAXDOWNSPEED = 0.25f;
    public static readonly float BLOCK_MAXHORIZONSPEED = 1.25f;
    public static readonly float BLOCK_MAXROTATIONSPEED = 140.0f;
    public static readonly float BLOCK_DEFAULTMINSPAWNTIME = 5f;
    public static readonly float BLOCK_DEFAULTMAXSPAWNTIME = 10f;
    public static readonly float BLOCK_MINSPAWNTIME_LIMIT = 2.5f;
    public static readonly float BLOCK_MAXSPAWNTIME_LIMIT = 4.5f;
    public static readonly float BLOCK_SPAWN_X = 2.5f;
    public static readonly float BLOCK_SPAWN_Y = 5.5f;
    public static readonly float BLOCK_MOVE_X = 2.6f;
    public static readonly float BLOCK_DESTROY_Y = -3.7f;

    // Armor Block
    public static readonly float ARMORBLOCK_SCALE = 1.5f;

    // Ghost Block
    public static readonly float GHOSTBLOCK_INTERVAL = 0.5f;
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

public enum FormulaOperator
{
    Plus,
    Subtract,
    Multiply,
    Divide
}

[System.Flags]
public enum BlockType
{
    None = 0,
    Rotation = 1 << 0,  // 1
    Move = 1 << 1,  // 2
    Armor = 1 << 2,  // 4
    Ghost = 1 << 3,  // 8
}

public enum SlotType
{
    Normal,
    Add,
    Lock,
    Select,
    Equip,
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
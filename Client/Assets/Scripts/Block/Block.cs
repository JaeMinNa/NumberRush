using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float DownSpeed = 1.0f;
    [SerializeField] private float HorizontalSpeed = 1.5f;
    [SerializeField] private float RotationSpeed = 100.0f;
    [SerializeField] private float GhostInterval = 0.5f;
    [SerializeField] private float ArmorScale = 1.5f;

    [Header("Text")]
    [SerializeField] private TextMeshPro NumberText = null;

    #region Member Property
    ChapterModule m_ChapterModule = null;

    // Block
    private BlockType m_BlockType;
    private int m_Number;
    private int m_HP = 0;
    private Vector3 m_DefaultScale;

    // Rotation 방향 
    private int m_RotationDirection = 0;

    // Move 방향
    private int m_MoveDirection = 0;

    // Ghost
    private float m_GhostTimer;
    private bool m_IsTextVisible = true;
    #endregion

    #region Unity Method
    public void Awake()
    {
        if (m_ChapterModule == null)
            m_ChapterModule = BattleModule.Instance as ChapterModule;

        // 초기화
        m_DefaultScale = transform.localScale;
    }

    public void Start()
    {

    }

    public void Update()
    {
        // 기본 하강
        MoveDown();

        // Rotation
        if ((m_BlockType & BlockType.Rotation) != 0)
        {
            UpdateRotation();
        }

        // Move
        if ((m_BlockType & BlockType.Move) != 0)
        {
            UpdateMove();
        }

        // Armor
        if ((m_BlockType & BlockType.Armor) != 0)
        {
            UpdateArmor();
        }

        // Ghost
        if ((m_BlockType & BlockType.Ghost) != 0)
        {
            UpdateGhost();
        }

        // 화면 아래까지 내려가면 HP 감소 후 제거
        if (transform.position.y <= ClientDef.BLOCK_DESTROY_Y)
        {
            m_ChapterModule.OnBlockMissed();
            Destroy(gameObject);
            UIManager.Instance.Refresh();

            return;
        }
    }
    #endregion

    #region Get/Set
    public int GetHP()
    {
        return m_HP;
    }

    public int GetNumber()
    {
        return m_Number;
    }

    public BlockType GetBlockType()
    {
        return m_BlockType;
    }
    #endregion

    #region Public Method
    // 블록 타입과 숫자를 설정
    public void SetBlock(BlockType type, int number)
    {
        m_BlockType = type;
        m_Number = number;

        // 기본 HP
        m_HP = ClientDef.BLOCK_DEFAULTHP;

        // 기본 크기
        transform.localScale = m_DefaultScale;

        // 기본 숫자 표시
        if (NumberText != null)
        {
            NumberText.text = m_Number.ToString();
            NumberText.gameObject.SetActive(true);
        }

        // Rotation
        if ((m_BlockType & BlockType.Rotation) != 0)
        {
            SetRotationBlock();
        }

        // Move
        if ((m_BlockType & BlockType.Move) != 0)
        {
            SetMoveBlock();
        }

        // Armor
        if ((m_BlockType & BlockType.Armor) != 0)
        {
            SetArmorBlock();
        }

        // Ghost
        if ((m_BlockType & BlockType.Ghost) != 0)
        {
            SetGhostBlock();
        }

        Debug.Log($"Block 설정 / Number:{m_Number} / Type:{m_BlockType} / HP:{m_HP}");
    }

    // 블록 숫자 변경
    public void SetNumber(int number)
    {
        m_Number = number;

        if (NumberText != null)
        {
            NumberText.text = m_Number.ToString();
        }
    }

    // 블록 데미지 처리
    public void Damage(int damage = 1)
    {
        m_HP -= damage;

        // Armor 블록은 HP가 기본 HP가 되는 순간 원래 크기로
        if ((m_BlockType & BlockType.Armor) != 0 && m_HP <= ClientDef.BLOCK_DEFAULTHP)
        {
            transform.localScale = m_DefaultScale;
        }

        // HP가 0이면 제거
        if (m_HP <= 0)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Member Method
    // 기본적으로 모두 아래로 이동
    private void MoveDown()
    {
        transform.position += Vector3.down * DownSpeed * Time.deltaTime;
    }

    private void SetRotationBlock()
    {
        // 좌/우 랜덤 회전
        m_RotationDirection = RandomUtil.GetRandomIndex(0, 1) < 0 ? -1 : 1;
    }

    private void UpdateRotation()
    {
        transform.Rotate(0.0f, 0.0f, RotationSpeed * m_RotationDirection * Time.deltaTime);
    }

    private void SetMoveBlock()
    {
        // 처음 이동 방향 좌/우 랜덤
        m_MoveDirection = RandomUtil.GetRandomIndex(0, 1) < 0 ? -1 : 1;
    }

    private void UpdateMove()
    {
        Vector3 position = transform.position;

        position.x += HorizontalSpeed * m_MoveDirection * Time.deltaTime;

        // 오른쪽 끝 도착
        if (position.x >= ClientDef.BLOCK_MOVE_X)
        {
            position.x = ClientDef.BLOCK_MOVE_X;
            m_MoveDirection = -1;
        }
        // 왼쪽 끝 도착
        else if (position.x <= -ClientDef.BLOCK_MOVE_X)
        {
            position.x = -ClientDef.BLOCK_MOVE_X;
            m_MoveDirection = 1;
        }

        transform.position = position;
    }

    private void SetArmorBlock()
    {
        // 기본 HP + 1
        m_HP = ClientDef.BLOCK_DEFAULTHP + 1;

        // 기본 크기의 1.5배
        transform.localScale = m_DefaultScale * ClientDef.ARMORBLOCK_SCALE;
    }

    private void UpdateArmor()
    {
        // HP가 다시 기본 HP가 되면 원래 크기로
        if (m_HP <= ClientDef.BLOCK_DEFAULTHP)
        {
            transform.localScale = m_DefaultScale;
        }
    }

    private void SetGhostBlock()
    {
        m_GhostTimer = 0.0f;
        m_IsTextVisible = true;

        if (NumberText != null)
        {
            NumberText.gameObject.SetActive(true);
        }
    }

    private void UpdateGhost()
    {
        if (NumberText == null)
            return;

        m_GhostTimer += Time.deltaTime;

        if (m_GhostTimer >= GhostInterval)
        {
            m_GhostTimer = 0.0f;

            m_IsTextVisible = !m_IsTextVisible;
            NumberText.gameObject.SetActive(m_IsTextVisible);
        }
    }
    #endregion
}
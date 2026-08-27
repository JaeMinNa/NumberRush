using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;


public class ChapterModule : BattleModule
{
    #region Public Property
    #endregion

    #region Member Property
    // 장착 숫자
    private List<int> m_EquipNumber = new List<int>();

    // 랜덤으로 선택된 연산자
    private List<FormulaOperator> m_Operators = new List<FormulaOperator>();

    // 유저가 입력한 숫자
    private List<int> m_InputNumbers = new List<int>();

    // 현재 필요한 숫자 개수
    private int m_RequiredNumberCount => m_Operators.Count + 1;

    // 스킵
    private int m_SkipTotalCount = 0;
    private int m_SkipNowCount = 0;

    private int m_Hp = 0;
    private int m_Score = 0;
    private float m_Time = 0f;

    private GameObject m_BlockPrefab;
    private Transform m_BlockRoot;

    // 다음 블록 생성까지 남은 시간
    private float m_BlockSpawnTimer = 0f;

    // 다음 블록 생성 시간
    private float m_NextBlockSpawnTime = 0f;
    #endregion

    #region Get / Set
    public int OperatorCount => m_Operators.Count;
    public int InputNumberCount => m_InputNumbers.Count;
    public int SkipTotalCount { get => m_SkipTotalCount; }
    public int SkipNowCount { get => m_SkipNowCount; set => m_SkipNowCount = value; }
    public int CurHp { get => m_Hp; set => m_Hp = value; }
    public int Score { get => m_Score; set => m_Score = value;  }
    public float CurTime { get => m_Time; }
    public List<int> EquipNumbers { get => m_EquipNumber; }
    #endregion

    #region Overrid Method
    public async override UniTask StartGame()
    {
        base.StartGame();

        InitChapter();
        SetFormula();
    }

    protected override void EndGame()
    {
        base.EndGame();
    }
    #endregion

    #region Public Method
    public void SetBlockRoot(Transform root)
    {
        m_BlockRoot = root;
    }

    public void SetFormula()
    {
        m_Operators.Clear();

        ClearInputNumbers();

        int operatorCount = RandomUtil.GetRandomIndex(1, 3);

        // 임시로 하나의 연산자만 선택
        operatorCount = 1;

        for (int i = 0; i < operatorCount; ++i)
        {
            FormulaOperator randomOperator = (FormulaOperator)Random.Range(0, 4);
            m_Operators.Add(randomOperator);
        }

        DebugFormula();
    }

    public void AddNumber(int number)
    {
        if (m_InputNumbers.Count >= m_RequiredNumberCount)
        {
            Debug.LogWarning("이미 필요한 숫자를 모두 입력했습니다.");
            return;
        }

        m_InputNumbers.Add(number);

        Debug.LogWarning($"숫자 입력 : {number}");
        Debug.LogWarning($"입력 개수 : {m_InputNumbers.Count} / {m_RequiredNumberCount}");
    }

    // Equal 버튼 클릭
    public void Calculate()
    {
        // 숫자가 모두 입력되지 않았으면 아무것도 하지 않음
        if (m_InputNumbers.Count != m_RequiredNumberCount)
        {
            return;
        }

        // 계산
        float result = CalculateFormula();

        Debug.LogWarning($"최종 결과 : {result}");

        // 현재 생성된 Block 중 결과값과 같은 숫자가 있는지 확인
        CheckBlockResult(result);

        // 계산이 끝났으면 입력 숫자 초기화
        ClearInputNumbers();
    }

    public int GetInputNumber(int index)
    {
        if (index < 0 || index >= m_InputNumbers.Count)
            return 0;

        return m_InputNumbers[index];
    }

    public FormulaOperator GetOperator(int index)
    {
        if (index < 0 || index >= m_Operators.Count)
            return default;

        return m_Operators[index];
    }

    public string GetOperatorText(int index)
    {
        if (index < 0 || index >= m_Operators.Count)
            return "";

        return GetOperatorString(m_Operators[index]);
    }

    public void ClearInputNumbers()
    {
        m_InputNumbers.Clear();

        Debug.LogWarning("입력 숫자 초기화");
    }

    public void OnBlockMissed()
    {
        // 이미 GameOver라면 처리 안 함
        if (m_Hp <= 0)
            return;

        m_Hp--;

        Debug.Log($"Block 놓침 / HP : {m_Hp}");

        if (m_Hp <= 0)
        {
            m_Hp = 0;

            Debug.LogError("GameOver");

            EndGame();
        }
    }
    #endregion

    #region Member Method
    // 초기화
    private void InitChapter()
    {
        m_SkipTotalCount = ClientDef.GAME_SKIPTOTALCOUNT;
        m_SkipNowCount = ClientDef.GAME_SKIPTOTALCOUNT;
        m_Hp = ClientDef.GAME_DEFAULTHP;
        m_Score = 0;
        m_Time = 0f;

        // Block Prefab 로드
        m_BlockPrefab = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/Game/Block/Block");

        // 장착 숫자 
        m_EquipNumber = User.UserNumberData.EquipNumber;

        m_EquipNumber.Add(10);
        m_EquipNumber.Add(15);
        m_EquipNumber.Add(0);

        // 첫 블록 생성 시간 결정
        ResetBlockSpawnTime();
    }

    // 사칙 연산
    private float CalculateFormula()
    {
        List<float> numbers = new List<float>();

        for (int i = 0; i < m_InputNumbers.Count; ++i)
        {
            numbers.Add(m_InputNumbers[i]);
        }

        List<FormulaOperator> tempOperators = new List<FormulaOperator>(m_Operators);

        // 1. 곱하기 / 나누기 먼저 처리
        for (int i = 0; i < tempOperators.Count;)
        {
            FormulaOperator op = tempOperators[i];

            if (op == FormulaOperator.Multiply ||
                op == FormulaOperator.Divide)
            {
                float left = numbers[i];
                float right = numbers[i + 1];

                float value = 0;

                if (op == FormulaOperator.Multiply)
                {
                    value = left * right;
                }
                else
                {
                    if (right == 0)
                    {
                        Debug.LogError("0으로 나눌 수 없습니다.");
                        return 0;
                    }

                    value = left / right;
                }

                numbers[i] = value;
                numbers.RemoveAt(i + 1);

                tempOperators.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }

        // 2. 더하기 / 빼기 처리
        float result = numbers[0];

        for (int i = 0; i < tempOperators.Count; ++i)
        {
            switch (tempOperators[i])
            {
                case FormulaOperator.Plus:
                    result += numbers[i + 1];
                    break;

                case FormulaOperator.Subtract:
                    result -= numbers[i + 1];
                    break;
            }
        }

        return result;
    }


    // 현재 생성된 식 확인용
    private void DebugFormula()
    {
        string formula = "";

        foreach (FormulaOperator op in m_Operators)
        {
            formula += GetOperatorString(op) + " ";
        }

        Debug.Log($"선택된 연산자 : {formula}");
        Debug.Log($"필요한 숫자 개수 : {m_RequiredNumberCount}");
    }


    private string GetOperatorString(FormulaOperator op)
    {
        switch (op)
        {
            case FormulaOperator.Plus:
                return "+";

            case FormulaOperator.Subtract:
                return "-";

            case FormulaOperator.Multiply:
                return "×";

            case FormulaOperator.Divide:
                return "÷";

            default:
                return "";
        }
    }

    private void ResetBlockSpawnTime()
    {
        m_BlockSpawnTimer = 0f;
        m_NextBlockSpawnTime = RandomUtil.GetRandomfloat(ClientDef.BLOCK_MINSPAWNTIME, ClientDef.BLOCK_MAXSPAWNTIME);
    }

    private void SpawnBlock()
    {
        if (m_BlockPrefab == null)
            return;

        // 스폰 위치 설정
        float randomX = Random.Range(-ClientDef.BLOCK_SPAWN_X, ClientDef.BLOCK_SPAWN_X);
        Vector3 spawnPosition = new Vector3(randomX, ClientDef.BLOCK_SPAWN_Y, 0f);

        // Block 생성
        GameObject blockObject = Instantiate(m_BlockPrefab, spawnPosition, Quaternion.identity, m_BlockRoot);

        Block block = blockObject.GetComponent<Block>();
        if (block == null)
        {
            Destroy(blockObject);
            return;
        }

        //랜덤 숫자
        int randomNumber = RandomUtil.GetRandomIndex(1, ClientDef.BLOCK_MAXNUM);

        // 랜덤 Block Type
        BlockType randomType = GetRandomBlockType();

        // Block 설정
        block.SetBlock(randomType, randomNumber);

        Debug.Log($"Block 생성 / Number:{randomNumber} / Type:{randomType}");
    }

    private BlockType GetRandomBlockType()
    {
        BlockType[] blockTypes =
        {
            BlockType.Rotation,
            BlockType.Move,
            BlockType.Armor,
            BlockType.Ghost
        };

        // 첫 번째 타입 랜덤
        int firstIndex = RandomUtil.GetRandomIndex(0, blockTypes.Length - 1);

        BlockType result = blockTypes[firstIndex];

        // 90% : 하나만
        // 10% : 두 개
        if (RandomUtil.GetRandomIndex(1, 100) <= 10)
        {
            int secondIndex;

            do
            {
                secondIndex = RandomUtil.GetRandomIndex(0,blockTypes.Length - 1);
            }
            while (secondIndex == firstIndex);

            // 두 Type 합치기
            result |= blockTypes[secondIndex];
        }

        return result;
    }

    private void CheckBlockResult(float result)
    {
        Block[] blocks = FindObjectsByType<Block>(FindObjectsSortMode.None);

        foreach (Block block in blocks)
        {
            if (block == null)
                continue;

            int blockNumber = block.GetNumber();

            // 계산 결과와 Block 숫자가 동일한지 확인
            if (Mathf.Approximately(result, blockNumber))
            {
                // 블록에 데미지
                block.Damage();

                // 아직 HP가 남아있으면 새로운 숫자로 변경
                if (block.GetHP() > 0)
                {
                    int newNumber = RandomUtil.GetRandomIndex(1, ClientDef.BLOCK_MAXNUM);
                    block.SetNumber(newNumber);
                }

                AddScore(100);

                // 계산 공식 변경
                SetFormula();

                return;
            }
        }
    }

    private void AddScore(int score)
    {
        m_Score += score;

        UIManager.Instance.Refresh();
    }
    #endregion

    #region Unity Method
    private void Update()
    {
        m_Time += Time.deltaTime;

        // GameOver 상태면 블록 생성 중지
        if (m_Hp <= 0)
            return;

        // Block 생성 시간 체크
        m_BlockSpawnTimer += Time.deltaTime;

        if (m_BlockSpawnTimer >= m_NextBlockSpawnTime)
        {
            SpawnBlock();

            // 블록 생성 시간 랜덤 결정
            ResetBlockSpawnTime();
        }
    }
    #endregion
}

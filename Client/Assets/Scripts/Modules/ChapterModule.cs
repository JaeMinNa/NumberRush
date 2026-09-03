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
    private int m_Gold = 0;
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
    public int Gold { get => m_Gold; set => m_Gold = value; }

    public float CurTime { get => m_Time; }
    public List<int> EquipNumbers { get => m_EquipNumber; }

    // 현재 난이도 비율 (0.0 ~ 1.0)
    public float DifficultyRate
    {
        get
        {
            return Mathf.Clamp01(m_Time / ClientDef.GAME_MAX_DIFFICULTY_TIME);
        }
    }

    // 현재 블록 하강 속도
    public float GetBlockDownSpeed()
    {
        return Mathf.Lerp(
            ClientDef.BLOCK_DEFAULTDOWNSPEED,
            ClientDef.BLOCK_MAXDOWNSPEED,
            DifficultyRate);
    }

    // 현재 블록 좌우 이동 속도
    public float GetBlockHorizonSpeed()
    {
        return Mathf.Lerp(
            ClientDef.BLOCK_DEFAULTHORIZONSPEED,
            ClientDef.BLOCK_MAXHORIZONSPEED,
            DifficultyRate);
    }

    // 현재 블록 회전 속도
    public float GetBlockRotationSpeed()
    {
        return Mathf.Lerp(
            ClientDef.BLOCK_DEFAULTROTATIONSPEED,
            ClientDef.BLOCK_MAXROTATIONSPEED,
            DifficultyRate);
    }

    // 현재 블록 최대 숫자
    public int GetBlockMaxNumber()
    {
        return Mathf.RoundToInt(
            Mathf.Lerp(
                ClientDef.BLOCK_DEFAULTMAXNUM,
                ClientDef.BLOCK_MAXNUM_LIMIT,
                DifficultyRate));
    }
    #endregion

    #region Overrid Method
    public async override UniTask StartGame()
    {
        base.StartGame();

        InitChapter();
        SetFormula();
    }

    public override void EndGame()
    {
        base.EndGame();
    }

    public async override UniTask RestartGame()
    {
        await base.RestartGame();

        // 기존 블록 제거
        Block[] blocks = FindObjectsByType<Block>(FindObjectsSortMode.None);

        foreach (Block block in blocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }

        // 챕터 데이터 초기화
        InitChapter();

        // 새로운 공식 생성
        SetFormula();

        // UI 갱신
        UIManager.Instance.Refresh();
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

        // 시간에 따른 연산자 개수 결정
        int operatorCount = GetRandomOperatorCount();

        for (int i = 0; i < operatorCount; ++i)
        {
            FormulaOperator randomOperator = (FormulaOperator)RandomUtil.GetRandomIndex(0, 3);
            m_Operators.Add(randomOperator);
        }

        DebugFormula();
    }

    private int GetRandomOperatorCount()
    {
        float randomValue = RandomUtil.GetRandomfloat(0f, 100f);

        // 0 ~ 60초
        // 숫자 2개 : 100%
        if (m_Time < 60f)
        {
            return 1;
        }

        // 60 ~ 120초
        // 숫자 2개 : 85%
        // 숫자 3개 : 15%
        if (m_Time < 120f)
        {
            return randomValue < 85f ? 1 : 2;
        }

        // 120 ~ 180초
        // 숫자 2개 : 70%
        // 숫자 3개 : 25%
        // 숫자 4개 : 5%
        if (m_Time < 180f)
        {
            if (randomValue < 70f)
                return 1;

            if (randomValue < 95f)
                return 2;

            return 3;
        }

        // 180 ~ 240초
        // 숫자 2개 : 55%
        // 숫자 3개 : 35%
        // 숫자 4개 : 10%
        if (m_Time < 240f)
        {
            if (randomValue < 55f)
                return 1;

            if (randomValue < 90f)
                return 2;

            return 3;
        }

        // 240초 이후
        // 숫자 2개 : 40%
        // 숫자 3개 : 40%
        // 숫자 4개 : 20%
        if (randomValue < 40f)
            return 1;

        if (randomValue < 80f)
            return 2;

        return 3;
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
            SoundManager.Instance.StartSFX("MissButton");
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

        SoundManager.Instance.StartSFX("DestroyLine");

        m_Hp--;

        Debug.Log($"Block 놓침 / HP : {m_Hp}");

        if (m_Hp <= 0)
        {
            m_Hp = 0;

            Debug.LogError("GameOver");

            SetPause(true);
            UIManager.Instance.Open<Popup_EndGame>(UI.Popup, "Prefabs/UI/Popup/Popup_EndGame");
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
        m_Gold = 0;
        m_Time = 0f;

        // Block Prefab 로드
        m_BlockPrefab = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/Game/Block/Block");

        // 장착 숫자 
        m_EquipNumber = User.UserNumberData.EquipNumber;

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

        float minSpawnTime = Mathf.Lerp(ClientDef.BLOCK_DEFAULTMINSPAWNTIME, ClientDef.BLOCK_MINSPAWNTIME_LIMIT, DifficultyRate);
        float maxSpawnTime = Mathf.Lerp(ClientDef.BLOCK_DEFAULTMAXSPAWNTIME, ClientDef.BLOCK_MAXSPAWNTIME_LIMIT, DifficultyRate);

        m_NextBlockSpawnTime = RandomUtil.GetRandomfloat(minSpawnTime, maxSpawnTime);

    }

    private void SpawnBlock()
    {
        if (m_BlockPrefab == null)
            return;

        // 스폰 위치 설정
        float randomX = RandomUtil.GetRandomfloat(-ClientDef.BLOCK_SPAWN_X, ClientDef.BLOCK_SPAWN_X);
        Vector3 spawnPosition = new Vector3(randomX, ClientDef.BLOCK_SPAWN_Y, 0f);

        // Block 생성
        GameObject blockObject = Instantiate(m_BlockPrefab, spawnPosition, Quaternion.identity, m_BlockRoot);

        Block block = blockObject.GetComponent<Block>();
        if (block == null)
        {
            Destroy(blockObject);
            return;
        }

        // 랜덤 숫자
        int maxNumber = GetBlockMaxNumber();
        int randomNumber = RandomUtil.GetRandomIndex(1, maxNumber);

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

        // 0 ~ 99 중 하나
        int randomValue = RandomUtil.GetRandomIndex(0, 99);

        // 일반 블록 : 40%
        if (randomValue < 40)
        {
            return BlockType.None;
        }

        // 특성 개수 결정
        // 40 ~ 84 : 특성 1개 = 45%
        // 85 ~ 94 : 특성 2개 = 10%
        // 95 ~ 99 : 특성 3개 = 5%
        int typeCount;

        if (randomValue < 85)
        {
            typeCount = 1;
        }
        else if (randomValue < 95)
        {
            typeCount = 2;
        }
        else
        {
            typeCount = 3;
        }

        BlockType result = BlockType.None;
        List<int> selectedIndexes = new List<int>();

        while (selectedIndexes.Count < typeCount)
        {
            int randomIndex =
                RandomUtil.GetRandomIndex(0, blockTypes.Length - 1);

            // 이미 선택한 타입이면 다시 뽑기
            if (selectedIndexes.Contains(randomIndex))
                continue;

            selectedIndexes.Add(randomIndex);

            // BlockType 추가
            result |= blockTypes[randomIndex];
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
                SoundManager.Instance.StartSFX("SuccessButton");

                // 블록에 데미지
                block.Damage();

                // 아직 HP가 남아있으면 새로운 숫자로 변경
                if (block.GetHP() > 0)
                {
                    int newNumber = RandomUtil.GetRandomIndex(1, GetBlockMaxNumber());
                    block.SetNumber(newNumber);
                }

                int rewardScore = GetRewardScore();
                int rewardGold = GetRewardGold();

                AddScore(rewardScore);
                AddGold(rewardGold);

                // 계산 공식 변경
                SetFormula();

                return;
            }
        }

        SoundManager.Instance.StartSFX("FailButton");
    }

    private void AddScore(int score)
    {
        m_Score += score;

        UIManager.Instance.Refresh();
    }

    private void AddGold(int gold)
    {
        m_Gold += gold;
        UIManager.Instance.Refresh();
    }

    private float GetEquipNumberAverage()
    {
        if (m_EquipNumber == null || m_EquipNumber.Count == 0)
            return 0f;

        int total = 0;

        foreach (int number in m_EquipNumber)
        {
            total += number;
        }

        return (float)total / m_EquipNumber.Count;
    }

    private int GetRewardScore()
    {
        float equipAverage = GetEquipNumberAverage();

        // 기본 점수
        int baseScore = 100;

        // 시간 보너스
        // 30초마다 +10
        int timeBonus = Mathf.FloorToInt(m_Time / 30f) * 10;

        // 장착 숫자 보너스
        int equipBonus = Mathf.RoundToInt(equipAverage * 1.5f);

        return baseScore + timeBonus + equipBonus;
    }

    private int GetRewardGold()
    {
        float equipAverage = GetEquipNumberAverage();

        // 기본 골드
        int baseGold = 20;

        // 시간 보너스
        // 30초마다 +3
        int timeBonus = Mathf.FloorToInt(m_Time / 30f) * 3;

        // 장착 숫자 보너스
        int equipBonus = Mathf.RoundToInt(equipAverage * 0.4f);

        return baseGold + timeBonus + equipBonus;
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

using UnityEngine;

public class GameScene : MonoBehaviour
{
    [SerializeField] private GameObject Root_Camera = null;
    [SerializeField] private GameObject Root_Environment = null;
    [SerializeField] private GameObject Root_UI = null;
    [SerializeField] private GameObject Root_Character = null;

    private async void Start()
    {
        // Module 생성
        BattleModule.CreateModule<ChapterModule>();
        BattleModule.Instance.SetRootObject(Root_Camera, Root_Environment, Root_Character);

        ChapterModule chapterModule = BattleModule.Instance as ChapterModule;
        chapterModule.SetBlockRoot(Root_Character.transform);

        await BattleModule.Instance.StartGame();

        // BackGround 생성
        GameObject backgroundPrefab = ResourceLoader.LoadAssetResources<GameObject>("Prefabs/Game/Background/Background");
        Instantiate(backgroundPrefab, Root_Environment.transform);

        // IngameWindow 생성
        UIManager.Instance.SetUIRoot(Root_UI);
        UIManager.Instance.SetActiveRoot(UI.BackGround, false);
        UIManager.Instance.Open<IngameWindow>(UI.Main, "Prefabs/UI/Window/IngameWindow");
    }
}
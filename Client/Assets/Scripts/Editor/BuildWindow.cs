using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build;

public class BuildWindow : EditorWindow
{
    private const string KEY_BUILDPATH = "BUILD_OUTPUT_PATH";

    private ServerType m_ServerType;
    private bool IsDebug = true;
    private bool isAab = false;
    private string ProductVersion = "1.0.0";
    private int BundleCode = 1;
    string OutputPath = string.Empty;

    private GUILayoutOption[] LabelOption = { GUILayout.MinWidth(10f), GUILayout.MaxWidth(100) };
    private GUILayoutOption[] OtherOption = { GUILayout.MinWidth(10f), GUILayout.MaxWidth(150f) };

    private Builder BundleBuilder = new Builder();
    private Vector2 ScrollBar;

    [MenuItem("JaeMin/BuildWindow")]
    public static void Open()
    {
        var window = GetWindow<BuildWindow>();
        window.Init();
    }

    private void Init()
    {
        // 플레이어 세팅
        ProductVersion = PlayerSettings.bundleVersion;
        BundleCode = PlayerSettings.Android.bundleVersionCode;

        // 게임 설정 파일
        var config = ResourceLoader.LoadAssetResources<GameConfig>("Data/GameConfig");
        if (config == null)
        {
            config = CreateInstance<GameConfig>();
            config.SetConfig(ResourceType.Bundle);

            AssetDatabase.CreateAsset(config, "Assets/Resources/Data/GameConfig.asset");
        }

        OutputPath = EditorPrefs.GetString(KEY_BUILDPATH, string.Empty);
    }

    private void Reload()
    {
        // 플레이어 세팅
        ProductVersion = PlayerSettings.bundleVersion;
        BundleCode = PlayerSettings.Android.bundleVersionCode;

        var config = ResourceLoader.LoadAssetResources<GameConfig>("Data/GameConfig");
        if (config == null)
        {
            config = CreateInstance<GameConfig>();
            config.SetConfig(ResourceType.Bundle);

            AssetDatabase.CreateAsset(config, "Assets/Resources/Data/GameConfig.asset");
        }

        OutputPath = EditorPrefs.GetString(KEY_BUILDPATH, string.Empty);

        ShowNotification(new GUIContent("Reload"));
    }

    private void SaveSetting()
    {
        EditorPrefs.SetString(KEY_BUILDPATH, OutputPath);
        PlayerSettings.bundleVersion = ProductVersion;
        PlayerSettings.Android.bundleVersionCode = BundleCode;

        List<string> Defines = new List<string>();
        Defines.Add("AMPLIFY_SHADER_EDITOR");
        Defines.Add("UNITASK_DOTWEEN_SUPPORT");
        Defines.Add("BESTHTTP_SIGNALR_CORE_ENABLE_MESSAGEPACK_CSHARP");

        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, Defines.ToArray());

        GameConfig m_GameConfig = CreateInstance<GameConfig>();
        m_GameConfig.SetConfig(ResourceType.Bundle);
        AssetDatabase.CreateAsset(m_GameConfig, "Assets/Resources/Data/GameConfig.asset");

        AssetDatabase.Refresh();
    }

    private void BuildGame()
    {
        // Common Setting
        EditorPrefs.SetString(KEY_BUILDPATH, OutputPath);

        // 빌드 버전(1.0.0)
        PlayerSettings.bundleVersion = ProductVersion;
        PlayerSettings.Android.bundleVersionCode = BundleCode;
        PlayerSettings.applicationIdentifier = GetPackageName();

        AssetDatabase.Refresh();

        List<string> Defines = new List<string>();
        Defines.Add("AMPLIFY_SHADER_EDITOR");
        Defines.Add("UNITASK_DOTWEEN_SUPPORT");
        Defines.Add("BESTHTTP_SIGNALR_CORE_ENABLE_MESSAGEPACK_CSHARP");

        // 프로젝트 빌드 설정
        PlayerSettings.Android.keystorePass = "15941594";
        PlayerSettings.Android.keyaliasName = "number rush";
        PlayerSettings.Android.keyaliasPass = "15941594";

        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, Defines.ToArray());
        EditorUserBuildSettings.buildAppBundle = isAab;

        // GameConfig 불러오기
        GameConfig m_GameConfig = ResourceLoader.LoadAssetResources<GameConfig>("Data/GameConfig");
        if (m_GameConfig == null)
        {
            m_GameConfig = CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(m_GameConfig, "Assets/Resources/Data/GameConfig.asset");
        }

        // 빌드 설정 저장
        m_GameConfig.SetConfig(ResourceType.Bundle);
        m_GameConfig.SetServerType(m_ServerType);

        // ScriptableObject 변경사항 저장
        EditorUtility.SetDirty(m_GameConfig);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[BUILD CONFIG] ServerType = {m_ServerType} / " +
            $"ResourceType = {ResourceType.Bundle} / " +
            $"BundleCode = {BundleCode}"
        );

        var Scenes = EditorBuildSettings.scenes;
        List<string> Levels = new List<string>();
        for (int count = 0; count < Scenes.Length; ++count)
            Levels.Add(Scenes[count].path);

        var BuildOption = BuildOptions.CompressWithLz4HC;
        if (IsDebug)
            BuildOption = BuildOption | BuildOptions.Development;

        BuildPipeline.BuildPlayer(Levels.ToArray(), System.IO.Path.Combine(OutputPath, MakeBuildName()), EditorUserBuildSettings.activeBuildTarget, BuildOption);
        AssetDatabase.Refresh();
        EditorUtility.RevealInFinder(OutputPath);
    }

    private string GetPackageName()
    {
        string packageName = string.Empty;
        packageName = "com.JaeMinsGame.NumberRush";

        return packageName;
    }

    private string MakeBuildName()
    {
        string packageName = GetPackageName();
        string extension = string.Empty;
        extension = ".apk";

        if (IsDebug)
            return $"{packageName}_{ProductVersion}({BundleCode})_{m_ServerType}_Debug{extension}";
        else
            return $"{packageName}_{ProductVersion}({BundleCode})_{m_ServerType}_Release{extension}";
    }

    #region Bundle
    private void BuildBundle()
    {
        if (string.IsNullOrEmpty(OutputPath))
        {
            Debug.LogError("[AssetBundle] Build Path가 설정되지 않았습니다.");
            return;
        }

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.LogError(
                $"[AssetBundle] 현재 Build Target이 Android가 아닙니다. " +
                $"현재 Target : {EditorUserBuildSettings.activeBuildTarget}"
            );

            return;
        }

        EditorPrefs.SetString(KEY_BUILDPATH, OutputPath);

        string bundleOutputPath = MakeBundleOutPut();

        Debug.Log($"[AssetBundle] 최종 Bundle 출력 경로 : {bundleOutputPath}");

        BundleBuilder.MakeAllBundles("Assets/ResourcesBundle", bundleOutputPath, EditorUserBuildSettings.activeBuildTarget);

        if (Directory.Exists(bundleOutputPath))
        {
            EditorUtility.RevealInFinder(bundleOutputPath);
        }
    }

    private string MakeBundleOutPut()
    {
        return $"{OutputPath}/AssetBundle/{ProductVersion}/{EditorUserBuildSettings.activeBuildTarget}";
    }

    private void SignBundleName()
    {
        string[] allAssetName = AssetDatabase.GetAllAssetBundleNames();

        for (int count = 0; count < allAssetName.Length; ++count)
        {
            AssetDatabase.RemoveAssetBundleName(allAssetName[count], true);
        }

        BundleBuilder.AssignAssetBundleName("Assets/ResourcesBundle");

        ShowNotification(new GUIContent("Set BundleName"));
    }

    private void RemoveBundleName()
    {
        string[] allAssetName = AssetDatabase.GetAllAssetBundleNames();

        for (int count = 0; count < allAssetName.Length; ++count)
        {
            string[] Assets = AssetDatabase.GetAssetPathsFromAssetBundle(allAssetName[count]);
            for (int lcount = 0; lcount < Assets.Length; ++lcount)
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(Assets[lcount]);
                assetImporter.SetAssetBundleNameAndVariant(string.Empty, string.Empty);
            }
        }

        for (int count = 0; count < allAssetName.Length; ++count)
            AssetDatabase.RemoveAssetBundleName(allAssetName[count], true);

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.SaveAssets();

        ShowNotification(new GUIContent("Remove BundleName"));
    }
    #endregion

    #region Display
    private void OnGUI()
    {
        ScrollBar = GUILayout.BeginScrollView(ScrollBar);
        DisplayGameConfig();
        DisplayBundleConfig();
        GUILayout.EndScrollView();
    }

    private void DisplayGameConfig()
    {
        // 타이틀
        GUILayout.BeginHorizontal();
        GUILayout.Label("Build Setting", EditorStyles.boldLabel, LabelOption);
        if (GUILayout.Button("Load", OtherOption))
        {
            Reload();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("", EditorStyles.boldLabel, LabelOption);
        if (GUILayout.Button("Save", OtherOption))
        {
            SaveSetting();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Service", EditorStyles.label, LabelOption);
        m_ServerType = (ServerType)EditorGUILayout.EnumPopup(m_ServerType, OtherOption);
        GUILayout.EndHorizontal();

        // 디버그 모드 - On, Off
        GUILayout.BeginHorizontal();
        GUILayout.Label("Develop Build", EditorStyles.label, LabelOption);
        IsDebug = GUILayout.Toggle(IsDebug, string.Empty, OtherOption);
        GUILayout.EndHorizontal();

        // AAB
        //if (m_platformType == PlatformType.Google)
        //{
        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("App Bundle", EditorStyles.label, LabelOption);
        //    isAab = GUILayout.Toggle(isAab, string.Empty, OtherOption);
        //    GUILayout.EndHorizontal();
        //}

        // 제품 버전 - 1.0.0
        GUILayout.BeginHorizontal();
        GUILayout.Label("Build Ver", EditorStyles.label, LabelOption);
        ProductVersion = GUILayout.TextField(ProductVersion, OtherOption);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Bundle Code", EditorStyles.label, LabelOption);
        BundleCode = EditorGUILayout.IntField(BundleCode, OtherOption);
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // 아웃풋 경로
        GUILayout.BeginHorizontal();
        GUILayout.Label("Build Path", EditorStyles.label, LabelOption);
        GUILayout.BeginVertical();
        GUILayout.TextArea(OutputPath, EditorStyles.textArea, OtherOption);
        if (GUILayout.Button("Select", OtherOption))
        {
            OutputPath = EditorUtility.SaveFolderPanel("Choose folder", "", "");

            if (string.IsNullOrEmpty(OutputPath) == false)
                EditorPrefs.SetString(KEY_BUILDPATH, OutputPath);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Path Open", EditorStyles.label, LabelOption);
        if (GUILayout.Button("Open", OtherOption))
        {
            EditorUtility.RevealInFinder(OutputPath);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // 파일 이름
        GUILayout.BeginHorizontal();
        GUILayout.Label("Output", EditorStyles.label, LabelOption);

        GUILayout.TextArea(MakeBuildName(), EditorStyles.textArea, OtherOption);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Build", EditorStyles.label, LabelOption);

        if (GUILayout.Button("Run", OtherOption))
        {
            if (EditorUtility.DisplayDialog("Build", "Run build?", "Ok", "Cancel"))
            {
                BuildGame();
                GUIUtility.ExitGUI();
                return;
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DisplayBundleConfig()
    {
        GUILayout.Space(20);

        GUILayout.Label("Patch Setting", EditorStyles.boldLabel, LabelOption);

        // 번들 이름 지정
        GUILayout.BeginHorizontal();
        GUILayout.Label("Bundle Name", EditorStyles.label, LabelOption);

        GUILayout.BeginVertical();
        if (GUILayout.Button("Save", OtherOption))
        {
            SignBundleName();
            GUIUtility.ExitGUI();
            return;
        }

        if (GUILayout.Button("Remove", OtherOption))
        {
            RemoveBundleName();
            GUIUtility.ExitGUI();
            return;
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        // 번들 빌드
        GUILayout.BeginHorizontal();
        GUILayout.Label("Build", EditorStyles.label, LabelOption);
        if (GUILayout.Button("Run", OtherOption))
        {
            if (EditorUtility.DisplayDialog("Bundle Build", "Run Bundle Build?", "Ok", "Cancel"))
            {
                BuildBundle();
                GUIUtility.ExitGUI();
                return;
            }
        }
        GUILayout.EndHorizontal();
    }
    #endregion
}
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

interface IResourceLoader
{
    T LoadAsset<T>(string bundleName, string assetName) where T : Object;

    string LoadTextAsset(string bundleName, string assetName);

    UniTask<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : Object;

    UniTask UnloadAsset(bool isUnload);
}

public class ResourceLoader
{
    private static IResourceLoader m_Loader;

    private static IResourceLoader Loader
    {
        get
        {
            if (m_Loader == null)
                Init();

            return m_Loader;
        }
    }

    private static void Init()
    {
        m_Loader = new LocalResourceLoader();
    }

    public static T LoadAsset<T>(string bundleName, string assetName) where T : Object
    {
        return Loader.LoadAsset<T>(bundleName, assetName);
    }

    public static T LoadAssetResources<T>(string assetPath) where T : Object
    {
        return Resources.Load<T>(assetPath);
    }

    public static string LoadTextAsset(string bundleName, string assetName)
    {
        return Loader.LoadTextAsset(bundleName, assetName);
    }

    public static async UniTask<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : Object
    {
        return await Loader.LoadAssetAsync<T>(bundleName, assetName);
    }

    public static async UniTask<T> LoadResourcesAsync<T>(string assetPath) where T : Object
    {
        var Asset = await Resources.LoadAsync<T>(assetPath);
        return Asset as T;
    }
}

class LocalResourceLoader : IResourceLoader
{
    public T LoadAsset<T>(string bundleName, string assetName) where T : Object
    {
#if UNITY_EDITOR
        string[] assetPaths;
        string bundlePath = ($"{bundleName}.assetbundle").ToLower();
        assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundlePath, assetName);

        if (assetPaths.Length == 0)
        {
            Debug.LogError($"There is no asset with name {assetName} in {bundlePath}");
            return new Object() as T;
        }

        T obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPaths[0], typeof(T)) as T;
        if (obj == null)
        {
            Debug.LogError($"There is no asset with name {assetName} in {bundlePath}");
            return new Object() as T;
        }

        return obj;
#else
        return new Object() as T;
#endif
    }

    public string LoadTextAsset(string bundleName, string assetName)
    {
#if UNITY_EDITOR
        string[] assetPaths;
        string bundlePath = ($"{bundleName}.assetbundle").ToLower();
        assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundlePath, assetName);

        if (assetPaths.Length == 0)
        {
            Debug.LogError($"There is no asset with name {assetName} in {bundlePath}");
            return string.Empty;
        }

        TextAsset obj = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(assetPaths[0]);
        if (obj == null)
        {
            Debug.LogError($"There is no asset with name {assetName} in {bundlePath}");
            return string.Empty;
        }

        string retValue = obj.text;
        obj = null;

        return retValue;
#else
        return string.Empty;
#endif
    }

    public async UniTask<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : Object
    {
#if UNITY_EDITOR
        string[] assetPaths;
        string bundlePath = ($"{bundleName}.assetbundle").ToLower();
        assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundlePath, assetName);

        if (assetPaths.Length == 0)
        {
            Debug.LogError($"There is no asset with name {assetName} in {bundlePath}");
            return new Object() as T;
        }

        T obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPaths[0], typeof(T)) as T;
        if (obj == null)
        {
            Debug.LogError($"There is no asset with name {assetName} in {bundlePath}");
            return new Object() as T;
        }

        // 오디오 클립인 경우 로드 상태 확인
        if (obj is AudioClip audioClip)
        {
            await UniTask.WaitUntil(() => audioClip.loadState == AudioDataLoadState.Loaded);
        }

        return obj;
#else
        return new Object() as T;
#endif
    }

    public async UniTask UnloadAsset(bool isUnload)
    {
        await Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}

class DownloadResourceLoader : IResourceLoader
{
    private AssetBundle m_DependencyAsset = null;
    private AssetBundleManifest m_DependencyManifest = null;

    private Dictionary<string, AssetBundle> m_LoadedAsset = new Dictionary<string, AssetBundle>();
    private string m_BasePath = Path.Combine(Application.persistentDataPath, "AssetBundle");

    private AssetBundle LoadBundle(string bundleName)
    {
        return AssetBundle.LoadFromFile(Path.Combine(m_BasePath, bundleName));
    }

    private async UniTask<AssetBundle> LoadBundleAsync(string bundleName)
    {
        var Bundle = await AssetBundle.LoadFromFileAsync(Path.Combine(m_BasePath, bundleName));
        return Bundle;
    }

    private void LoadDependency(string bundleName)
    {
        if (m_DependencyAsset == null)
        {
            AssetBundle depAsset = null;
#if UNITY_ANDROID
            depAsset = AssetBundle.LoadFromFile(Path.Combine(m_BasePath, "Android.assetbundle"));
#elif UNITY_IOS
            depAsset = AssetBundle.LoadFromFile(Path.Combine(m_BasePath, "iOS.assetbundle"));
#elif UNITY_WEBGL
            depAsset = AssetBundle.LoadFromFile(Path.Combine(m_BasePath, "WebGL.assetbundle"));
#endif

            if (depAsset == null)
                return;

            m_DependencyAsset = depAsset;
        }

        if (m_DependencyAsset == null)
            return;

        if (m_DependencyAsset != null && m_DependencyManifest == null)
        {
            AssetBundleManifest Manifest = m_DependencyAsset.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (Manifest == null)
                return;

            m_DependencyManifest = Manifest;
        }

        if (m_DependencyManifest == null)
            return;

        string[] dependencies = m_DependencyManifest.GetAllDependencies(bundleName);
        foreach (var dependecy in dependencies)
        {
            if (m_LoadedAsset.ContainsKey(dependecy) == false)
            {
                AssetBundle bundle = LoadBundle(dependecy);

                if (bundle != null)
                    m_LoadedAsset.Add(dependecy, bundle);
            }
        }
    }

    private async UniTask LoadDependencyAsync(string bundleName)
    {
        if (m_DependencyAsset == null)
        {
            AssetBundle depAsset = null;
#if UNITY_ANDROID
            depAsset = await AssetBundle.LoadFromFileAsync(Path.Combine(m_BasePath, "Android.assetbundle"));
#elif UNITY_IOS
            depAsset = await AssetBundle.LoadFromFileAsync(Path.Combine(m_BasePath, "iOS.assetbundle"));
#elif UNITY_WEBGL
            depAsset = await AssetBundle.LoadFromFileAsync(Path.Combine(m_BasePath, "WebGL.assetbundle"));
#endif

            if (depAsset == null)
                return;

            m_DependencyAsset = depAsset;
        }

        if (m_DependencyAsset == null)
            return;

        if (m_DependencyAsset != null && m_DependencyManifest == null)
        {
            var Request = await m_DependencyAsset.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
            AssetBundleManifest Manifest = Request as AssetBundleManifest;
            if (Manifest == null)
                return;

            m_DependencyManifest = Manifest;
        }

        if (m_DependencyManifest == null)
            return;

        string[] dependencies = m_DependencyManifest.GetAllDependencies(bundleName);
        foreach (var dependecy in dependencies)
        {
            if (m_LoadedAsset.ContainsKey(dependecy) == false)
            {
                AssetBundle bundle = await LoadBundleAsync(dependecy);

                if (bundle != null)
                    m_LoadedAsset.Add(dependecy, bundle);
            }
        }
    }

    public T LoadAsset<T>(string bundleName, string assetName) where T : Object
    {
        string BundlePath = ($"{bundleName}.assetbundle").ToLower();

        LoadDependency(BundlePath);

        AssetBundle bundle;
        if (m_LoadedAsset.ContainsKey(BundlePath) && m_LoadedAsset[BundlePath] != null)
        {
            m_LoadedAsset.TryGetValue(BundlePath, out bundle);
        }
        else
        {
            bundle = LoadBundle(BundlePath);
            if (bundle != null)
                m_LoadedAsset.Add(BundlePath, bundle);
        }

        if (bundle == null)
        {
            Debug.LogError($"There is no asset with name {assetName} in {BundlePath}");
            return new Object() as T;
        }

        T obj = bundle.LoadAsset<T>(assetName);
        if (obj == null)
        {
            Debug.LogError($"Asset Name Is NULL : {bundleName}, {assetName}");
            return new Object() as T;
        }

        return obj;
    }

    public string LoadTextAsset(string bundleName, string assetName)
    {
        string bundlePath = ($"{bundleName}.assetbundle").ToLower();

        var bundle = LoadBundle(bundlePath);
        if (bundle == null)
            return string.Empty;

        TextAsset obj = bundle.LoadAsset<TextAsset>(assetName);
        if (obj == null)
        {
            Debug.LogError($"Asset Name Is NULL : {bundleName}, {assetName}");
            return string.Empty;
        }

        string retValue = obj.text;
        obj = null;
        bundle.Unload(true);

        return retValue;
    }

    public async UniTask<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : Object
    {
        string bundlePath = ($"{bundleName}.assetbundle").ToLower();

        await LoadDependencyAsync(bundlePath);

        AssetBundle bundle;
        if (m_LoadedAsset.ContainsKey(bundlePath) && m_LoadedAsset[bundlePath] != null)
        {
            m_LoadedAsset.TryGetValue(bundlePath, out bundle);
        }
        else
        {
            bundle = await LoadBundleAsync(bundlePath);
            if (bundle != null)
                m_LoadedAsset.Add(bundlePath, bundle);
        }

        if (bundle == null)
        {
            Debug.LogError($"There is no asset with name {assetName} in {bundlePath}");
            return new Object() as T;
        }

        var obj = await bundle.LoadAssetAsync(assetName);
        if (obj == null)
        {
            Debug.LogError($"Asset Name Is NULL : {bundleName}, {assetName}");
            return new Object() as T;
        }

        // 오디오 클립인 경우 로드 상태 확인 (WebGL 오류)
        if (obj is AudioClip audioClip)
        {
            await UniTask.WaitUntil(() => audioClip.loadState == AudioDataLoadState.Loaded);
        }

        return obj as T;
    }

    public async UniTask UnloadAsset(bool isUnload)
    {
        if (isUnload)
        {
            var iter = m_LoadedAsset.GetEnumerator();
            while (iter.MoveNext())
                iter.Current.Value.Unload(true);

            m_LoadedAsset.Clear();
        }
        else
        {
            var iter = m_LoadedAsset.GetEnumerator();
            while (iter.MoveNext())
                iter.Current.Value.Unload(false);

            m_LoadedAsset.Clear();
        }

        await Resources.UnloadUnusedAssets();
        await UniTask.Yield();

        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        await UniTask.Yield();
    }
}
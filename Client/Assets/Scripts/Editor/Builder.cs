using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed class Builder
{
    #region PUBLIC METHODS
    public List<AssetExceptionInfo> GetExceptionInfo()
    {
        // * 사용하지 않는 폴더는 자동으로 스킵되기 때문에 지우지 말고 참고 하자!
        var exceptionInfoList = new List<AssetExceptionInfo>
        {
            // 애니메이션
            new AssetExceptionInfo("Animation", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            // 테이블 데이터
            new AssetExceptionInfo("Data", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Data/Chapter", AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY),
            new AssetExceptionInfo("Data/GSTR", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Data/Table", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),

            // 아틀라스
            new AssetExceptionInfo("Atlas", AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY),

            // 이펙트
            new AssetExceptionInfo("Effect", AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY),

            // Font
            new AssetExceptionInfo("Font", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            
            // 맵 관련
            new AssetExceptionInfo("Map", AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY),

            // 프리팹
            new AssetExceptionInfo("Prefabs", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Prefabs/3D", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/Boss", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/Camera", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/Chapter", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/Contents", AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY),
            new AssetExceptionInfo("Prefabs/Effect", AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY),
            new AssetExceptionInfo("Prefabs/Event", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/Hero", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/Monster", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/Monster/Model", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            new AssetExceptionInfo("Prefabs/Object", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/Object/Model", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/Object/UI", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/Object/Weather", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            new AssetExceptionInfo("Prefabs/Player", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/Player/Model", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            new AssetExceptionInfo("Prefabs/Skill", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            new AssetExceptionInfo("Prefabs/UI", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Prefabs/UI/Common", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            new AssetExceptionInfo("Prefabs/UI/Ingame", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/UI/Ingame/Common", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/UI/Ingame/DamageFont", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/UI/Ingame/Input", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/UI/Ingame/List", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/UI/Ingame/Material", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Prefabs/UI/Ingame/Shader", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            new AssetExceptionInfo("Prefabs/UI/Outgame", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/UI/Outgame/Background", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/UI/Outgame/Common", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/UI/Outgame/List", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),

            new AssetExceptionInfo("Prefabs/UI/Popup", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/UI/Slot", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/UI/Window", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),

            new AssetExceptionInfo("Prefabs/Game", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Prefabs/Game/Background", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Prefabs/Game/Block", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),

            // 쉐이더
            new AssetExceptionInfo("Shaders", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            // 사운드
            new AssetExceptionInfo("Sound", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Sound/BGM", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Sound/SFX", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Sound/SFX/Game", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),
            new AssetExceptionInfo("Sound/SFX/UI", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),

            // 오디오 믹서
            new AssetExceptionInfo("AudioMixer", AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER),

            // 스파인
            new AssetExceptionInfo("Spine", AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY),

            // 텍스쳐
            new AssetExceptionInfo("Texture", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Texture/Background", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Chapter", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Deco", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/FindDiff", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/GIF_Image", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Ingame", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Ingame_Block", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Ingame_Deco", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Ingame_Hero_pannel", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Ingame_SummonBush", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Loading", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Outgame", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Sequence", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/Toon", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/UI", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Texture/UpperMask", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Image", AssetExceptionInfo.eExceptionType.NONE),
            new AssetExceptionInfo("Image/Character", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Image/Ranking", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
            new AssetExceptionInfo("Image/Slider", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),

            new AssetExceptionInfo("Video", AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF),
        };

        return exceptionInfoList;
    }

    public void AssignAssetBundleName(string resourceRootPath)
    {
        string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        List<string> unnecessaryAssetbundleNameList = allAssetBundleNames.ToList();

        List<AssetExceptionInfo> exceptionInfoList = GetExceptionInfo();

        List<AssetInfo> assetInfoList = GetAssetInfoList(resourceRootPath, true, exceptionInfoList);

        foreach (AssetInfo assetInfo in assetInfoList)
        {
            string resourcePath = assetInfo.Path;
            string assetBundleName = assetInfo.Name;

            AssetImporter assetImporter = AssetImporter.GetAtPath(resourcePath);

            if (!assetImporter.assetBundleName.Equals(assetBundleName))
            {
                assetImporter.assetBundleName = assetBundleName;
                assetImporter.SaveAndReimport();
            }

            unnecessaryAssetbundleNameList.Remove(assetBundleName);
        }

        foreach (string unnecessaryAssetBundleName in unnecessaryAssetbundleNameList)
        {
            Debug.Log("Removing Unnecessary Asset Bundle Name : " + unnecessaryAssetBundleName);
            AssetDatabase.RemoveAssetBundleName(unnecessaryAssetBundleName, true);
        }
    }

    public void MakeAllBundles(string resourceRootPath, string BundleOutputPath, BuildTarget buildTarget)
    {
        string DefalutBundlePath = $"{BundleOutputPath}_Default";
        string PlatFormName = buildTarget.ToString();

        Debug.Log($"[AssetBundle] 빌드 시작");
        Debug.Log($"[AssetBundle] BuildTarget : {buildTarget}");
        Debug.Log($"[AssetBundle] 임시 출력 경로 : {DefalutBundlePath}");
        Debug.Log($"[AssetBundle] 최종 출력 경로 : {BundleOutputPath}");

        // 기존 임시 빌드 폴더 삭제
        if (Directory.Exists(DefalutBundlePath))
        {
            Directory.Delete(DefalutBundlePath, true);
        }

        Directory.CreateDirectory(DefalutBundlePath);

        List<AssetExceptionInfo> exceptionInfoList = GetExceptionInfo();
        List<AssetInfo> assetInfoList = GetAssetInfoList(resourceRootPath, true, exceptionInfoList);

        Debug.Log($"[AssetBundle] 빌드 대상 Bundle 개수 : {assetInfoList.Count}");

        // 실제 AssetBundle 빌드
        AssetBundleManifest assetBundleManifest = runAssetBundleBuild(DefalutBundlePath, assetInfoList, buildTarget);

        if (assetBundleManifest == null)
        {
            Debug.LogError("[AssetBundle] AssetBundle 빌드 실패");
            return;
        }

        Debug.Log("[AssetBundle] AssetBundle 빌드 성공");

        // Unity가 생성한 Root Manifest Bundle 찾기
        string rootBundleName = Path.GetFileName(DefalutBundlePath);
        string singleManifestFilePath = Path.Combine(DefalutBundlePath, rootBundleName);
        string convertBundleFilePath = Path.Combine(DefalutBundlePath, $"{PlatFormName}.assetbundle");

        Debug.Log($"[AssetBundle] Root Manifest Bundle 예상 경로 : {singleManifestFilePath}");

        // Root Manifest Bundle 이름 변경
        if (File.Exists(singleManifestFilePath))
        {
            if (File.Exists(convertBundleFilePath))
            {
                File.Delete(convertBundleFilePath);
            }

            File.Move(singleManifestFilePath, convertBundleFilePath);

            Debug.Log($"[AssetBundle] Root Manifest Bundle 변경 완료 : {convertBundleFilePath}");
        }
        else
        {
            Debug.LogWarning($"[AssetBundle] Root Manifest Bundle 파일을 찾지 못했습니다 : {singleManifestFilePath}");

            // 실제 생성된 파일 확인용 로그
            string[] files = Directory.GetFiles(DefalutBundlePath, "*", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
                Debug.Log($"[AssetBundle] 생성 파일 : {file}");

            return;
        }

        // Root Manifest의 .manifest 파일 이름 변경
        string sourceManifestPath = $"{singleManifestFilePath}.manifest";
        string targetManifestPath = Path.Combine(DefalutBundlePath, $"{PlatFormName}.manifest");

        if (File.Exists(sourceManifestPath))
        {
            if (File.Exists(targetManifestPath))
            {
                File.Delete(targetManifestPath);
            }

            File.Move(sourceManifestPath, targetManifestPath);
        }

        // 기존 PatchList 읽기
        List<string> previousAssetbundleNameList = GetAssetbundleNameListFromPatchListFile(DefalutBundlePath);

        // 삭제된 AssetBundle 제거
        RemoveDeletedPatchFile(PlatFormName, DefalutBundlePath, assetBundleManifest, previousAssetbundleNameList);

        // PatchList 생성
        PatchList patchList = MakePatchList(DefalutBundlePath, assetBundleManifest);

        // Root Manifest Bundle도 PatchList에 추가
        FileInfo rootBundleFileInfo = new FileInfo(convertBundleFilePath);
        byte[] rootBundleBytes = File.ReadAllBytes(convertBundleFilePath);
        Hash128 rootBundleHash = Hash128.Compute(rootBundleBytes);
        PatchData rootBundleData = new PatchData($"{PlatFormName}.assetbundle", rootBundleHash, rootBundleFileInfo.Length);
        patchList.AddData(rootBundleData);

        // PatchList.pat 생성
        WritePatchListFile(patchList, DefalutBundlePath);
        Debug.Log($"[AssetBundle] PatchList 생성 완료 : {Path.Combine(DefalutBundlePath, "PatchList.pat")}");

        // 실제 업로드용 폴더로 복사
        CopyPatchFileForUpload(assetBundleManifest, DefalutBundlePath, BundleOutputPath);
        Debug.Log($"[AssetBundle] 업로드용 파일 복사 완료 : {BundleOutputPath}");

        AssetDatabase.Refresh();
        Debug.Log("[AssetBundle] 모든 작업 완료");
    }
    #endregion PUBLIC METHODS

    #region PRIVATE METHODS
    private List<AssetInfo> GetAssetInfoList(string resourceRootPath, bool bTopDirectoryBundle, List<AssetExceptionInfo> exceptionInfoList = null)
    {
        List<AssetInfo> resultList = new List<AssetInfo>();

        if (true == bTopDirectoryBundle)
        {
            string[] directoryPaths = Directory.GetDirectories(resourceRootPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string directoryPath in directoryPaths)
            {
                if (isExistDirectory(directoryPath, exceptionInfoList))
                    continue;

                if (!hasFileInPath(directoryPath))
                {
                    Debug.LogWarning(directoryPath + " : 빈 리소스 폴더 입니다.");
                    continue;
                }

                string path = directoryPath.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(directoryPath) + ".assetbundle";
                resultList.Add(new AssetInfo(path, name));
            }
        }

        if (null != exceptionInfoList)
        {
            foreach (AssetExceptionInfo exceptionInfo in exceptionInfoList)
            {
                string exceptionPath = Path.Combine(resourceRootPath, exceptionInfo.Name);

                // 실제 프로젝트에 해당 폴더가 없으면 건너뜀
                if (!Directory.Exists(exceptionPath))
                {
                    Debug.LogWarning($"[AssetBundle] 폴더 없음 - Skip : {exceptionPath}");
                    continue;
                }

                switch (exceptionInfo.Type)
                {
                    case AssetExceptionInfo.eExceptionType.NONE:
                        {
                            int FindIndex = resultList.FindIndex(Data => Data.Name == exceptionInfo.Name + ".assetbundle");
                            if (FindIndex != -1)
                                resultList.RemoveAt(FindIndex);
                        }
                        break;
                    case AssetExceptionInfo.eExceptionType.EXCLUDEFOLDER:
                        {
                            string[] sceneDrectoryPaths = Directory.GetFiles(resourceRootPath + "/" + exceptionInfo.Name);

                            foreach (string sceneDrectoryPath in sceneDrectoryPaths)
                            {
                                if (sceneDrectoryPath.Contains(".meta"))
                                    continue;

                                string path = sceneDrectoryPath.Replace('\\', '/');
                                string name = exceptionInfo.Name + "/" + Path.GetFileNameWithoutExtension(sceneDrectoryPath) + ".assetbundle";
                                resultList.Add(new AssetInfo(path, name));
                            }
                        }
                        break;
                    case AssetExceptionInfo.eExceptionType.SUBTOPDIRECTORY:
                        {
                            string[] subTopDirectoryPaths = Directory.GetDirectories(resourceRootPath + "/" + exceptionInfo.Name, "*", SearchOption.TopDirectoryOnly);

                            foreach (string subTopdirectoryPath in subTopDirectoryPaths)
                            {
                                if (!hasFileInPath(subTopdirectoryPath))
                                {
                                    Debug.LogWarning(subTopdirectoryPath + " : 빈 리소스 폴더 입니다.");
                                    continue;
                                }

                                string path = subTopdirectoryPath.Replace('\\', '/');
                                string name = exceptionInfo.Name + "/" + Path.GetFileNameWithoutExtension(subTopdirectoryPath) + ".assetbundle";
                                resultList.Add(new AssetInfo(path, name));
                            }
                        }
                        break;
                    case AssetExceptionInfo.eExceptionType.TOPDIRECTORY_SELF:
                        {
                            string DirPath = Path.Combine(resourceRootPath, exceptionInfo.Name);

                            if (!hasFileInPath(DirPath))
                            {
                                Debug.LogWarning(DirPath + " : 빈 리소스 폴더 입니다.");
                                continue;
                            }

                            string path = DirPath.Replace('\\', '/');
                            string name = $"{exceptionInfo.Name}.assetbundle";
                            resultList.Add(new AssetInfo(path, name));
                        }
                        break;
                    case AssetExceptionInfo.eExceptionType.SCENEDIRECTORY:
                        {
                            string[] sceneDrectoryPaths = Directory.GetFiles(resourceRootPath + "/" + exceptionInfo.Name, "*.unity", SearchOption.AllDirectories);

                            foreach (string sceneDrectoryPath in sceneDrectoryPaths)
                            {
                                string path = sceneDrectoryPath.Replace('\\', '/');
                                string name = exceptionInfo.Name + "/" + Path.GetFileNameWithoutExtension(sceneDrectoryPath) + ".assetbundle";
                                resultList.Add(new AssetInfo(path, name));
                            }
                        }
                        break;
                }
            }
        }

        return resultList;
    }

    private AssetBundleManifest runAssetBundleBuild(string outputPath, List<AssetInfo> assetInfoList, BuildTarget buildTarget)
    {
        List<AssetBundleBuild> bundleBuildList = new List<AssetBundleBuild>();

        foreach (AssetInfo assetInfo in assetInfoList)
        {
            AssetBundleBuild buildMap = new AssetBundleBuild();

            buildMap.assetBundleName = assetInfo.Name;
            buildMap.assetNames = new string[] { assetInfo.Path };

            bundleBuildList.Add(buildMap);
        }

        AssetBundleBuild[] builds = bundleBuildList.ToArray();

        return BuildPipeline.BuildAssetBundles(outputPath, builds, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
    }

    private bool hasFileInPath(string path)
    {
        // 폴더 자체가 존재하지 않으면 false
        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"[AssetBundle] 폴더 없음 - Skip : {path}");
            return false;
        }

        return Directory.GetFiles(path).Length > 0 ||
               Directory.GetDirectories(path).Length > 0;
    }

    private bool isExistDirectory(string resourceDirectoryPath, List<AssetExceptionInfo> exceptionInfoList)
    {
        if (null == exceptionInfoList)
            return false;

        foreach (AssetExceptionInfo exceptionInfo in exceptionInfoList)
        {
            if (exceptionInfo.Name.Equals(Path.GetFileNameWithoutExtension(resourceDirectoryPath)))
                return true;
        }

        return false;
    }

    private List<string> GetAssetbundleNameListFromPatchListFile(string BundleOutputPath)
    {
        string patchListFilePath = Path.Combine(BundleOutputPath, "PatchList.pat");
        if (!File.Exists(patchListFilePath))
            return new List<string>();

        string patchListJsonString = File.ReadAllText(patchListFilePath);
        PatchList patchList = new PatchList(patchListJsonString);

        return patchList.datas.Keys.ToList();
    }

    private void RemoveDeletedPatchFile(string PlatformName, string assetBundleOutputPath, AssetBundleManifest newAssetBundleManifest, List<string> previousAssetbundleNameList)
    {
        string[] allAssetbundles = newAssetBundleManifest.GetAllAssetBundles();

        // 이전 패치 목록에 있고 현재 패치 목록에도 있으면 삭제 대상에서 제거
        foreach (string assetBundleName in allAssetbundles)
        {
            previousAssetbundleNameList.Remove(assetBundleName);
        }

        // 종속성 에셋번들은 삭제 대상에서 제거
        previousAssetbundleNameList.Remove($"{PlatformName}.assetbundle");

        // 삭제 대상 파일을 실제로 삭제한다
        foreach (string assetbundleName in previousAssetbundleNameList)
        {
            string assetbundlePath = assetBundleOutputPath + "/" + assetbundleName;
            File.Delete($"{assetbundlePath}.asetbundle");
            File.Delete($"{assetbundlePath}.manifest");
            Debug.Log(assetbundleName + " has been Removed");
        }

        // 빈 폴더를 삭제한다
        string[] outputSubDirectoryPaths = Directory.GetDirectories(assetBundleOutputPath, "*.*", SearchOption.AllDirectories);
        foreach (string outputSubdirectoryPath in outputSubDirectoryPaths)
        {
            if (hasFileInPath(outputSubdirectoryPath) == false)
            {
                Directory.Delete(outputSubdirectoryPath);
                Debug.Log(outputSubdirectoryPath + " has been Removed because it was empty");
            }
        }
    }

    private void CopyPatchFileForUpload(AssetBundleManifest Manifest, string CopyTargetPath, string NewTargetPath)
    {
        DirectoryCopy(CopyTargetPath, NewTargetPath, true);
    }

    private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension == ".manifest")
                continue;

            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }

    private PatchList MakePatchList(string OutPath, AssetBundleManifest Manifest)
    {
        PatchList patchList = new PatchList();
        string[] allAssetbundles = Manifest.GetAllAssetBundles();
        foreach (string assetBundleName in allAssetbundles)
        {
            PatchData data = CreatePatchData(assetBundleName, Manifest, OutPath);
            patchList.AddData(data);
        }

        return patchList;
    }

    private void WritePatchListFile(PatchList patchList, string OutPath)
    {
        string jsonString = patchList.GetJsonString();
        File.WriteAllText(Path.Combine(OutPath, "PatchList.pat"), jsonString);
    }

    private PatchData CreatePatchData(string assetBundleName, AssetBundleManifest assetBundleManifest, string assetbundleOutputPath)
    {
        Hash128 hash = assetBundleManifest.GetAssetBundleHash(assetBundleName);

        FileInfo assetbundleFileInfo = new FileInfo(assetbundleOutputPath + "/" + assetBundleName);
        long fileSize = assetbundleFileInfo.Length;

        return new PatchData(assetBundleName, hash, fileSize);
    }
    #endregion PRIVATE METHODS
}

public struct AssetExceptionInfo
{
    // 특정 리소스 폴더를 어떤 규칙으로 AssetBundle로 묶을지 지정하는 타입
    public enum eExceptionType
    {
        /// <summary>
        /// 해당 폴더를 번들 대상에서 제외
        /// </summary>
        NONE,

        /// <summary>
        /// 해당 폴더 바로 아래 파일을 각각 개별 번들 (번들 파일 수는 많지만 하나의 수정으로 하나만 패치)
        /// </summary>
        EXCLUDEFOLDER,

        /// <summary>
        /// 해당 폴더의 바로 아래 자식 폴더별로 번들
        /// </summary>
        SUBTOPDIRECTORY,

        /// <summary>
        /// 해당 폴더 전체를 하나의 번들로 묶음 (번들 파일 수는 적지만 하나의 수정으로 모두 패치)
        /// </summary>
        TOPDIRECTORY_SELF,

        /// <summary>
        /// 하위의 .unity 씬 파일을 찾아 씬별 개별 번들
        /// </summary>
        SCENEDIRECTORY
    }

    private string name;

    public string Name
    {
        get { return name; }
    }

    private eExceptionType type;

    public eExceptionType Type
    {
        get { return type; }
    }

    public AssetExceptionInfo(string name, eExceptionType type)
    {
        this.name = name;
        this.type = type;
    }
};

public struct AssetInfo
{
    private string path;
    public string Path
    {
        get { return path; }
    }

    private string name;
    public string Name
    {
        get { return name; }
    }

    public AssetInfo(string path, string name)
    {
        this.path = path;
        this.name = name;
    }
}
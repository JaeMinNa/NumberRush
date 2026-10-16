using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class PatchList
{
    public Dictionary<string, PatchData> datas
    {
        get
        {
            return m_Datas;
        }
    }

    private Dictionary<string, PatchData> m_Datas;

    public PatchList()
    {
        m_Datas = new Dictionary<string, PatchData>();
    }

    public PatchData GetPatchData(string BundleName)
    {
        if (m_Datas.ContainsKey(BundleName))
            return m_Datas[BundleName];
        else
            return null;
    }

    // 로컬 패치파일 로드
    public void LoadLocalPatchList()
    {
        string path = Path.Combine(Application.persistentDataPath, "AssetBundle", "PatchList.pat");
        if (File.Exists(path))
        {
            string text = File.ReadAllText(path);
            if (string.IsNullOrEmpty(text))
                m_Datas = new Dictionary<string, PatchData>();
            else
                m_Datas = MakePatchList(text);
        }
        else
        {
            m_Datas = new Dictionary<string, PatchData>();
        }
    }

    public Dictionary<string, PatchData> GetUpdateList(Dictionary<string, PatchData> NewPatch)
    {
        Dictionary<string, PatchData> updateDatas = new Dictionary<string, PatchData>();

        foreach (var Patch in NewPatch)
        {
            // 리스트에 파일이 있다
            if (m_Datas.ContainsKey(Patch.Key))
            {
                string Key = Patch.Key;

                // 해쉬값이 다름
                if (m_Datas[Key].hash != Patch.Value.hash)
                {
                    updateDatas.Add(Patch.Key, Patch.Value);
                }
                // 파일 존재 체크
                else
                {
                    string FilePath = Path.Combine(Application.persistentDataPath, "AssetBundle", Patch.Value.name);
                    if (File.Exists(FilePath))
                    {
                        FileInfo info = new FileInfo(FilePath);
                        if (info.Length != Patch.Value.size)
                        {
                            updateDatas.Add(Patch.Value.name, Patch.Value);
                        }
                    }
                    else
                    {
                        updateDatas.Add(Patch.Value.name, Patch.Value);
                    }
                }
            }
            // 리스트에 파일이 없다
            else
            {
                // 파일 존재 체크
                string FilePath = Path.Combine(Application.persistentDataPath, "AssetBundle", Patch.Value.name);
                if (File.Exists(FilePath))
                {
                    FileInfo info = new FileInfo(FilePath);
                    if (info.Length != Patch.Value.size)
                        updateDatas.Add(Patch.Value.name, Patch.Value);
                }
                else
                {
                    updateDatas.Add(Patch.Value.name, Patch.Value);
                }
            }
        }

        return updateDatas;
    }

    public Dictionary<string, PatchData> GetRemoveList(Dictionary<string, PatchData> NewPatch)
    {
        Dictionary<string, PatchData> removeDatas = new Dictionary<string, PatchData>();

        foreach (var Patch in m_Datas)
        {
            if (NewPatch.ContainsKey(Patch.Key))
                continue;

            removeDatas.Add(Patch.Key, Patch.Value);
        }

        return removeDatas;
    }

    public void UpdateData(Dictionary<string, PatchData> NewList)
    {
        m_Datas = new Dictionary<string, PatchData>(NewList);
    }

    public void WriteLocalPatchList()
    {
        string path = Path.Combine(Application.persistentDataPath, "AssetBundle", "PatchList.pat");

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is invalid.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        JsonWriter jsonWriter = new JsonTextWriter(sw);

        jsonWriter.Formatting = Formatting.Indented;

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("Datas");
        jsonWriter.WriteStartArray();

        foreach (KeyValuePair<string, PatchData> pair in m_Datas)
        {
            pair.Value.WriteData(jsonWriter);
        }

        jsonWriter.WriteEndArray();
        jsonWriter.WriteEndObject();

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        File.WriteAllText(path, sb.ToString());
    }

    public void SortByFileSize()
    {
        IEnumerable<PatchData> sorted = from pair in m_Datas orderby pair.Value.size ascending select pair.Value;
        m_Datas = sorted.ToDictionary(patchData => patchData.name);
    }

    public static Dictionary<string, PatchData> MakePatchList(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
            return new Dictionary<string, PatchData>();

        JObject json = JObject.Parse(jsonData);
        if (json == null)
            return new Dictionary<string, PatchData>();

        JToken array = json["Datas"];
        if (array == null)
            return new Dictionary<string, PatchData>();

        Dictionary<string, PatchData> datas = new Dictionary<string, PatchData>();

        for (int i = 0; i < array.Count(); ++i)
        {
            JToken obj = array[i];
            PatchData data = new PatchData(obj);

            datas.Add(data.name, data);
        }

        return datas;
    }

    #region Editor
    public PatchList(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData)) return;

        JObject json = JObject.Parse(jsonData);
        if (json == null) return;

        if (m_Datas == null)
            m_Datas = new Dictionary<string, PatchData>();

        m_Datas.Clear();

        JToken array = json["Datas"];
        if (array == null) return;

        for (int i = 0; i < array.Count(); ++i)
        {
            JToken obj = array[i];
            PatchData data = new PatchData(obj);

            m_Datas.Add(data.name, data);
        }
    }

    public void AddData(PatchData data)
    {
        if (data == null) return;

        m_Datas.Add(data.name, data);
    }

    public string GetJsonString()
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        JsonWriter jw = new JsonTextWriter(sw);

        jw.Formatting = Formatting.Indented;

        WriteJsonData(jw);

        return sb.ToString();
    }

    private void WriteJsonData(JsonWriter writer)
    {
        if (writer == null) return;

        writer.WriteStartObject();

        writer.WritePropertyName("Datas");

        writer.WriteStartArray();
        foreach (PatchData data in m_Datas.Values)
        {
            data.WriteData(writer);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
    #endregion
}
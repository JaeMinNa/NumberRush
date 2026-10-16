using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class PatchData
{
    #region     PRIVATE FIELDS
    private string m_Name;
    private long m_FileSize;
    private Hash128 m_Hash;
    #endregion  PRIVATE FIELDS

    #region     PROPERTIES
    public string name
    {
        get { return m_Name; }
    }

    public long size
    {
        get { return m_FileSize; }
    }

    public Hash128 hash
    {
        get { return m_Hash; }
        set { m_Hash = value; }
    }
    #endregion  PROPERTIES

    #region     PUBLIC METHODS
    public PatchData(string assetBundleName, Hash128 hash, long fileSize)
    {
        m_Name = assetBundleName;
        m_Hash = hash;
        m_FileSize = fileSize;
    }

    public PatchData(JToken obj)
    {
        if (obj == null)
        {
            Debug.LogError("오브젝트가 올바르지 않습니다.");
            return;
        }

        m_Name = obj["Name"].ToString();

        string hashString = obj["Hash"].ToString();
        m_Hash = Hash128.Parse(hashString);

        if (!long.TryParse(obj["Size"].ToString(), out m_FileSize))
            m_FileSize = 0;
    }

    public void WriteData(JsonWriter writer)
    {
        if (writer == null)
        {
            Debug.LogError("JsonWriter is invalid");
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName("Name");
        writer.WriteValue(name);

        writer.WritePropertyName("Hash");
        writer.WriteValue(hash.ToString());

        writer.WritePropertyName("Size");
        writer.WriteValue(size);

        writer.WriteEndObject();
    }
    #endregion  PUBLIC METHODS
}
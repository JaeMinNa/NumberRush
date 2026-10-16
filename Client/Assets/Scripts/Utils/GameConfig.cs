
using System;
using UnityEngine;

public enum ResourceType
{
    Local,
    Bundle,
}

[CreateAssetMenu]
public class GameConfig : ScriptableObject
{
    [SerializeField] private ServerType m_ServerType;
    [SerializeField] private ResourceType m_ResourceType;

    private string m_ServerAddress = string.Empty;
    private string m_PatchAddress = string.Empty;

    public void SetConfig(ResourceType resourceType)
    {
        m_ResourceType = resourceType;
    }

    public void SetServerType(ServerType serverType)
    {
        m_ServerType = serverType;
    }

    public void SetServerAddress(string addr)
    {
        m_ServerAddress = addr;
    }

    public void SetPatchAddress(string addr)
    {
        m_PatchAddress = addr;
    }

    public string ServerAddress()
    {
        switch (m_ServerType)
        {
            case ServerType.Local:
                return "http://localhost:15000/Server";

            case ServerType.Live:
                return "http://43.201.58.20:15000/Server";
        }

        return string.Empty;
    }

    public string PatchAdress()
    {
        return $"{m_PatchAddress}/{Application.version}/Android";
    }

    public ServerType ServerConnectType()
    {
        return m_ServerType;
    }

    public ResourceType ResourceLoadType()
    {
        return m_ResourceType;
    }
}
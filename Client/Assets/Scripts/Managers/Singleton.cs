using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : class
{
    protected static T m_Instance = null;

    protected abstract void CreateInstance();
    public abstract void DestroyInstance();
}
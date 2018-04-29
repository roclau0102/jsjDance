using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// NetworkManger ��װ�ɸ������ˡ�
/// </summary>
public class NetworkManager : MonoBehaviour
{
    public enum STATE { DISCONNECT, CONNECTING, COLLECTED }
    public delegate void SocketDelegate(int protocol, Dictionary<string, object> data);
    public SocketDelegate socketCallback;
    INetworkClient client;

    static NetworkManager _instance;
    public static NetworkManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject();
                go.name = "NetworkManager";
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<NetworkManager>();
                _instance.Init();
            }
            return _instance;
        }
    }
    

    void Init()
    {        
        client = new SocketClient();
    }


    /// <summary>
    /// ����Э��
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="data"></param>
    /// <param name="callback"></param>
    public void SendProtocol(Protocol.PROTOCOL protocol, Dictionary<string, object> data, ProtocolCallback callback)
    {
        if (callback != null) AddCallback(callback);
        client.SendProtocol(protocol, data);
    }

    /// <summary>
    /// ���Э��ص�
    /// </summary>
    /// <param name="callback"></param>
    public void AddCallback(ProtocolCallback callback)
    {
        if (callback == null)
        {
            Debug.LogWarning("Э��ص�Ϊɶ�ǿյģ���");
            return;
        }
        client.AddCallback(callback);
    }

    static public void outputBytes(byte[] r)
    {
        string rr = "{";
        for (int i = 0; i < r.Length; i++)
        {
            rr += (sbyte)r[i];
            if (i != (r.Length - 1))
            {
                rr += ",";
            }
        }
        rr += "}";
        Debug.Log(rr);
    }

    /// <summary>
    /// ����socket��
    /// </summary>
    void Update()
    {
        client.Update();
    }

    public STATE  GetConnectState()
    {
        return client.GetConnectState();
    }

    public void CloseConnection()
    {
        client.CloseConnection();
    }

    public void Connect()
    {
        client.Connect();
    }
}

#region �ӿ�
/// <summary>
/// �򵥵Ľӿ�
/// </summary>
public interface INetworkClient
{
    void AddCallback(ProtocolCallback callback);
    void SendProtocol(Protocol.PROTOCOL protocol, Dictionary<string, object> data);

    void Update();
    NetworkManager.STATE GetConnectState();

    void CloseConnection();

    void Connect();
}
#endregion

/// <summary>
/// �ص���Ϣ
/// </summary>
public class ProtocolCallback
{
    public Protocol.PROTOCOL protocol;
    public Action<Dictionary<string, object>> callback;
    public bool autoRemove = true;
}


#region EDITOR
#if UNITY_EDITOR
[CanEditMultipleObjects()]
[CustomEditor(typeof(NetworkManager), true)]
public class NetworkManagerInspctor:Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("State:"+  NetworkManager.instance.GetConnectState().ToString());
        GUILayout.Label("Pleaes Use Protocol Class to do things.");
        NetworkManager m = target as NetworkManager;
        switch (NetworkManager.instance.GetConnectState() )
        {
            case NetworkManager.STATE.COLLECTED:
            if (GUILayout.Button("�Ͽ�", GUILayout.Height(50)))
            {                
                m.CloseConnection();
            }
            break;
            case NetworkManager.STATE.CONNECTING:
            if (GUILayout.Button("������", GUILayout.Height(50)))
            {
                Debug.Log("�Եȡ���������.....");   
            }
            break;
            case NetworkManager.STATE.DISCONNECT:
            if (GUILayout.Button("����", GUILayout.Height(50)))
            {                
                m.Connect();
            }
            break;
        }
    }
}
#endif
#endregion
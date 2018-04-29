using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// NetworkManger 包装成个经理了。
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
    /// 发送协议
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
    /// 添加协议回调
    /// </summary>
    /// <param name="callback"></param>
    public void AddCallback(ProtocolCallback callback)
    {
        if (callback == null)
        {
            Debug.LogWarning("协议回调为啥是空的？？");
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
    /// 更新socket类
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

#region 接口
/// <summary>
/// 简单的接口
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
/// 回调信息
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
            if (GUILayout.Button("断开", GUILayout.Height(50)))
            {                
                m.CloseConnection();
            }
            break;
            case NetworkManager.STATE.CONNECTING:
            if (GUILayout.Button("连接中", GUILayout.Height(50)))
            {
                Debug.Log("稍等。正在连接.....");   
            }
            break;
            case NetworkManager.STATE.DISCONNECT:
            if (GUILayout.Button("连接", GUILayout.Height(50)))
            {                
                m.Connect();
            }
            break;
        }
    }
}
#endif
#endregion
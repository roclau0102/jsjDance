using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

/// <summary>
/// Socket类。很简单的流程，三分钟就能看懂。不解释了。
/// </summary>

public class SocketClient: INetworkClient
{
    #region 变量   
    Dictionary<Protocol.PROTOCOL, ProtocolCallback> protocolCallback = new Dictionary<Protocol.PROTOCOL, ProtocolCallback>();
    public enum DisType {    Exception,   Disconnect  }

    public enum NetActionType{Connect, Message,Logout }

    /// <summary>
    /// socket超时时间...
    /// </summary>
    const float MAX_WAIT_CONNECT_TIME = 10;
    /// <summary>
    /// 最大尝试连接次数
    /// </summary>
    const int MAX_RETRY_TIME = 5;
    /// <summary>
    /// 连接状态
    /// </summary>
    public NetworkManager.STATE state = NetworkManager.STATE.DISCONNECT;
    /// <summary>
    /// 不用说
    /// </summary>
    private TcpClient client = null;
    private NetworkStream outStream = null;
    private MemoryStream memStream;
    private BinaryReader reader;
    private const int MAX_READ = 8192;
    private byte[] byteBuffer = new byte[MAX_READ];

    /// <summary>
    /// 出消息处理队列
    /// </summary>
    static private Queue<KeyValuePair<NetActionType, ByteBuffer>> outQueue = new Queue<KeyValuePair<NetActionType, ByteBuffer>>();
    /// <summary>
    /// 入消息处理队列
    /// </summary>
    private static Queue<KeyValuePair<Protocol.PROTOCOL, string>> InQueue = new Queue<KeyValuePair<Protocol.PROTOCOL, string>>();
    /// <summary>
    /// 连接的时间
    /// </summary>
    float connectionTime = 0;
    /// <summary>
    /// 重试次数
    /// </summary>
    float retryTime = 0;
    

    public SocketClient()
    {
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);
    }
    #endregion

    #region 公用变量
    /// <summary>
    /// 当处理完消息之后自动断开链接
    /// </summary>
    public bool autoDisconnect = false;

    #endregion

    #region 消息检查处理
    public void CheckRecevieQueue()
    {
        //处理消息队列
        if (InQueue.Count > 0)
        {
            while (InQueue.Count > 0)
            {
                KeyValuePair<Protocol.PROTOCOL, string> _event = InQueue.Dequeue();                
                try
                {
                    Dictionary<string, object> data = null;
                    if (_event.Value!=null && _event.Value.Length > 0)
                    { 
                        data = MiniJSON.Json.Deserialize(_event.Value) as Dictionary<string, object>;
                    }
                    if (protocolCallback.ContainsKey(_event.Key) && protocolCallback[_event.Key].callback != null)
                    {
                         var info = protocolCallback[_event.Key];
                         info.callback(data);
                         if(info.autoRemove)protocolCallback.Remove(_event.Key);
                         Log("协议[ "+ _event.Key.ToString() + " ]处理完毕");
                    }
                    else
                    {
                        Log("没有找到协议为" + (int)_event.Key + "解析的回调 ~~~");
                    }
                }
                catch (Exception e)
                {
                    Log("协议" +(int) _event.Key + " 数据结构体出错：" + e.ToString());
                }
                break;
            }
        }
    }

    public void Update()
    {
        if (state == NetworkManager.STATE.CONNECTING)
        {
            if ((Time.time - connectionTime) > MAX_WAIT_CONNECT_TIME)
            {                
                connectionTime = Time.time;
                retryTime++; 
                if (retryTime >= MAX_RETRY_TIME)
                {
                    retryTime = 0;
                    CloseConnection();
                    Log("重试连接达到最大" + MAX_RETRY_TIME + "次，无法连接服务器。");
                }
                else
                {
                    Log("重试连接...当前次数：" + retryTime);
                    Connect();
                }
            }
        }
        CheckSendQueue();
        CheckRecevieQueue();
    }

    /// <summary>
    /// 检查发送消息循环
    /// </summary>
    public void CheckSendQueue()
    {
        if (outQueue.Count == 0) return;
        switch (state)
        {
            case NetworkManager.STATE.CONNECTING:
                return;
            case NetworkManager.STATE.DISCONNECT:
                Connect(); //自动连接上服务器
                return;
        }
        while (outQueue.Count > 0)
        {
            KeyValuePair<NetActionType, ByteBuffer> _event = outQueue.Dequeue();
            switch (_event.Key)
            {                
                case NetActionType.Connect:
                    //nothing to do;
                    break;
                case NetActionType.Message:                    
                    WriteMessage(_event.Value.ToBytes());
                    break;
                case NetActionType.Logout: CloseConnection(); break;
            }
            if (_event.Value != null) _event.Value.Close();
        }
        if (autoDisconnect)
        {
            CloseConnection();
        }
    }


    #endregion
    
    #region 事件


    /// <summary>
    /// 连接到服务器
    /// </summary>
    public void Connect()
    {
        if (client != null)
        {
            CloseConnection();
        }        
        client = new TcpClient();
        client.SendTimeout = 1000;
        client.ReceiveTimeout = 1000;
        client.NoDelay = true;
        state = NetworkManager.STATE.CONNECTING;
        Log("开始连接服务器...");
        connectionTime = Time.time;
        try
        {
            var callback = new AsyncCallback(OnConnect);            
            client.BeginConnect("218.17.99.70", 9919, callback , null);
        }
        catch (Exception e)
        {
            CloseConnection(); Log(""+e.Message);
        }
    }


    /// <summary>
    /// 连接上服务器
    /// </summary>
    void OnConnect(IAsyncResult asr)
    {        
        outStream = client.GetStream();
        client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
        state = NetworkManager.STATE.COLLECTED;
        Log("连接服务器成功");
        retryTime = 0;
        /*
        //登陆成功之后发送102协议
        byte[] modules = RSA.GetModulus();
        SendProtocol(102, modules); 
        AddProtoclCallback(new ProtocolCallback()
        {
            protocol = 1020,
            callback = (a) =>
            {
                Log("上传Modules：" + a["result"]);
                SendProtocol(103, RSA.GetExponent()); 
            }
        });

        AddProtoclCallback(new ProtocolCallback()
        {
            protocol = 1030,
            callback = (a) =>
            {
                Log("上传Exponent：" + a["result"]);
                state = NetworkManager.STATE.COLLECTED;
            }
        });*/
    }

    /// <summary>
    /// 丢失链接
    /// </summary>
    void OnDisconnected(DisType dis, string msg)
    {
        CloseConnection();   //关掉客户端链接        
        Log("Connection was closed by the server:>" + msg + " Distype:>" + dis);
    }

    #endregion

    #region 写
    /// <summary>
    /// 写数据
    /// </summary>
    void WriteMessage(byte[] message)
    {
        MemoryStream ms = null;
        using (ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter writer = new BinaryWriter(ms);
            int msglen = message.Length;
            writer.Write(System.Net.IPAddress.HostToNetworkOrder(msglen));
            writer.Write(message);
            writer.Flush();
            Log("发送消息" + writer.BaseStream.Length + "字节");
            if (client != null && client.Connected)
            {
                byte[] payload = ms.ToArray();
                outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
            }
            else
            {
                Log("client.connected----->>false");
            }
        }
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    void OnWrite(IAsyncResult r)
    {
        try
        {
            outStream.EndWrite(r);
        }
        catch (Exception ex)
        {
            Log("OnWrite--->>>" + ex.Message);
        }
    }

    #endregion

    #region 读   
   
    /// <summary>
    /// 读取消息
    /// </summary>
    void OnRead(IAsyncResult asr) {
        int bytesRead = 0;
        try {
            lock (client.GetStream()) {         //读取字节流到缓冲区
                bytesRead = client.GetStream().EndRead(asr);
            }
            if (bytesRead < 1) {                //包尺寸有问题，断线处理
                //OnDisconnected(DisType.Disconnect, "bytesRead < 1");
                Log("包长度为零为废包，丢弃~");
                return;
            }
            OnReceive(byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
            lock (client.GetStream()) {         //分析完，再次监听服务器发过来的新消息
                Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
                client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
            }
        } catch (Exception ex) {
            //PrintBytes();
            OnDisconnected(DisType.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    void OnReceive(byte[] bytes, int length)
    {
        memStream.Seek(0, SeekOrigin.End);
        memStream.Write(bytes, 0, length);
        //Reset to beginning
        memStream.Seek(0, SeekOrigin.Begin);
        while (RemainingBytes() > 2)
        {
            int messageLen = reader.ReadInt32();
            messageLen = System.Net.IPAddress.NetworkToHostOrder(messageLen);
            if (RemainingBytes() >= messageLen)
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(reader.ReadBytes(messageLen));
                ms.Seek(0, SeekOrigin.Begin);
                OnReceivedMessage(ms);
            }
            else
            {
                //Back up the position two bytes
                memStream.Position = memStream.Position - 2;
                break;
            }
        }
        //Create a new stream with any leftover bytes        
        byte[] leftover = reader.ReadBytes((int)RemainingBytes());
        memStream.SetLength(0);     //Clear
        memStream.Write(leftover, 0, leftover.Length);
    }


    /// <summary>
    /// 接收到消息
    /// </summary>
    /// <param name="ms"></param>
    void OnReceivedMessage(MemoryStream ms)
    {
        BinaryReader r = new BinaryReader(ms);
        byte[] message = r.ReadBytes((int)(ms.Length - ms.Position));
        ByteBuffer buffer = new ByteBuffer(message);
        int mainId = buffer.ReadInt();
        mainId = System.Net.IPAddress.NetworkToHostOrder(mainId);
        string content = null;
        if ((message.Length - 4) > 0) { 
           content = buffer.ReadString(message.Length - 4);
        }
        Log("收到消息" + mainId);
        InQueue.Enqueue(new KeyValuePair<Protocol.PROTOCOL, string>((Protocol.PROTOCOL)mainId, content));
    }
    #endregion

    #region 公用方法

    public NetworkManager.STATE GetConnectState()
    {
        return state;
    }


    public void SendProtocolBytes(Protocol.PROTOCOL protocol, byte[] strByte)
    {
        ByteBuffer b = new ByteBuffer();
        b.WriteInt(System.Net.IPAddress.HostToNetworkOrder((int)protocol));
        Log("添加消息[ " +  protocol.ToString() + " ]进入队列");

        if (strByte != null) { 
             int packCount = Mathf.CeilToInt((float)strByte.Length / 117f);
             List<byte> result = new List<byte>();
             for (int i = 0; i < packCount; i++)
             {
                 int len = (strByte.Length - 117 * i);
                 if (len > 117) len = 117;

                 byte[] cutByte = new byte[len];
                 for (int j = 0; j < len; j++)
                 {
                     cutByte[j] = strByte[i * 117 + j];
                 }
                 result.AddRange(RSA.Encrypt(cutByte));
             }
             byte[] r = result.ToArray();
             b.WriteBytes(r);
         }
         outQueue.Enqueue(new KeyValuePair<NetActionType, ByteBuffer>(NetActionType.Message, b));
     }


    public void SendProtocol(Protocol.PROTOCOL protocol, Dictionary<string, object> data)
    {
        if (data == null)
        {
            SendProtocolBytes(protocol, null);
        }
        else { 
            string s = MiniJSON.Json.Serialize(data);
            Byte[] strByte = System.Text.Encoding.UTF8.GetBytes(s);
            SendProtocolBytes(protocol, strByte);
        }
    }

     /// <summary>
     /// 当消息处理完之后登出
     /// </summary>
     public void QueueLogout()
     {
         outQueue.Enqueue(new KeyValuePair<NetActionType, ByteBuffer>(NetActionType.Logout, null));
     }

     /// <summary>
     /// 立刻关闭链接
     /// </summary>
     public void CloseConnection()
     {
         if (client != null)
         {
             if (client.Connected) client.Close();
             client = null;
         }
         state = NetworkManager.STATE.DISCONNECT;
     }

    /// <summary>
    /// 添加协议回调
    /// </summary>
    /// <param name="callback"></param>
     public void AddCallback(ProtocolCallback callback)
     {
         if (protocolCallback.ContainsKey(callback.protocol))
         {
             protocolCallback.Remove(callback.protocol);
         }
         protocolCallback.Add(callback.protocol, callback);
     }

    #endregion

    #region 其他方法

     public List<string>  debugStr = new List<string>();
     public bool recordLog = false;
     const int MAX_LOG_LENGTH = 50;

     public void Log(string log)
     {
         if (recordLog)
         {
             debugStr.Add(("[SOCKET]" + log));
             if (debugStr.Count > MAX_LOG_LENGTH)
             {
                 debugStr.RemoveAt(0);
             }
         }
         else
         {
             Debug.Log("[SOCKET]"+log);
         }
     }

    /// <summary>
    /// 返回日志
    /// </summary>
    /// <returns></returns>
     public string GetLog()
     {
         return String.Join("\n" , debugStr.ToArray());
     }


     /// <summary>
    /// 打印字节
    /// </summary>
    /// <param name="bytes"></param>
    void PrintBytes() {
        string returnStr = string.Empty;
        for (int i = 0; i < byteBuffer.Length; i++) {
            returnStr += byteBuffer[i].ToString("X2");
        }
        Log(returnStr);
    }
    
    /// <summary>
    /// 剩余的字节
    /// </summary>
    private long RemainingBytes() {
        return memStream.Length - memStream.Position;
    }
     #endregion

}



/// <summary>
/// 加密
/// </summary>
public class RSA
{    
    /// <summary>
    /// 这个实例不初始化
    /// </summary>
   static public RSACryptoServiceProvider encryptProvider;
   static public RSACryptoServiceProvider decryptProvider;

   static public byte[] Encrypt(byte[] b)
    {
        if (encryptProvider == null) { 
            sbyte[] skey = new sbyte[] { 0, -96, 115, -72, 93, -76, 90, 66, -49, 41, 43, 67, 120, 107, -9, 30, 80, 101, -98, 14, 85, 29, -44, 25, -80, -37, -86, 44, 77, -104, 9, 116, 105, 54, -63, 39, -43, 123, 112, 21, 38, -36, -43, -59, 44, 96, 7, -120, 47, 36, -122, 43, 40, 111, 54, -114, -12, -47, 42, 119, -42, 113, 3, -116, -21, -12, 107, 108, 125, -33, -23, -70, -17, -74, -12, 71, -79, 104, -59, 6, -77, 65, 31, -116, -121, -128, 103, 82, 57, -81, 53, 44, 57, 20, 101, -37, 30, 107, -45, 106, 12, 114, -99, 34, -115, 94, -74, -72, -30, 52, 116, 49, 87, 46, 112, -61, -40, 5, -121, 54, 112, 124, -40, 14, 125, 19, -103, 68, 43 }; ;
            byte[] key = Array.ConvertAll(skey, (a) => (byte)a);
            RSAParameters p = new RSAParameters();
            p.Exponent = new byte[] { 1, 0, 1 };
            p.Modulus = key;
            encryptProvider = new RSACryptoServiceProvider();
            encryptProvider.ImportParameters(p);
        }
        return encryptProvider.Encrypt(b,false);
    }


    static public byte[] Decrypt(byte[] b){
        if (decryptProvider == null) decryptProvider = new RSACryptoServiceProvider(1024);
         return decryptProvider.Decrypt(b,false);
    }

    static public byte[] GetModulus()
   {
       if (decryptProvider == null) decryptProvider = new RSACryptoServiceProvider(1024);
       RSAParameters p = decryptProvider.ExportParameters(true);
       Debug.Log("Modulus上传长度:" + p.Modulus.Length);
       NetworkManager.outputBytes(p.Modulus);
       return p.Modulus;
   }

   static public byte[] GetExponent()
   {
       RSAParameters p = decryptProvider.ExportParameters(true);
       Debug.Log("Exponent上传长度:" + p.Exponent.Length);
       NetworkManager.outputBytes(p.Exponent);
       return p.Exponent;
   }
}
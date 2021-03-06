//=================================================================================
//
//        Copyright (C) 2015 JMS justnetfly@qq.com
//        All rights not reserved
//        description :随意改.现在提供的接口只有Login()登陆。调完这个接口之后就不需要鸟了。各回各家各找各妈。
//        http://www.o-dear-fly.cn
//
//==================================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Protocol
{
    /// <summary>
    /// 一些网络的用户数据。
    /// </summary>
    static public NetworkUserData userData = new NetworkUserData();

    public enum PROTOCOL
    {
        请求用户ID=106,
        请求用户ID结果=1060,

        登陆=105,
        登陆结果=1050
    }
    

    #region 登陆协议
    static TryTime loginTryTime = new TryTime(5);
    static int gameID=0;

    /// <summary>
    /// 登陆流程, 注意，需要修改gameID为相应的游戏ID
    /// </summary>
    static public void Login()
    {
        /*
        阿里云TVOS(合作版)：33F276D0-0890-438E-AF69-6F0CEB4BF7F0
        雅佳：2E69F238-1D38-4E71-BBA9-F39155C2DC38
        金亚：40C39DEF-B672-4808-B026-26EACFB43211
        外星游戏(自研游戏版)：584208C6-A4A6-48AA-B448-9C649C232FF7
        日松：00442C84-C097-44B4-9B69-443F06243CDF
         */
        
        //没错，就是改这里。不清楚你有多少个渠道要发，随意加
        string userID = PlayerPrefs.GetString("USER_ID", null);        
        
        string channelId = "";
        //if (Version.IsWX())
        //{
        //    channelId = "33F276D0-0890-438E-AF69-6F0CEB4BF7F0";
        //    gameID = 110; //提供包名给卢桂福，他会提供给你这个ID;
        //}
        //else if (Version.IsOS())
        //{
        //    channelId = "584208C6-A4A6-48AA-B448-9C649C232FF7";
        //    gameID = 109;
        //}
        //else
        //{
        //    channelId = "unknow channelid";
        //}


        if(userID==null || userID==""){
            Dictionary<string, object> sendData = new Dictionary<string, object>() { { "channelId", channelId }, { "gameId", gameID } };
            NetworkManager.instance.SendProtocol(PROTOCOL.请求用户ID, sendData, new ProtocolCallback()
            {
                protocol = PROTOCOL.请求用户ID结果,
                callback = (data) =>
                {
                    if (data.ContainsKey("accountName")) userData.accountName = (string)data["accountName"];
                    if (data.ContainsKey("specName")) userData.specName = (string)data["specName"];
                    if (data.ContainsKey("spritName")) userData.spritName = (string)data["spritName"];
                    if (data.ContainsKey("playerId")) userData.playerId = (string)data["playerId"];

                    if (userData.playerId != null) {
                        PlayerPrefs.SetString("USER_ID", userData.playerId);
                        PlayerPrefs.Save();
                        Debug.Log("得到用户ID：" + userData.playerId);
                        LoginSubmitID(userData.playerId);
                    }
                    else
                    {
                        Debug.Log("从服务器取ID协议返回错误！");
                    }
                }
            });
        }else{
            Debug.Log("直接使用ID登陆：" + userID);
            LoginSubmitID(userID);
        }        
    }

    /// <summary>
    /// 登陆
    /// </summary>
    /// <param name="userID"></param>
    static private void LoginSubmitID(string userID)
    {
       
         NetworkManager.instance.SendProtocol( PROTOCOL.登陆,
                new Dictionary<string, object> { { "playerId", userID }, { "gameId", gameID } },                
                new ProtocolCallback(){ protocol = PROTOCOL.登陆结果, callback=(data)=>{
                    if ((bool)data["result"] == true)
                        Debug.Log("登陆成功！");
                    else {
                        Debug.LogWarning("登陆失败！原因：" + data["msg"]);
                        loginTryTime.AddTime();
                        if (loginTryTime.CanTry()) {
                            CleanUserID();
                            Debug.Log("重试从服务器取得ID");
                            Login();
                        }
                        else
                        {
                            Debug.LogWarning("重试5次无法得到服务器ID，好吧，服务器又不稳定了~找卢同学吧。");
                            loginTryTime.Reset();
                        }
                    }
                }} 
         );
    }

    /// <summary>
    /// 清除掉本地的ID
    /// </summary>
    static public void CleanUserID()
    {
        PlayerPrefs.SetString("USER_ID", null); //消除ID
        PlayerPrefs.Save();
    }

    #endregion
}

/// <summary>
/// 用户数据
/// </summary>
public class NetworkUserData
{
    public string accountName;
    public string specName;
    public string spritName;
    public string playerId;
    //private Date createTime;
    private string createType;
}

public class TryTime
{
    public int nowTime = 0;
    public int maxTime = 5;

    public TryTime(int  maxTime)
    {
        this.maxTime = maxTime;
    }

    public void Reset()
    {
        nowTime = 0;
    }

    public bool CanTry()
    {
        return nowTime < maxTime;
    }

    public void AddTime()
    {
        nowTime++;
    }
}
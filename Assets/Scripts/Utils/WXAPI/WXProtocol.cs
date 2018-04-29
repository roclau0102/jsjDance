using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class WXProtocol : MonoBehaviour
{

    //类型ID
    public const int TYPE_ID = 600;
    //游戏ID
    public const int GAME_ID = 5009;
    static public string sdk = "123456";
    static public string product = "17";
    static public string uid = "54:5b:cf:8c:82:a1";
    static public string macUID = "53:5b:cf:8c:82:a1";
    static public string userName = "";
    static public string userPwd = "";
    static public string tableName;
    static public int updateID;
    static public int insertID;
    static public int selectID;
    static public int LoginID;
    static public int Money;

    static public WXProtocol _instance;
    public API_Login login;
    public API_Money money;

    /// <summary>
    /// 用来从主线程执行内容的回调,否则会各种不同线程导致的问题
    /// </summary>
    public System.Action mainThreadUpdateCall;

    


    static public WXProtocol instance
    {
        get
        {
            if (_instance == null)
            {                
                GameObject go = new GameObject();
                go.name = "WXAPI";
                _instance = go.AddComponent<WXProtocol>();
                _instance.Init();                
            }
            return _instance;
        }
    }




    public void Init()
    {
        login = this.gameObject.AddComponent<API_Login>();
        money = this.gameObject.AddComponent<API_Money>();
        http = new HttpHelper();
    }


    public void ExchangeMoney(int moneyCount, Action<bool, string> callback)
    {
        money.ExchangeMoney(moneyCount, callback); 
    }

    public void Init(string tableName,int selectID, int updateID, int insertID)
    {
        WXProtocol.updateID = updateID;
        WXProtocol.tableName = tableName;
        WXProtocol.insertID = insertID;
        WXProtocol.selectID = selectID;

#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass jc;
            AndroidJavaObject jo;
            jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            WXProtocol.sdk = jo.Call<string>("getSDK");
            WXProtocol.product = jo.Call<string>("getProduct");
            WXProtocol.uid = jo.Call<string>("getSharedUid");
            WXProtocol.macUID = jo.Call<string>("getId");
            if (WXProtocol.uid == "" || WXProtocol.uid == null)
            {
                WXProtocol.uid = WXProtocol.macUID;
            }
        }
        else
        {
            string backUid = PlayerPrefs.GetString("uid", "");
            if (backUid != "")
            {
                WXProtocol.uid = backUid;
            }
        }
#endif
    }


    HttpHelper http;

    static public bool CheckSuccess(Dictionary<string, object> json)
    {
        if (json.ContainsKey("message"))
        {
            if (json["message"] is String)
            {
                string r = json["message"].ToString();
                return  r== "true";
            }
            else
            {
                return false;
            }
        }
        return false;
    }



    public void GetJson(string url, Action<Dictionary<string, object>> onCompleteCall, Action<string> onFailedCall)
    {
        Log(url);
        http.AsyDownLoad(url, (result) =>
        {
            //跨线程调用需要加一层
            mainThreadUpdateCall = () =>
            {
                Dictionary<string, object> data = null;
                try
                {
                    data = MiniJSON.Json.Deserialize(result) as Dictionary<string, object>;
                }
                catch (Exception e)
                {
                    Debug.Log("登陆返回字符串解析失败:" + result + "," + e.ToString());
                }
                onCompleteCall(data);
            };
        }, (result)=>{
            mainThreadUpdateCall = () =>{
                onFailedCall(result);
            };
        });
        checkingTime = Time.time;
    }

    float checkingTime = 0;


    static public void Log(string str)
    {
        Debug.Log("[WX API]" + str);
    }


    void Update()
    {
        if (checkingTime!=-1)
        {
            if (http.state == HttpHelper.STATE.DONE || http.state == HttpHelper.STATE.READY)
            {
                checkingTime = -1;
            }
            else
            {
                if (Time.time - checkingTime > 3)
                {
                    http.Restart();
                    checkingTime = Time.time;
                }
            }
        }

        if (mainThreadUpdateCall != null)
        {
            mainThreadUpdateCall();
            mainThreadUpdateCall = null;
        }
    }
}

public class HTTP_TIMEOUT
{
    public HttpHelper http;
    public float time;
}
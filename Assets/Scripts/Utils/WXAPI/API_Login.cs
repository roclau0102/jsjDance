using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class API_Login : MonoBehaviour  {

    Action<LoginResult> doneCallback;
    Action<Dictionary<string, object>> gameData;


    public enum OPERATEION
    {
        LOGIN,
        SWITCH_USER,
        EDIT_USER_PWD
    }

    OPERATEION curAction = OPERATEION.LOGIN;

    public OPERATEION GetAction()
    {
        return curAction;
    }

    #region 切换用户 


    public void UpdateUserNameAndPWD(string newUID, string backUID,  Action<bool> callback)
    {
        string sql = "http://api.waixing.com:8090/client/doMb.action?sql=update games_users set  uid='" + newUID + "', newtime = GETDATE()  where  uid = '"+backUID+"' &id=31";
            WXProtocol.instance.GetJson(sql, (json) =>
            {
                callback(WXProtocol.CheckSuccess(json));
            }, (failed) =>
            {
                callback(false);
            });
    }

    bool needSaveUID = false;

    void Update()
    {
        if (needSaveUID)
        {
            SaveUID(WXProtocol.uid);
            needSaveUID = false;
        }
    }


    /// <summary>
    /// 切换到空用户
    /// </summary>
    /// <param name="macUid"></param>
    /// <param name="doneCall"></param>
    public void SwitchUser(string macUid, Action<LoginResult> doneCall)
    {
        this.doneCallback = doneCall;
        CheckUid(macUid, (b, jsonData) =>
        {
            if (b) //理论上不应该存在
            {
                //用户存在的话,更新用户名密码为空吧？
                var list = jsonData["data"] as List<object>;
                var dic = list[0] as Dictionary<string, object>;
                String backUID = dic["uid"].ToString();

                string sql = "http://api.waixing.com:8090/client/doMb.action?sql=update games_users set  newtime = GETDATE()  where  uid = '" + backUID + "' &id=31";
                WXProtocol.instance.GetJson(sql, (json) =>
                {
                    if (WXProtocol.CheckSuccess(json))
                    {
                        SetLoginID(jsonData);
                        WXProtocol.uid = macUid;
                        SaveUID(WXProtocol.uid);
                        GetGameData();
                    }
                    else
                    {
                        if (doneCall != null) doneCall(new LoginResult() { reason = json["data"].ToString() });
                    }
                }, (failed) =>
                {
                    if (doneCall != null) doneCall(new LoginResult() { reason = failed });
                });
            }
            else
            {
                CreateUser(true);
            }
        });
    }


    /// <summary>
    /// 切换用户
    /// </summary>
    /// <param name="username"></param>
    /// <param name="pwd"></param>
    /// <param name="doneCall"></param>
    public void SwitchUser(String username, String pwd, Action<LoginResult> doneCall, Action failedCall=null)
    {
        if (username == WXProtocol.userName)
        {
            doneCall(new LoginResult() { reason = "不能切换您当前的用户名" });
            return;
        }

        this.doneCallback = doneCall;

        curAction = OPERATEION.SWITCH_USER;
        //检查用户是否存在
        CheckUser(username, pwd, (b, jsonData) =>
        {
            if (b)
            {
                //用户存在的话，更新UID为新的用户ID
                var list = jsonData["data"] as List<object>;
                var dic = list[0] as Dictionary<string, object>;
                String backUID = dic["uid"].ToString();
                String uid = GetNewUID();
                UpdateUserNameAndPWD(uid, backUID,  (bb)=>{
                    if (bb)
                    {                        
                        SetLoginID(jsonData);
                        //保存UID
                        WXProtocol.uid = uid;
                        SaveUID(uid);
                        needSaveUID = true;
                        //游戏逻辑，需要读取当前某条表数据再返回
                        GetGameData(); 
                    }
                    else
                    {
                        if (doneCall != null) doneCall(new LoginResult() { reason = "更新用户名密码失败" });
                    }    
                });
            }
            else
            {
                if (failedCall != null)
                {
                    failedCall();
                }
                else
                {
                    if (doneCall != null) doneCall(new LoginResult() { reason = "没有这个用户或者用户名密码错误" });
                }                
            }
        });

    }

    string GetNewUID()
    {
        System.DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        System.DateTime nowTime = System.DateTime.Now;        
        string newUID = ((long)(nowTime - startTime).TotalMilliseconds).ToString();
        return newUID;
    }

    


    private void SaveUID(string uid)
    {
        //调用底层的保存UID
        PlayerPrefs.SetString("uid", uid);
        PlayerPrefs.Save();

        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass jc;
            AndroidJavaObject jo;
            jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("setSharedUid", uid);
        }
    }


    private void GetGameData()
    {
        WXProtocol.Log("得到游戏表数据");


        string sql = "http://api.waixing.com:8090/client/getMb.action?sql=select * from " + WXProtocol.tableName
        + " where id = " + WXProtocol.LoginID + "&id= " + WXProtocol.selectID;

        WXProtocol.instance.GetJson(sql, (json1) =>
        {
            if (WXProtocol.CheckSuccess(json1))
            {
                //有数据
                doneCallback(new LoginResult() { success = true, data = json1 });
            }
            else
            {
                //没有数据
                //新建一条数据
                WXProtocol.Log(json1["data"].ToString());
                WXProtocol.Log("新建游戏数据");
                WXProtocol.instance.GetJson("http://api.waixing.com:8090/client/doMb.action?sql=insert into " + WXProtocol.tableName
                 + "(id,data) values(" + WXProtocol.LoginID + ", '{}')&id=" + WXProtocol.insertID, (json2) =>
                 {
                     if (WXProtocol.CheckSuccess(json2))
                     {
                         doneCallback(new LoginResult() { success = true });
                     }
                     else
                     {
                         doneCallback(new LoginResult() { success = false, reason = json2["data"].ToString() });
                     }
                 }, (s) => {
                     doneCallback(new LoginResult() {  reason = s });
                 });
            }
        }, (failedReason) =>
        {
            doneCallback(new LoginResult() { reason = failedReason });
        });
    }


    private void CheckUid(string uid, Action<bool, Dictionary<string, object>> callback)
    {
        string sql = "http://api.waixing.com:8090/client/getMb.action?sql=select * from games_users where uid = '" + uid + "'";
        sql += "&id=30";


        WXProtocol.instance.GetJson(sql, (json) =>
        {
            if (WXProtocol.CheckSuccess(json))
            {
                callback(true, json);
            }
            else
            {
                //没有这个用户的数据
                callback(false, null);
            }
        }, (failedReason) =>
        {
            callback(false, null);
        });
    }
    


    private void CheckUser(string user, string pwd, Action<bool, Dictionary<string,object>> callback)
    {
        string sql = "http://api.waixing.com:8090/client/getMb.action?sql=select * from games_users where name = '" + user + "'";
        if (pwd != null)
        {
            sql += " and pwd = '" + pwd + "' ";
        }
        sql += "&id=49";

        WXProtocol.instance.GetJson(sql, (json) =>
        {
            if (WXProtocol.CheckSuccess(json))
            {                
                callback(true, json);
            }
            else
            {
                //没有这个用户的数据
                callback(false,null);
            }
        }, (failedReason) =>
        {
            callback(false,null);
        });
    }


    private System.DateTime GetTime(string timeStamp)
    {
        System.DateTime dtStart = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        long lTime = long.Parse(timeStamp + "0000000");
        System.TimeSpan toNow = new System.TimeSpan(lTime);
        return dtStart.Add(toNow);
    }

    #endregion


    #region 修改用户名密码

    public void ChangeUserAndPwd(String user, String pwd, Action<LoginResult> completeCall)
    {
        WXProtocol.Log("修改用户名和密码");

        curAction = OPERATEION.EDIT_USER_PWD;

        CheckUser(user, null, (b, d) =>
        {
            if (b)
            {
                List<object> list = d["data"] as List<object>;
                if (list.Count > 0)
                {
                    var dict = list[0] as Dictionary<string, object>;
                    if (dict["uid"].ToString() == WXProtocol.uid)
                    {
                        b = false;
                    }
                }
            }
           

            if (!b)
            {
                this.doneCallback = completeCall;
                string sql = "http://api.waixing.com:8090/client/getMb.action?sql=update games_users set  name='" + user + "', pwd='" + pwd + "',  newtime = GETDATE()  where uid='" + WXProtocol.uid + "'&id=31";
                WXProtocol.instance.GetJson(sql, (json) =>
                {
                    WXProtocol.userName = user;
                    WXProtocol.userPwd = pwd;
                    if (doneCallback != null) doneCallback(new LoginResult() { success = true });
                }, (faileStr) =>
                {
                    if (doneCallback != null) doneCallback(new LoginResult() { reason = "您当前没有网络无法连接服务器" });
                });
            }
            else
            {
                if (doneCallback != null) doneCallback(new LoginResult() { reason = "用户名已存在,请更换用户名重试" });
            }
        });
    }
    #endregion



    #region 登陆

    /// <summary>
    /// 设置登陆表的流水ID
    /// </summary>
    /// <param name="json"></param>
    public void SetLoginID(Dictionary<string, object> json)
    {
        List<object> list = json["data"] as List<object>;
        if (list.Count > 0)
        {
            Dictionary<string, object> data = list[0] as Dictionary<string, object>;
            if(data.ContainsKey("id")){
                WXProtocol.LoginID = int.Parse(data["id"].ToString());
            }

            if (data.ContainsKey("money"))
            {
                WXProtocol.Money = int.Parse(data["money"].ToString());
            }

            if (data.ContainsKey("name"))
            {
                WXProtocol.userName = (data["name"].ToString());
            }

            if (data.ContainsKey("pwd"))
            {
                WXProtocol.userPwd = (data["pwd"].ToString());
            }
        }
    }


    

    /// <summary>
    /// 登你妹的陆啊
    /// </summary>
    /// <param name="doneCall"></param>
    public void Login(Action<LoginResult> doneCall, bool checkUserBefore=true)
    {
        curAction = OPERATEION.LOGIN;
        doneCallback = doneCall;
        //用本机的ＵＩＤ注册一个
        WXProtocol.instance.GetJson("http://api.waixing.com:8090/client/getMb.action?sql=select * from games_users where uid = '" + WXProtocol.uid + "' &id=30", (json) =>
        {
            if (WXProtocol.CheckSuccess(json))
            {
                SetLoginID(json);
                WXProtocol.Log("UID" + WXProtocol.uid + "已注册，刷新上线时间为现在");
                RefreshLogin();
            }
            else
            {
                WXProtocol.Log("UID" + WXProtocol.uid + "未注册，新注册用户");
                CreateUser(); //需要拿上次登陆的ＵＩＤ来登陆，除非用户切换了账号
            }
        }, (failedReason) =>
        {
            LoginResult data = new LoginResult() { success = false, reason = failedReason };
            doneCall(data);
        });        
    }


    public void CreateUser(bool useMacUID=false)
    {
        string uid = useMacUID ? WXProtocol.macUID : WXProtocol.uid;

        WXProtocol.instance.GetJson("http://api.waixing.com:8090/client/doMb.action?sql=insert into games_users (uid,product,sdk,type,game,time) values('" + uid + "','" + WXProtocol.product + "','" + WXProtocol.sdk + "','" + WXProtocol.TYPE_ID + "', '" + WXProtocol.GAME_ID + "', GETDATE()) &id=32",
         (json) =>
         {
             if (WXProtocol.CheckSuccess(json))
             {
                 WXProtocol.uid = uid;
                 WXProtocol.userPwd = "";
                 WXProtocol.userName = "";
                 SaveUID(uid);
                 WXProtocol.instance.GetJson("http://api.waixing.com:8090/client/getMb.action?sql=select * from games_users where uid = '" + WXProtocol.uid + "'&id=30", (d) =>
                 {
                     if (WXProtocol.CheckSuccess(d))
                     {
                         SetLoginID(d);
                         GetGameData();
                     }
                     else
                     {
                         if (doneCallback != null) doneCallback(new LoginResult() { reason = d["data"].ToString() });
                     }
                 }, (f) =>
                 {
                     if (doneCallback != null) doneCallback(new LoginResult() { reason = f });
                 });                
             }
             else
             {
                 if (doneCallback != null) doneCallback(new LoginResult() { reason = json["data"].ToString() });
             }
         }, LoginFailed);
    }

    public void RefreshLogin()
    {
        WXProtocol.instance.GetJson("http://api.waixing.com:8090/client/doMb.action?sql=update games_users set  newtime = GETDATE() where uid = '" + WXProtocol.uid + "' &id=31",
        (json) =>
        { 
            if (WXProtocol.CheckSuccess(json))
            {
                GetGameData();
            }
            else
            {
                if (doneCallback != null) doneCallback(new LoginResult() { reason = json["data"].ToString() });
            }            
        }, LoginFailed);
    }



    void LoginFailed(string result)
    {
        LoginResult data = new LoginResult();
        data.reason = "JSON 无法解析或者访问失败:" + result;
        if (doneCallback != null) doneCallback(data);
    }

    #endregion
}



public class LoginResult
{
    public bool success = false;
    public Dictionary<string,object> data;
    public string reason;
    public int money;
}
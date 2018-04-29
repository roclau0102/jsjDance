using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class API_Money: MonoBehaviour  {


    public void TestAddMoney(int m)
    {
        string sql = "http://api.waixing.com:8090/client/getMb.action?sql=update games_users set money = " + m + " where id = " + WXProtocol.LoginID + "&id=31";

        WXProtocol.instance.GetJson(sql, (json) =>
        {
            if (WXProtocol.CheckSuccess(json))
            {
                Debug.Log("加"+m+"成功");
            }else{
                Debug.Log("加" + m + "失败");
            }
        }, (f) => {
            Debug.Log("加" + m+ "失败:"+f);
        
        });
    }


    public void ExchangeMoney(int money, Action<bool, string> callback)
    {
        string sql = "http://api.waixing.com:8090/client/getMb.action?sql=select * from games_users where uid ='" + WXProtocol.uid + "'&id=30";

        WXProtocol.instance.GetJson(sql, (json) =>
        {
            if (WXProtocol.CheckSuccess(json))
            {
                List<object> data = json["data"] as List<object>;
                Dictionary<string, object> per = data[0] as Dictionary<string, object>;

                if (per.ContainsKey("money"))
                {
                    int m = int.Parse(per["money"].ToString());
                    if (money > m)
                    {
                        callback(false, "您的外星币不足以支付这次兑换");
                        return;
                    }

                    m -= money;

                    

                    sql = "http://api.waixing.com:8090/client/doMb.action?sql=update games_users set money="+ m +" where uid = '" + WXProtocol.uid + "'&id=31";
                    WXProtocol.instance.GetJson(sql, (json1) =>
                    {
                        WXProtocol.Money = m;
                        callback(true, "您成功兑换了"+money+"游戏币");
                    }, (failedReson1) => {
                         callback(false,  "兑换失败:" + json);
                    });
                }
                else
                {
                    if (json.ContainsKey("data"))
                    {
                        callback(false, "兑换失败:" + json["data"]);
                    }
                    else
                    {
                        callback(false, "兑换失败");
                    }
                }
            }           
        }, (failedReason) =>
        {
            
        });
    }
	
}

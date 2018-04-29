using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.IO;



public class HttpHelper
{
    public enum STATE
    {
        READY,
        DOING,
        DONE
    }


    public STATE state
    {
        get ; private set;
    }
    
    public HttpHelper()
    {

    }

    System.Action<string> onCompleteCallback;
    System.Action<string> onFailedCallback;
    HttpWebRequest httpRequest;
    string url;

    public void AsyDownLoad(string url, Action<string> onCompleteCall, Action<string> onFailedCall)
    {
        if (state == STATE.DOING)
        {
            Debug.LogWarning("同时只能有一个HTTP访问");
            return;
        }
        state = STATE.DOING;
        this.url = url;
        this.onCompleteCallback = onCompleteCall;
        this.onFailedCallback = onFailedCall;

        httpRequest = WebRequest.Create(url) as HttpWebRequest;
        httpRequest.BeginGetResponse(new AsyncCallback(ResponseCallback), httpRequest);
    }

    void ResponseCallback(IAsyncResult ar)
    {
        state = STATE.DONE;
        HttpWebRequest req = ar.AsyncState as HttpWebRequest;
        if (req == null) return;
        HttpWebResponse response = null;
        try
        {
            response = req.EndGetResponse(ar) as HttpWebResponse;
        }
        catch
        {
            if (onFailedCallback != null)
            {
                onFailedCallback("您当前没有网络无法连接服务器");
            }            
            return;
        }
         
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (onFailedCallback != null)
            {
                onFailedCallback(response.StatusCode.ToString());
            }
            response.Close();            
            return;
        }

        Stream responseStream = response.GetResponseStream();
        Encoding htmlEncoding = Encoding.GetEncoding("utf-8"); //或者gb2312;
        StreamReader sr = new StreamReader(responseStream, htmlEncoding);
        string result = sr.ReadToEnd();
        sr.Close();
        sr = null;
        httpRequest = null;
        responseStream.Close();
        if(onCompleteCallback!=null)onCompleteCallback(result);
        
    }

    internal void Restart()
    {
        state = STATE.DOING;
        if (httpRequest != null)
        {
            httpRequest.Abort();
            httpRequest = null;
        }
        AsyDownLoad(this.url, this.onCompleteCallback, this.onFailedCallback);
    }
}


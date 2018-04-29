using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;

public class JLoader : MonoBehaviour {
	private List<JLoaderInfo> queue = new List<JLoaderInfo>();
	private List<JLoaderInfo> deleteItem = new List<JLoaderInfo>();

	static private JLoader _instance=null;

	static public JLoader instance{
		get{
			if(_instance==null){
				GameObject go = new GameObject();
				go.name = "JLoader";
				_instance = go.AddComponent<JLoader>();
			}
			return _instance;
		}
	}

	void Update(){
        for (int i = 0, c = queue.Count; i < c; i++)
        {
            JLoaderInfo info = queue[i];
            //if (info.www.isDone)
            //{
            //    if (info.www.error != null)
            //    {
            //        //Debug.Log(info.www.url + ":加载失败");
            //        info.callback(DOWNLOAD_TYPE.FAILED, info);
            //        queue.Remove(info);
            //    }
            //    else
            //    {
            //        info.callback(DOWNLOAD_TYPE.SUCCESS, info);
            //        queue.Remove(info);
            //    }
            //    //info.www.Dispose();
            //    //deleteItem.Add(info);
            //}
            //else
            //{
                info.callback(DOWNLOAD_TYPE.PROGRESS, info);
            //}
        }

        //if (deleteItem.Count > 0)
        //{
        //    for (int i = 0, c = deleteItem.Count; i < c; i++)
        //    {
        //        queue.Remove(deleteItem[i]);
        //    }
        //    deleteItem.Clear();
        //}

    }

	public enum DOWNLOAD_TYPE{
		PROGRESS,
		FAILED,
		SUCCESS
	}

    public JLoaderInfo Load(string url, object userdata, Action<DOWNLOAD_TYPE, JLoaderInfo> callback)
    {
		JLoaderInfo info = new JLoaderInfo();
		info.url = url;
		info.callback = callback;
        //info.www = new WWW(url);
        info.userData = userdata;
        queue.Add(info);
        if (url.EndsWith("wav")||url.EndsWith("mp3"))
            StartCoroutine(DownloadSong(info));
        else
            StartCoroutine(Download(info));
        return info;
	}

    IEnumerator Download(JLoaderInfo info)
    {
        using (info.www = UnityWebRequest.Get(info.url))
        {
            info.result = info.www.Send();
            yield return info.result;
          
            if (info.www.isError)
            {
                Debug.Log(info.www.url + ":下载失败-" + info.www.error);
                info.callback(DOWNLOAD_TYPE.FAILED, info);
            }
            else
            {
                Debug.Log(info.www.url + ":下载成功");
                info.callback(DOWNLOAD_TYPE.SUCCESS, info);
            }
            queue.Remove(info);
        }

    }

    IEnumerator DownloadSong(JLoaderInfo info)
    {
        if (info.url.EndsWith("mp3"))
            info.www = UnityWebRequest.GetAudioClip(info.url, AudioType.MPEG);
        else if (info.url.EndsWith("wav"))
            info.www = UnityWebRequest.GetAudioClip(info.url, AudioType.WAV);

        info.result = info.www.Send();
        yield return info.result;

        if (info.www.isError)
        {
            Debug.Log(info.www.url + ":下载失败");
            info.callback(DOWNLOAD_TYPE.FAILED, info);
        }
        else
        {
            Debug.Log(info.www.url + ":下载成功");
            info.callback(DOWNLOAD_TYPE.SUCCESS, info);
        }
        info.www.Dispose();
        queue.Remove(info);

    }

    internal void Remove(JLoaderInfo info)
    {
        if (info != null && info.www != null) info.www.Abort();
        //queue.Remove(info);
    }
}


public class JLoaderInfo{
	public string url;
	public System.Action<JLoader.DOWNLOAD_TYPE,JLoaderInfo> callback;
	public UnityWebRequest www;
    public object userData;
    public AsyncOperation result;
}

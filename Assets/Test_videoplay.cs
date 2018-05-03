using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.IO;

public class Test_videoplay : MonoBehaviour
{

    public VideoPlayer video;

    // Use this for initialization
    void Start()
    {
        //StartCoroutine(LoadVideo(Application.streamingAssetsPath + "/2.mp4"));
        //string url = Application.streamingAssetsPath + "/2.mp4";
        //Debug.Log(url.Substring(url.LastIndexOf('/') + 1, url.Length - url.LastIndexOf('/') - 1));
        StartCoroutine(PlayVideo(Application.streamingAssetsPath + "/videos/videobundle"));

        //var ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/videos");

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(ao.progress);
    }

    IEnumerator LoadVideo(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (!www.isNetworkError || www.error == null)
            {
                Debug.Log("request =" + www.error);
            }
            Debug.Log("Done - " + www.isDone);

            byte[] videoBytes = www.downloadHandler.data;
            //string fileName = url.Substring(url.LastIndexOf('/') + 1, url.Length - url.LastIndexOf('/') - 1);
            string savePath = Path.Combine(Application.persistentDataPath, /*fileName*/"2.mp4");
            Debug.Log(savePath);
            File.WriteAllBytes(savePath, videoBytes);

            StartCoroutine(PlayVideoAtPath(savePath));
        }
    }

    IEnumerator PlayVideoAtPath(string url)
    {
        video.source = VideoSource.Url;
        video.url = url;
        video.Prepare();

        while (!video.isPrepared)
            yield return null;
        video.Play();
    }

    AsyncOperation ao;

    IEnumerator PlayVideo(string url)
    {
        var www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        ao = www.SendWebRequest();
        yield return ao;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.error);
        }
        else
        {
            video.source = VideoSource.VideoClip;
            video.clip = DownloadHandlerAssetBundle.GetContent(www).LoadAsset<VideoClip>("2.mp4");
            Debug.Log("Done");
            video.Play();
        }
    }
}

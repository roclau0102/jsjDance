using UnityEngine;
using System.Collections;

public class OpenScene : MonoBehaviour {    
    public GameObject frame;
    public UnityEngine.Video.VideoPlayer video;
    
	// Use this for initialization
	void Start () {

        Global.InitOpen(()=> {
            Debug.Log("初始化完成");
            //video.url = Global.AppContentPath() + "open.mp4";
            //video.Play();
            //GetComponent<AudioSource>().Play();
            //time = Time.time;
            StartCoroutine(PlayVideoAtUrl(Global.AppContentPath() + "open.mp4"));
        });
	}

    IEnumerator PlayVideoAtUrl(string url)
    {
        Debug.Log(url);
        video.source = UnityEngine.Video.VideoSource.Url;
        video.url = url;
        video.Prepare();

        while (!video.isPrepared)
            yield return null;

        GetComponent<AudioSource>().Play();
        time = Time.time;
    }


    float time = -1;
    bool done = false;

    void Update()
    {
        if (done) return;
        bool complete =false;
        if (complete || (time!=-1 && (Time.time - time) > 9.8f))  
        {
            Debug.Log("complete:" + complete);
            GetComponent<Animation>().Play();
            Invoke("Load", 0.5f);
            done = true;
        }        
    }

    void Load()
    {
        Sounder.instance.Play("背景音乐", true);
        Application.LoadLevel("Title");
    }
}

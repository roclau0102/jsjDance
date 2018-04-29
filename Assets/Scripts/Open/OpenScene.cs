using UnityEngine;
using System.Collections;

public class OpenScene : MonoBehaviour {    
    public GameObject frame;
    //public VideoPlaybackBehaviour video;
    public UnityEngine.Video.VideoPlayer video;
    
	// Use this for initialization
	void Start () {

        Global.InitOpen(()=> {
            Debug.Log("初始化完成");
            video.url = "file://" + Global.AppContentPath() + "open.mp4";
            video.Play();
            GetComponent<AudioSource>().Play();
            time = Time.time;
        });

        //if (Global.IsOverlayMode())
        //{
        //    video.gameObject.SetActive(false);
        //    Global.InitOpen(() =>
        //    {
        //        Debug.Log("初始化完成");

        //        if (Application.platform == RuntimePlatform.Android)
        //        {
        //            Debug.Log("初始化完成,播放视频StaticPlayVideo:" + Global.AppContentPath() + "open");
        //            Global.CallAndroidStatic("StaticPlayVideo", Global.AppContentPath() + "open");
        //        }
        //        GetComponent<AudioSource>().Play();
        //        time = Time.time;
        //    });
        //}
        //else
        //{
        //    video.gameObject.SetActive(true);
        //    Global.InitOpen(() =>
        //    {
        //        video.SetPath("open");
                
        //        video.readyCall = () =>
        //        {
        //            Debug.Log("准备好了");
        //            GetComponent<AudioSource>().Play();
        //            Invoke("PlayVideo", 0.2f);
        //        };

        //        video.completeCall = () =>
        //        {
        //            GetComponent<Animation>().Play();
        //            Invoke("Load", 0.5f);
        //            done = true;
        //        };
        //    });
        //}

        //if(Application.platform == RuntimePlatform.WindowsEditor)
        //{
        //    Invoke("Load", 1);
        //}
	}




    void PlayVideo()
    {
        //video.VideoPlayer.Play(false, 0);
        GetComponent<AudioSource>().Play();
    }

    float time = -1;
    bool done = false;

    void Update()
    {
        if (!Global.IsOverlayMode()) return;

        if (done) return;
        bool complete =false;
        if (Application.platform == RuntimePlatform.Android)
        {
             //complete = Global.GetAndroidBool("openComplete");
        }
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

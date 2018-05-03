using UnityEngine;
using System.Collections;

public class RandomVideo : MonoBehaviour {

    public UnityEngine.Video.VideoPlayer video;
    //public VideoPlaybackBehaviour video;
	// Use this for initialization
	void Start () {
        video.source = UnityEngine.Video.VideoSource.Url;
	}

    public void LoopPlay(string path)
    {
        //video.SetPath(path);
        video.url = path.StartsWith("file://") ? path : "file:///" + path;
        video.prepareCompleted += Video_prepareCompleted;
        //video.readyCall = () =>
        //{
        //    Debug.Log("准备好了");
        //    Invoke("Play", 0.1f);
        //};
        video.loopPointReached += Video_loopPointReached;
        //video.completeCall = () =>
        //{
        //    //视频播放完鸟
        //};
    }

    private void Video_loopPointReached(UnityEngine.Video.VideoPlayer source)
    {
        //throw new System.NotImplementedException();
        //视频播放完鸟
    }

    private void Video_prepareCompleted(UnityEngine.Video.VideoPlayer source)
    {
        //throw new System.NotImplementedException();
        Debug.Log("准备好了");
        Invoke("Play", 0.1f);
    }

    void Play()
    {
        //video.VideoPlayer.Play(false, 0);
        video.Play();
        ready = true;
    }

    public void PlayLast5()
    {
        goingEnd = true;
        try
        {
            //video.VideoPlayer.SeekTo(25);
            //video.see
        }
        catch
        {

        }
        
    }

    float lastTime = 0;
    float[] times = new float[]{
        5,10,15,20
    };

    bool goingEnd = false;
    bool ready = false;

	// Update is called once per frame

	void Update () {
        if (!ready) return;
        if (goingEnd) return;
        //float v= video.VideoPlayer.GetCurrentPosition();
        //if (v - lastTime >= 5f)
        //{            
        //    float newTime=lastTime;
        //    while(newTime==lastTime){
        //        float temp = times[ Random.Range(0, times.Length)];
        //        if(temp==lastTime)continue;
        //        newTime = temp;
        //    }
        //    lastTime = newTime;
        //    video.VideoPlayer.SeekTo(newTime);
        //    Debug.Log(">5s, seek to" + lastTime);
        //}
	}
}

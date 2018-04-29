using UnityEngine;
using System.Collections;

public class RandomVideo : MonoBehaviour {
    public VideoPlaybackBehaviour video;
	// Use this for initialization
	void Start () {
	    
	}

    public void LoopPlay(string path)
    {
        video.SetPath(path);
        video.readyCall = () =>
        {
            Debug.Log("准备好了");
            Invoke("Play", 0.1f);
        };

        video.completeCall = () =>
        {
            //视频播放完鸟
        };
    }

    void Play()
    {
        video.VideoPlayer.Play(false, 0);
        ready = true;
    }

    public void PlayLast5()
    {
        goingEnd = true;
        try
        {
            video.VideoPlayer.SeekTo(25);
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
        float v= video.VideoPlayer.GetCurrentPosition();
        if (v - lastTime >= 5f)
        {            
            float newTime=lastTime;
            while(newTime==lastTime){
                float temp = times[ Random.Range(0, times.Length)];
                if(temp==lastTime)continue;
                newTime = temp;
            }
            lastTime = newTime;
            video.VideoPlayer.SeekTo(newTime);
            Debug.Log(">5s, seek to" + lastTime);
        }
	}
}

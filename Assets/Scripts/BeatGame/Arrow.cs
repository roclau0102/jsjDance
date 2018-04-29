using UnityEngine;
using System.Collections;
using System;

public class Arrow : MonoBehaviour {

    public STATE state = STATE.READY;
    protected float lastRunTime = 0;
    float lifeRunTime = 0;
    float bronY = 0;

    public enum STATE
    {
        READY,
        PRESSING,
        FAILED,        
        SUCCESS
    }

    protected tk2dSprite icon;
    protected tk2dSpriteAnimator anim;

    public bool isLastOne;
    protected float speed;
    public SongTime songTime{get;private set;}


    virtual protected void Start()
    {
        anim.Play(ds); 
    }
    string ds;

    public void SetType(SongInfo.DIRECTION d)
    {
        if (icon == null)
        {
            icon = transform.Find("Icon").GetComponent<tk2dSprite>();
            anim = transform.Find("Icon").GetComponent<tk2dSpriteAnimator>();
        }
        ds = d.ToString().ToLower();
        anim.Play(ds);
        
    }


    internal void SetSongTime(SongTime songTime)
    {
        this.songTime = songTime;
    }


    
    

    internal virtual void Move(float runTime)
    {
        float timeOffset = runTime - lastRunTime;
        lastRunTime = runTime;
        AddRunTime(timeOffset);        
    }

    public bool isDebug = false;

    protected virtual void AddRunTime(float timeOffset)
    {
        lifeRunTime += timeOffset;
        Vector3 pos = transform.position;
        pos.y = bronY + speed * lifeRunTime;
        if(isDebug)Debug.Log(lifeRunTime);
        transform.position = pos;
    }


    public virtual void SetState(STATE s)
    {
        state = s;
    }


    virtual internal void Init(float time, float initSpeed)
    {
        lastRunTime = time;
        this.speed = initSpeed;
        bronY = transform.position.y;
        lifeRunTime = 0;
    }

    public bool IsPerfectTime()
    {
        if (isDebug)
        {
            Debug.Log("剩余时间:"+(songTime.showTime - BeatGame.instance.audioSource.time - SongPlayer.PRESS_DELAY_TIME));
        }
        return (songTime.showTime - BeatGame.instance.audioSource.time - SongPlayer.PRESS_DELAY_TIME) < 0;
    }


    virtual internal void Kill()
    {
        //Debug.Log(ds + "时间差:" + (songTime.showTime  - BeatGame.instance.audioSource.time));
    }
}

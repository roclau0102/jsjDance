using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class LongPressArrow : Arrow {

    public tk2dSlicedSprite bg;
    public tk2dSprite secondArrow;
    public tk2dSprite topArrow;
    Vector3 arrowBgBackupPos;
    float _height = 178;
    public Arrow.STATE subState = STATE.READY;

    protected override void Start()
    {
 	     base.Start();
         arrowBgBackupPos = secondArrow.transform.localPosition; 
    }

    LongPressEff eff;


    internal override void Init(float time, float initSpeed)
    {
        base.Init(time, initSpeed);
        topArrow.gameObject.SetActive(false);
        bg.gameObject.SetActive(true);
        icon.gameObject.SetActive(true);
        pressingTime = 0;
    }

    public bool IsPerfectTime(bool isFirst)
    {
        if (isFirst)
        {
            if (isDebug)
            {
                //Debug.Log("剩余时间:" + (songTime.showTime - BeatGame.instance.audioSource.time));
            }
            return (songTime.showTime - BeatGame.instance.audioSource.time- SongPlayer.PRESS_DELAY_TIME) < 0;
        }
        return (songTime.showTime+(songTime as LongSongTime).PressTime - BeatGame.instance.audioSource.time- SongPlayer.PRESS_DELAY_TIME) < 0;
    }


    internal override void Kill()
    {
        //Debug.Log("时间差:"+ (songTime.showTime+(songTime as LongSongTime).PressTime - BeatGame.instance.audioSource.time));
    }


    protected override void AddRunTime(float timeOffset)
    {
        if (state == STATE.PRESSING)
        {
            height -= timeOffset* speed  * 100;
            if (height <= 65)
            {
                topArrow.gameObject.SetActive(false);
                bg.gameObject.SetActive(false);
                CleanEff();
            }
        }
        else
        {
            base.AddRunTime(timeOffset);
        }        
    }



    /*
    internal override void Move(float speed)
    {        
        if (state == STATE.PRESSING)
        {            
            height -= speed*100;
            if (height <= 65)
            {
                topArrow.gameObject.SetActive(false);
                bg.gameObject.SetActive(false);
                CleanEff();
            }
        }
        else { 
            base.Move(speed);
        }
    }*/

    
    public void SetType(SongInfo.DIRECTION d, float height)
    {        
        if (height < 178) height = 178;
        this.height = height;
        this.directoin = d;
    }

    SongInfo.DIRECTION _directoin;
    public float pressingTime;

    public SongInfo.DIRECTION directoin
    {
        get
        {
            return _directoin;
        }
        set
        {
            _directoin = value;
            float z = 0;
            switch (_directoin)
            {
                case SongInfo.DIRECTION.DOWN:
                    z = 90;
                    break;
                case SongInfo.DIRECTION.RIGHT:
                    z = 180;
                    break;
                case SongInfo.DIRECTION.UP:
                    z = 270;
                    break;
            }
            secondArrow.transform.localEulerAngles = new Vector3(0, 0, z);
            topArrow.transform.localEulerAngles = new Vector3(0, 0, z);

            bg.SetSprite("longPress" + _directoin.ToString().ToLower());
            base.SetType(_directoin);
        }
    }   

    /// <summary>
    /// 设置总体高度
    /// </summary>
    public float height { 
        get{
            return _height;
        } 
        set{
            _height = value;
            bg.dimensions = new Vector2(119, _height);
            secondArrow.transform.localPosition = new Vector3(arrowBgBackupPos.x,  -(height / 100) + 0.65f, arrowBgBackupPos.z);
        }
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    /// <param name="s"></param>
    public override void SetState(Arrow.STATE s)
    {
 	    base.SetState(s);
        if (state == STATE.PRESSING)
        {
            topArrow.gameObject.SetActive(true);
            icon.gameObject.SetActive(false); 
        }
        else
        {
            CleanEff();
        }
    }

    public void AddLongEff(Vector3 pos)
    {
        eff = Eff.instance.AddLongArrowEff();
        eff.transform.position = pos;        
    }

    void CleanEff()
    {        
        if (eff != null)
        {
            eff.Restore();
            eff = null;
        }
    }

    /// <summary>
    /// 得到第二个箭头的位置
    /// </summary>
    /// <returns></returns>
    internal Vector3 GetSecondArrowPos()
    {
        return secondArrow.transform.position;
    }
}

/*
#if UNITY_EDITOR
[CanEditMultipleObjects()]
[CustomEditor(typeof(LongPressArrow), true)]
public class LongPressArrowExtension : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LongPressArrow arrow= (target as LongPressArrow);
        float h= EditorGUILayout.FloatField("Test Length", arrow.height);
        if (h != arrow.height)
        {
            arrow.height = h;
        }

        SongInfo.DIRECTION d = (SongInfo.DIRECTION)EditorGUILayout.EnumPopup("Test Direction", arrow.directoin);
        if (d != arrow.directoin)
        {
            arrow.directoin = d;
        }

    }
}
#endif*/
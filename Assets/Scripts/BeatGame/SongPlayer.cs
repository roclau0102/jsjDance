using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SongPlayer : MonoBehaviour
{
    #region 变量
    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject upArrow;
    public GameObject downArrow;
    public ZoomNumber markText;
    public CorrectProp correctProp;
    public GameObject arrowContainer;
    public TimingJudge timingJudge;
    public tk2dTiledSprite hpBar;
    public GameObject hpEff;
    public GameObject arrowPrefab;
    public GameObject longArrowPrefab;
    public const float END_POSITION_Y = 9;    
    public AudioClip leftRightClip;
    public BeatLight leftLight;
    public BeatLight rightLight;
    public BeatLight upLight;
    public BeatLight downLight;
    public PlayParticle combo10Eff;
    public PlayParticle combo1Eff;


    public Combo combo;
    public GameObject lose;

    public const float HIT_DISTANCE = 1.36f;

    List<Arrow> arrows = new List<Arrow>();
    List<Arrow> deleteArrows = new List<Arrow>();

    SongTime[] curBeats;
    /// <summary>
    /// 运行时间
    /// </summary>
    float runTime = 0;
    int beatIndex = 0;
    /// <summary>
    /// 得分的百分比
    /// </summary>
    float[] juadePercent = new float[] { 0, 0.1f, 0.3f, 0.5f, 1 };

    public PlayerScoreData scoreData = new PlayerScoreData();

    /// <summary>
    /// 每一帧移动的单位(不是像素)
    /// </summary>
    float unitSpeedPerFrame = 0;
    float unitSpeedPerSec = 0;
    float bornPosY = 0;

    int mark = 0;
    float hp = 100;
    float hpMax = 200f;
    SongInfo info;
    float dimensionsX = 0;
    public static float moveToTopTime;
    [HideInInspector]
    public int index;

    /// <summary>
    /// 玩家反应时间
    /// </summary>
    public const float PRESS_DELAY_TIME = 0;

    void Start()
    {        
        alive = true;
        moveToTopTime = 3;

        //bornPosY = 0.67f;
        switch (DataUtils.difficult)
        {
            case SongInfo.DIFF_LEVEL.MID:
                moveToTopTime = 3f;
                break;
            case SongInfo.DIFF_LEVEL.HARD:
                moveToTopTime = 3f;
                break;
        }
        unitSpeedPerSec = ((END_POSITION_Y - bornPosY) / (moveToTopTime)) ;
        unitSpeedPerFrame = unitSpeedPerSec / ((float)Application.targetFrameRate);        
        RefreshHP(0);
    }


    public bool alive
    {
        get;
        private set;
    }
    #endregion

    public void AddHP(int hpAdd)
    {        
        RefreshHP(-hpAdd);
    }

    public void SetMaxHP(int hp)
    {
        hpMax = hp;
    }

   

    /// <summary>
    /// MISS
    /// </summary>
    /// <param name="minusHP"></param>
    void RefreshHP(float minusHP = 1)
    {
        //这个平台不扣血啦
        //if (Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_SHOW_NO_REGIST ||
        //    Version.currentPlatform == Version.PLAFTFORM_ENUM.TEL
        //    )
        //{
        //    minusHP = 0;
        //}
        hp -= minusHP;
        if(hp>hpMax){
            hp = hpMax;
        }else if (hp < 0)
        {
            hp = 0;
            alive = false;            
            scoreData.SetMissCount(curBeats.Length- beatIndex);
            ClearnArrow();
            combo.gameObject.SetActive(false);
            BeatGame.instance.PlayerDead(this);
            return;
        }
        float per = hp / hpMax;
        if (per > 1)
        {
            per = 1;
        }
        else if (per < 0) per = 0;
        dimensionsX = per * 695f;
    }

    public void ClearnArrow()
    {
        for (int i = 0; i < arrows.Count; i++)
        {
            KillArrow(arrows[i]);
        }
        arrows.Clear();        
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="info"></param>
    /// <param name="diff"></param>
    public void StartPlay(SongInfo info, SongInfo.DIFF_LEVEL diff)
    {
        this.info = info;
        runTime = 0;
        switch (diff)
        {
            case SongInfo.DIFF_LEVEL.EASY:
                curBeats = info.easy;
                break;
            case SongInfo.DIFF_LEVEL.MID:
                curBeats = info.mid;
                break;
            case SongInfo.DIFF_LEVEL.HARD:
                curBeats = info.hard;
                break;
        }
    }

    /// <summary>
    /// 按键
    /// </summary>
    /// <param name="d"></param>
    /// <param name="keyState"></param>
    public void PressKey(SongInfo.DIRECTION d, JoystickManager.JOYSTICK_KEY_STATE keyState)
    {
        Arrow a;
        GameObject targetArrow = null;
        

        for (int i = 0, c = arrows.Count; i < c; i++)
        {
            a = arrows[i];
            if (a.songTime.direction != d) continue;

            switch (a.songTime.direction)
            {
                case SongInfo.DIRECTION.UP:
                    targetArrow = upArrow;
                    break;
                case SongInfo.DIRECTION.DOWN:
                    targetArrow = downArrow;
                    break;
                case SongInfo.DIRECTION.LEFT:
                    targetArrow = leftArrow;
                    break;
                case SongInfo.DIRECTION.RIGHT:
                    targetArrow = rightArrow;
                    break;
            }
            float dis = Vector3.Distance(a.transform.position, targetArrow.transform.position);
            Global.TIMING_JUAGE_TYPE type = GetJuade(dis);

            if (a is LongPressArrow)
            {
                LongPressArrow l = a as LongPressArrow;
                switch (l.state)
                {
                    case LongPressArrow.STATE.READY:
                        //准备好时，如果按下相应的链接又在区域内，改变为按下状态
                        if (type != Global.TIMING_JUAGE_TYPE.NONE && keyState == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN)
                        {
                            l.SetState(LongPressArrow.STATE.PRESSING);
                            l.pressingTime = runTime;
                            SetArrowResult(type, a);
                            float offset = targetArrow.transform.position.y - a.transform.position.y;
                            a.transform.position = targetArrow.transform.position;
                            l.height += offset * 100;
                            l.AddLongEff(targetArrow.transform.position);
                            return;
                        }
                        break;
                    case LongPressArrow.STATE.PRESSING:
                        //如果按下状态时弹起，改为失败状态
                        if (keyState == JoystickManager.JOYSTICK_KEY_STATE.KEY_UP)
                        {
                            dis = Vector3.Distance(l.GetSecondArrowPos(), targetArrow.transform.position);
                            Global.TIMING_JUAGE_TYPE secondType = GetJuade(dis);
                            if (secondType != Global.TIMING_JUAGE_TYPE.NONE)
                            {
                                l.SetState(LongPressArrow.STATE.SUCCESS);
                                l.subState = Arrow.STATE.SUCCESS;
                                KillArrow(a);
                                SetArrowResult(secondType, a,false);
                                Eff.instance.AddArrowEff(targetArrow.transform.position, a.songTime.direction);
                            }
                            else
                            {
                                l.SetState(LongPressArrow.STATE.FAILED);
                            }
                            return;
                        }
                        break;
                }
            }
            else
            {
                if (keyState == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN)
                {
                    if (type != Global.TIMING_JUAGE_TYPE.NONE)
                    {
                        KillArrow(a);
                        SetArrowResult(type, a);
                        Eff.instance.AddArrowEff(targetArrow.transform.position, a.songTime.direction);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 得到分数
    /// </summary>
    /// <param name="curCombo"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    static public int GetMark(int curCombo, Global.TIMING_JUAGE_TYPE type, bool playSound=false)
    {
        int addMark = 0;
        switch (type)
        {
            case Global.TIMING_JUAGE_TYPE.PREFECT:
                addMark += 30;
                break;
            case Global.TIMING_JUAGE_TYPE.COOL:
                addMark += 20;
                break;
            case Global.TIMING_JUAGE_TYPE.GOOD:
                addMark += 10;
                break;
        }

        float awardFactor = 0.5f;
        addMark = (int)((Mathf.Floor((float)curCombo / 10) * awardFactor + 1) * (float)addMark);

        if (curCombo % 100 == 0 && curCombo > 0)
        {
            addMark += (int)((float)addMark * ((float)curCombo / 100f) * 0.1f);            
        }        
        return addMark;
    }



    void AddCombo(Global.TIMING_JUAGE_TYPE type)
    {
        combo.Add();
        int curCombo = combo.GetCount();
        mark += GetMark(curCombo - 1, type,true);
        Eff.instance.ShowCombo100(curCombo > 100, this.index);
        
        scoreData.score = mark;
        scoreData.comboCount++;
        

        
        if (curCombo % 100 == 0 && curCombo > 1)
        {            
            Sounder.instance.Play("连击加成音效100");
        }
        else if (curCombo % 10 == 0 && curCombo > 1)
        {
            combo10Eff.gameObject.SetActive(true);
            combo10Eff.PlayParticles();
            Sounder.instance.Play("连击加成音效");
        }
        else
        {
            combo1Eff.gameObject.SetActive(true);
            combo1Eff.PlayParticles();            
        }

        if (curCombo % 20 == 0)
        {
            AddHP(1);
        }
    }


    /// <summary>
    /// 设置箭头，计算分数
    /// </summary>
    /// <param name="type"></param>
    void SetArrowResult(Global.TIMING_JUAGE_TYPE type, Arrow arrow, bool isMainBeat=true)
    {        
        switch (type)
        {
            case Global.TIMING_JUAGE_TYPE.BAD:
                Sounder.instance.Play("BAD音效");
                break;
        }

        if (arrow.isLastOne)
        {
            bool end = true;            
            if (arrow is LongPressArrow)
            {
                LongPressArrow longPress = arrow as LongPressArrow;                
                switch (longPress.subState)
                {
                    case Arrow.STATE.PRESSING:
                    case Arrow.STATE.READY:
                        end = false;
                        break;
                }
            }
            if (hp > 0 && end)
            {
                ClearnArrow();
                Eff.instance.lastHit.SetActive(true);
                BeatGame.instance.ArrowComplete();
            }            
        }

        if(type!= Global.TIMING_JUAGE_TYPE.MISS){
            int comboTime = combo.GetCount(); 
            GetComponent<AudioSource>().volume = comboTime < 10 ? 0.7f : 1;
            if(isMainBeat)GetComponent<AudioSource>().PlayOneShot(leftRightClip);         
        }

        timingJudge.Show(type);
        if (type < Global.TIMING_JUAGE_TYPE.BAD)
        {
            AddCombo(type);
            BeatLight bl=null;
            switch (arrow.songTime.direction)
            {
                case SongInfo.DIRECTION.LEFT:
                    bl = leftLight;
                    break;
                case SongInfo.DIRECTION.RIGHT:
                    bl = rightLight;
                    break;
                case SongInfo.DIRECTION.UP:
                    bl = upLight;
                    break;
                case SongInfo.DIRECTION.DOWN:
                    bl = downLight;
                    break;
            }
            bl.Show();
            Eff.instance.ScreenHit(arrow.songTime.direction);
        }
        else
        {
            //道具
            if (correctProp != null && DataUtils.mode == Global.MODE.MODE_1P)
            {
                if (correctProp.Use())
                {
                    Sounder.instance.Play("道具消耗音效");
                    AddCombo(Global.TIMING_JUAGE_TYPE.PREFECT);
                    Eff.instance.ScreenHit(arrow.songTime.direction);
                    type = Global.TIMING_JUAGE_TYPE.PREFECT;
                }
                else
                {
                    combo.Reset();
                }
            }
            else
            {
                combo.Reset();
            }

            switch (type)
            {
                case Global.TIMING_JUAGE_TYPE.BAD:
                    mark += 5; //bad加5
                    break;
                case Global.TIMING_JUAGE_TYPE.MISS:
                    RefreshHP();
                    break;
            }
        }

        scoreData.AddJuade(info.data, type);
        markText.SetValue(mark);
    }

    #region 公用函数
    /// <summary>
    /// 设置血量类型
    /// </summary>
    /// <param name="type"></param>
    internal void SetLifeType(Global.LIFE_TYPE type)
    {
        hpBar.SetSprite(type.ToString().ToLower());
        switch (type)
        {
            case Global.LIFE_TYPE.LV1:
                hp = hpMax = 10;
                break;
            case Global.LIFE_TYPE.LV2:
                hp = hpMax = 13;
                break;
            case Global.LIFE_TYPE.LV3:
                hp = hpMax = 16;
                break;
            case Global.LIFE_TYPE.LV4:
                hp = hpMax = 20;
                break;


        }
    }
    /// <summary>
    /// 干箭头
    /// </summary>
    /// <param name="a"></param>
    void KillArrow(Arrow a)
    {
        a.Kill();
        if (deleteArrows.IndexOf(a) != -1) return;
        if (a is LongPressArrow)
        {
            PrefabPool.restore(a.gameObject, longArrowPrefab);
        }
        else
        {
            PrefabPool.restore(a.gameObject, arrowPrefab);
        }
        deleteArrows.Add(a);
    }


    /// <summary>
    /// 好坏判定
    /// </summary>
    /// <param name="dis"></param>
    /// <returns></returns>
    Global.TIMING_JUAGE_TYPE GetJuade(float dis)
    {
        if (dis > HIT_DISTANCE) return Global.TIMING_JUAGE_TYPE.NONE;
        float startPer = 0;
        float nowPer = dis / HIT_DISTANCE;
        for (int i = 1; i < juadePercent.Length; i++)
        {
            if (nowPer > startPer && nowPer <= juadePercent[i])
            {
                int type = i - 1;
                return (Global.TIMING_JUAGE_TYPE)type;
            }
            startPer = juadePercent[i];
        }
        return Global.TIMING_JUAGE_TYPE.NONE;
    }
    #endregion



    // Update is called once per frame
    void FixedUpdate()
    {
        var d = hpBar.dimensions;
        d.Set(d.x + (dimensionsX - d.x) * .3f, hpBar.dimensions.y);
        hpBar.dimensions = d;
        Vector3 p = hpEff.transform.position;
        p.x = hpBar.transform.position.x + d.x / 100;
        hpEff.transform.position = p;
        if (BeatGame.instance.playState != BeatGame.PLAY_STATE.PLAYING || !alive) return;

        //runTime+= Time.fixedDeltaTime;
        runTime = BeatGame.instance.audioSource.time;
        #region 添加箭头
        for (int i = beatIndex, c = curBeats.Length; i < c; i++)
        {
            SongTime songTime = curBeats[i];
            if (runTime >= (songTime.showTime - moveToTopTime))
            {
                if (i >= beatIndex)
                {
                    beatIndex = i + 1;
                }
                Arrow arrow;
                if (songTime is LongSongTime)
                {
                    arrow = PrefabPool.instance(longArrowPrefab).GetComponent<LongPressArrow>();
                    (arrow as LongPressArrow).subState = Arrow.STATE.READY;
                }
                else
                {
                    arrow = PrefabPool.instance(arrowPrefab).GetComponent<Arrow>();
                }                
                arrow.SetState(Arrow.STATE.READY);                      
                arrow.gameObject.SetActive(true);
                arrow.gameObject.transform.parent = arrowContainer.transform;
                

                if (beatIndex == curBeats.Length)
                {
                    arrow.isLastOne = true;
                }

                GameObject targetArrow = null;
                switch (songTime.direction)
                {
                    case SongInfo.DIRECTION.DOWN:
                        targetArrow = downArrow;
                        break;
                    case SongInfo.DIRECTION.LEFT:
                        targetArrow = leftArrow;
                        break;
                    case SongInfo.DIRECTION.RIGHT:
                        targetArrow = rightArrow;
                        break;
                    case SongInfo.DIRECTION.UP:
                        targetArrow = upArrow;
                        break;
                }


                if (songTime is LongSongTime)
                {
                    arrow.GetComponent<LongPressArrow>().SetType(songTime.direction, (songTime as LongSongTime).PressTime * unitSpeedPerFrame * Application.targetFrameRate * 100);
                }
                else
                {
                    arrow.GetComponent<Arrow>().SetType(songTime.direction);
                }
                arrow.transform.localPosition = new Vector3(targetArrow.transform.localPosition.x, bornPosY, 0);
                arrow.SetSongTime(songTime);
                arrows.Add(arrow);
                arrow.Init(runTime - PRESS_DELAY_TIME, unitSpeedPerSec);                
            }
            else
            {
                break;
            }
        }

        

        #endregion

        #region 移动、删除箭头
        for (int i = 0, c = arrows.Count; i < c; i++)
        {
            if (!alive) return;
            Arrow a = arrows[i];
            a.Move(runTime);

            bool isLongPress = a is LongPressArrow;

            //超出底线，干掉
            if ((!isLongPress && a.transform.localPosition.y >= (Global.SCREEN_HEIGHT + 1f)) ||
                (isLongPress && a.transform.localPosition.y >= (Global.SCREEN_HEIGHT + 1f + (a as LongPressArrow).height / 100))
                )
            {
                KillArrow(arrows[i]);
            }
            else if ((a.transform.position.y > (END_POSITION_Y + HIT_DISTANCE))) //超过顶部图标，判定MISS
            {
                if (a.state != Arrow.STATE.FAILED)
                {
                    a.SetState(Arrow.STATE.FAILED);
                    SetArrowResult(Global.TIMING_JUAGE_TYPE.MISS, a);
                }
                else if (isLongPress && (a as LongPressArrow).subState != Arrow.STATE.FAILED)
                {
                    Vector3 pos = (a as LongPressArrow).GetSecondArrowPos();
                    if (pos.y > (END_POSITION_Y + HIT_DISTANCE))
                    {
                        (a as LongPressArrow).subState = Arrow.STATE.FAILED;
                        SetArrowResult(Global.TIMING_JUAGE_TYPE.MISS, a);
                    }
                }
            }
            else if (DataUtils.isAutoMode && DataUtils.mode == Global.MODE.MODE_1P) //自动模式
            {
                if(a is LongPressArrow){
                    LongPressArrow l = a as LongPressArrow;
                    switch(l.state){
                        case Arrow.STATE.READY:
                             if ((a as LongPressArrow).IsPerfectTime(true )){
                                 PressKey(a.songTime.direction, JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN);
                                 //Debug.Log("长按:" + a.songTime.direction + "时间:" + runTime);
                             }
                            break;
                        case Arrow.STATE.PRESSING:
                            if (GetJuade(Mathf.Abs(l.GetSecondArrowPos().y - END_POSITION_Y)) == Global.TIMING_JUAGE_TYPE.PREFECT)
                            //if ((a as LongPressArrow).IsPerfectTime(false))
                            {
                                PressKey(a.songTime.direction, JoystickManager.JOYSTICK_KEY_STATE.KEY_UP);
                                //Debug.Log("长按结束:" + a.songTime.direction + "时间:" + runTime);
                            }
                            break;
                    }
                }else{
                    if (a.IsPerfectTime())
                    {
                        PressKey(a.songTime.direction, JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN);
                    }
                }


                if (a is LongPressArrow)
                {
                    LongPressArrow l = a as LongPressArrow;
                    if (a.state == Arrow.STATE.PRESSING)
                    {
                        if (l.pressingTime!=0 && (runTime - l.pressingTime) > 0.25f)
                        {
                            SetArrowResult(Global.TIMING_JUAGE_TYPE.PREFECT, a, false);
                            l.pressingTime = runTime;
                        }
                    }                    
                }
            }
        }

        if (deleteArrows.Count > 0)
        {
            for (int i = 0, c = deleteArrows.Count; i < c; i++)
            {
                arrows.Remove(deleteArrows[i]);
            }
            deleteArrows.Clear();
        }

        #endregion

    }



}
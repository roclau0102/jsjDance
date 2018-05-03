using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Guide : BaseScene {
    public GameObject[] steps;
    int index = -1;
    public GameObject bg;
    static public Guide instance = null;
    public bool focusOpen = false;

	// Use this for initialization
	void Awake () {
        instance = this;
        
        bool isNew = PlayerPrefs.GetInt("NEW_GUIDE", 0) == 0;
        if (focusOpen)
        {
            isNew = true;
        }
        
        if (isNew)
        {
            PlayerPrefs.SetInt("NEW_GUIDE", 1);
            PlayerPrefs.Save();            
            DontDestroyOnLoad(this.gameObject);
            DataUtils.songDataID = 1;
        }
        else
        {
            Free();
        }
	}

    public void Free()
    {
        instance = null;
        DestroyImmediate(this.gameObject);
        RemoveKeyEvent();
    }

    public void Show(float time=1)
    {
        tk2dSprite b = bg.GetComponent<tk2dSprite>();
        Color c = b.color;
        c.a = 0;
        b.color = c;
        bg.GetComponent<Tk2dUpdateColor>().color = c;
        bg.gameObject.SetActive(true);
        Invoke("ShowBG", time+0.01f);
        bg.GetComponent<Animation>().Stop();
        bg.GetComponent<Animation>().Play();
        ShowNextStep(time);
    }


    public void Hide()
    {
        //bg.gameObject.SetActive(false);
        steps[index].gameObject.SetActive(false);
        canOperate = false;
    }


    internal void Kill()
    {
        DestroyImmediate(this.gameObject);
    }


    bool canOperate = false;
    

    private void ShowNextStep(float delayTime=0)
    {        
        if (index >= (steps.Length - 1))
        {
            RemoveKeyEvent();            
            return;
        }
        if (index >= 0)
        {
            SwitchOutStep(index);
            DestroyImmediate(steps[index].gameObject);
        }
        index++;
        if (delayTime != 0)
        {
            Invoke("ShowUI", delayTime+0.3f);
        }
        else
        {
            canOperate = true;
            steps[index].gameObject.SetActive(true);
        }
        SwitchInStep(index);
    }

    void ShowUI()
    {
        steps[index].gameObject.SetActive(true);
        canOperate = true;
    }

    private void SwitchOutStep(int index)
    {
        switch (index)
        {
            case 3:
                
                break;
            case 5:
                Transform t = songList.transform.Find("Difficult/Normal");
                Vector3 p = t.localPosition;
                p.z = 0;
                t.localPosition = p;
                break;
        }
    }




    



    Title title;
    SongListMain songList;
    Transform startBtn;


    void CanPressEnterInTitle()
    {
        title.canPressEnter = true;
    }


    private void SwitchInStep(int index)
    {
        Debug.Log("新手引导:"+index);
        Vector3 p;
        switch (index)
        {
            case 0:
                title = GameObject.Find("Title").transform.GetComponent<Title>();
                title.canPressCancel = title.canPressDown = title.canPressEnter = title.canPressLeft = title.canPressRight = title.canPressUp = false;
                break;
            case 3:
                startBtn = title.transform.Find("StartText");
                Vector3 pos = startBtn.position;
                pos.z = -10;
                startBtn.transform.position = pos;
                Invoke("CanPressEnterInTitle", 0.3f);                
                break;
            case 4:                
                break;
            case 5:
                songList = GameObject.Find("Main").GetComponent<SongListMain>();
                songList.canPressCancel = songList.canPressDown = songList.canPressEnter = songList.canPressLeft = songList.canPressRight = songList.canPressUp = false;
                Transform t = songList.transform.Find("Difficult/Normal");
                p = t.position;
                p.z = -10;
                t.position = p;
                //songList.list.GetXiaoPingGouItem();
                break;
            case 6:
                Transform songCard = songList.list.cards[0].transform;
                songList.list.isScroll = false;
                p = songCard.transform.position;
                p.z = -10;
                songCard.position = p;
                break;
            case 7:
                p = songList.list.cards[0].transform.localPosition;
                p.z = 0;
                songList.list.cards[0].transform.localPosition = p;

                p = songList.okBtn.transform.position;
                p.z = -10;
                songList.okBtn.transform.position = p;
                Invoke("CanPressEnterInSongList", 0.2f);
                break;
            case 8:
                ps = GameObject.Find("Main").transform.GetComponent<PropScene>();
                ps.canPressCancel = ps.canPressDown = ps.canPressEnter = ps.canPressLeft = ps.canPressRight = ps.canPressUp = false;
                p = ps.okBtn.transform.position;
                p.z = -10;
                ps.okBtn.transform.position = p;
                break;
            case 9:
                Invoke("CanPressEnterInProp", 0.2f);
                p = ps.okBtn.transform.localPosition;
                p.z = 0;
                ps.okBtn.transform.position = p;
                break;
        }
    }

    PropScene ps;


    void CanPressEnterInProp()
    {
        ps.canPressEnter = true;
    }


    void CanPressEnterInSongList()
    {
        songList.canPressEnter = true;
    }


    public override void Move(int x, int y, BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (!canOperate) return;
        switch (index)
        {
            case 0:
            case 1:
            case 2:
                ShowNextStep();
                break;




        }
    }





    public override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (!canOperate) return;
        if (keyState != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;
        Sounder.instance.Play("项目分数弹出音效");
        switch (index)
        {
            case 0:
            case 1:
            case 2:            
            case 5:
            case 6:
            case 8:
            case 9:
                ShowNextStep();
                break;
            case 4:
            case 7:
                Hide();
                break; 
            case 3:
                Vector3 p = startBtn.position;
                p.z = 0;
                startBtn.transform.position = p;

                Hide();
                bg.SetActive(false);
                Show(0.5f);
                break;
        }
    }

    public override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (!canOperate) return;
        if (keyState != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;
        switch (index)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                ShowNextStep();
                break;




        }
    }

}

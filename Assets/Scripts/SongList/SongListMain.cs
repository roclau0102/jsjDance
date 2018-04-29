using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SongListMain : BaseScene
{
    public SongList list;
    public Difficult difficult;
    static public SongListMain instance;
    public BuyConfirm buyConfirm;
    public Alert alert;

    void Start()
    {
        instance = this;

        list.getDiffCallback = difficult.GetDiff;
        difficult.changeCallback = list.OnDiffChange;

        Global.init(() =>
        {
            Debug.Log("配置文件加载完毕。");
            list.createListComplete = () =>
            {
                if (Guide.instance != null)
                {
                    Guide.instance.Show();
                }
            };

            list.CreateList();
            DataUtils.RandomCharacter();

            //测试
            if (DataUtils.runingAutoMode)
            {
                Invoke("AutoGoNext", 3);
            }
        });
        pressTime = Time.time;
    }

    private void Update()
    {
        if (Version.currentPlatform == Version.PLAFTFORM_ENUM.SkyWorth_Dis_NoReg)
        {
            if (Time.time - pressTime > 15)
            {
                if (!DataUtils.isAutoMode)
                {
                    DataUtils.isAutoMode = true;
                    DataUtils.runingAutoMode = true;
                    AutoGoNext();
                }
            }
        }
    }

    void AutoGoNext()
    {
        if (!DataUtils.runingAutoMode) return;
        DataUtils.songDataID = list.GetRandomDownload().id;
        LoadLevel("video");
    }


    float pressTime = 0;

    public bool GetCanPress()
    {
        if (Time.time - pressTime > 0.5f)
        {
            pressTime = Time.time;
            return true;
        }
        return false;
    }



    internal void Alert(string p)
    {
        alert.Show(p);
        alert.gameObject.SetActive(true);
    }



    protected override void Move(int x, int y, BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (!GetCanPress()) return;

        if (SongCard.IsDownloading())
        {
            if (buyConfirm.gameObject.activeSelf)
            {
                buyConfirm.Move(x);
            }
            return;
        }


        if (buyConfirm.gameObject.activeSelf)
        {
            buyConfirm.Move(x);
        }
        else if (alert.gameObject.activeSelf)
        {
            return;
        }
        else
        {
            Sounder.instance.Play("歌曲切换音效");
            if (list.downloading)
            {
                return;
            }
            if (x != 0)
            {
                difficult.Move(x);
            }
            else if (y != 0)
            {
                list.Move(y);
            }
        }
    }

    protected override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (SongCard.IsDownloading())
        {
            if (buyConfirm.gameObject.activeSelf)
            {
                buyConfirm.Press();
            }
            return;
        }

        if (buyConfirm.gameObject.activeSelf)
        {
            buyConfirm.Press();
        }
        else if (alert.gameObject.activeSelf)
        {
            alert.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (list.Press())
            {
                //去下个界面
                DataUtils.songDataID = list.GetSelectData();
                Sounder.instance.Play("选中歌曲下一页");
                switch (DataUtils.mode)
                {
                    case Global.MODE.MODE_1P:
                        LoadLevel("PropAndPlayer");
                        break;
                    case Global.MODE.MODE_2P:
                        LoadLevel("Video");
                        break;
                }
            }
        }
    }

    protected override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (SongCard.IsDownloading())
        {
            if (!buyConfirm.gameObject.activeSelf)
            {
                buyConfirm.Show(() =>
                {
                    list.CancelDownload();
                }, "正在下载，是否取消？");
            }
            else
            {
                buyConfirm.gameObject.SetActive(false);
            }
            return;
        }

        Sounder.instance.Play("返回按键");
        if (buyConfirm.gameObject.activeSelf)
        {
            buyConfirm.gameObject.SetActive(false);
        }
        else if (alert.gameObject.activeSelf)
        {
            alert.gameObject.SetActive(false);
            return;
        }
        else
        {
            LoadLevel("Title", false);
        }
    }

    public void BackToMain()
    {
        LoadLevel("Title");
    }
}

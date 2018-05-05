using UnityEngine;
using System.Collections;

public class ScoreScene : BaseScene
{

    public tk2dTextMesh getCoinTet;
    public tk2dTextMesh totalScoreText;
    public GameObject singleExtraInfo;
    public PlayerScore p1;
    public PlayerScore p2;
    public tk2dTextMesh p1CoinTxt;
    public tk2dTextMesh p1TotalScoreTxt;
    public GameObject coinEff;
    public GameObject totalMarkEff;

    // Use this for initialization
    void Start()
    {
        //测试=============

        if (DataUtils.p1ScoreData == null)
        {
            DataUtils.mode = Global.MODE.MODE_1P;
            DataUtils.p1ScoreData = new PlayerScoreData
            {
                comboCount = 100,
                juadeType2Count = new int[] { 100, 40, 30, 20, 10 },
                score = 10000
            };

            DataUtils.p2ScoreData = new PlayerScoreData
            {
                comboCount = 110,
                juadeType2Count = new int[] { 120, 30, 10, 30, 50 },
                score = 12000
            };
        }
        //结束测试===========


        Global.init(() =>
        {
            if (DataUtils.mode == Global.MODE.MODE_1P)
            {
                p2.gameObject.SetActive(false);
                singleExtraInfo.gameObject.SetActive(true);
                p1.UpdateData(DataUtils.p1ScoreData);


                int getCoin = DataUtils.p1ScoreData.GetWinMoney();
                DataUtils.AddScore(DataUtils.p1ScoreData.score);
                DataUtils.AddMoney(getCoin);

                ITweenTextValue.instance.SetTextIntTween(p1CoinTxt, getCoin, 0.8f, 0.3f);
                ITweenTextValue.instance.SetTextIntTween(p1TotalScoreTxt, DataUtils.GetScore(), 0.9f, 0.8f);
                p1.SetWin(false);
            }
            else
            {
                p2.gameObject.SetActive(true);
                singleExtraInfo.gameObject.SetActive(false);
                p1.UpdateData(DataUtils.p1ScoreData);
                p2.UpdateData(DataUtils.p2ScoreData, true);
                Destroy(coinEff);
                Destroy(totalMarkEff);
                coinEff = totalMarkEff = null;

                p1.SetWin(DataUtils.p1ScoreData.score > DataUtils.p2ScoreData.score);
                p2.SetWin(DataUtils.p1ScoreData.score < DataUtils.p2ScoreData.score);
            }

            if (DataUtils.isAutoMode)
            {
                Invoke("AutoGoNext", 3);
            }
        });

        pressTime = Time.time;
    }

    private void Update()
    {
        //if (Version.currentPlatform == Version.PLAFTFORM_ENUM.SkyWorth_Dis_NoReg)
        //{
        //    if (Time.time - pressTime > 15)
        //    {
        //        DataUtils.isAutoMode = true;
        //        DataUtils.runingAutoMode = true;

        //        AutoGoNext();
        //    }
        //}
    }

    void AutoGoNext()
    {
        LoadLevel("SongList");
    }

    void OnDestroy()
    {
        iTween.Stop();
    }

    public override void Move(int x, int y, BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {

    }

    public override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        BackToSceneSongList();
    }

    public void BackToSceneSongList()
    {
        Sounder.instance.Play("返回按键");
        LoadLevel("SongList", false);
    }

    public override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        BackToSceneSongList();
    }
}

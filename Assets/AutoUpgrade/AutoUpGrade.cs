using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AutoUpGrade : MonoBehaviour {
    // Use this for initialization
	void Start () {

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            UpgradeDone("");
            return;

        }

        int id = 0;
        switch (Version.currentPlatform)
        {
            case Version.PLAFTFORM_ENUM.OS_HIDE:
            case Version.PLAFTFORM_ENUM.OS_HIDE_NO_REG:
            case Version.PLAFTFORM_ENUM.OS_SHOW:
            case Version.PLAFTFORM_ENUM.OS_SHOW_LOGIN:
                id = 11001;
                break;
            case Version.PLAFTFORM_ENUM.WX_HIDE:
            case Version.PLAFTFORM_ENUM.WX_SHOW:
            case Version.PLAFTFORM_ENUM.WX_SHOW_NO_REGIST:
                id = 12001;
                break;
            case Version.PLAFTFORM_ENUM.TEL:
                id = 10001;
                break;
            case Version.PLAFTFORM_ENUM.WX_XRDS:
            case Version.PLAFTFORM_ENUM.WX_XRDSS_DISPLAY:
                id = 13001;
                break;
            default:
                Debug.Log("没有设置游戏版本ID,不自动更新");
                UpgradeDone("1");
                return;
        }

        new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity")
            .Call("ShowUpgradeActivity", id);//这个10001是自动更新ID
	}


    void UpgradeDone(string p)
    {
        //TODO:检查无更新或者用户取消更新.这里进入下个场景
        Debug.Log("检查更新完毕");
        //Application.LoadLevel(1);
        SceneManager.LoadScene(1);
    }
}

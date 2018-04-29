using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// 湖南电信版本的检测~不注册无购买，只需要检查一下这个,如果检查不通过直接就闪退了
/// </summary>
public class HNDXRegist:MonoBehaviour {

    public Version.PLAFTFORM_ENUM 目标版本;


    public void Start()
    {
        if (Version.currentPlatform != 目标版本)
        {
            Destroy(gameObject);
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            bool avaible = new AndroidJavaObject("com.waixing.thirdparty.hunandianxin.MainActivity").CallStatic<bool>("IsAvaible");
            if (!avaible)
            {
                new AndroidJavaObject("java.lang.System").CallStatic("exit", 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

}

using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Invoke("Test1", 2);
	}

    void Test1()
    {
        var androidJC = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var jo = androidJC.GetStatic<AndroidJavaObject>("currentActivity");
        var jc = new AndroidJavaClass("com.jms.cwTips");
        jc.CallStatic("Call", jo);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

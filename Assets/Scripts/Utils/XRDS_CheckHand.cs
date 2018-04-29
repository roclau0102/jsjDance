using UnityEngine;
using System.Collections;
using System;

public class XRDS_CheckHand : MonoBehaviour {
    public Action doneCall;
    public Action failedCall;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (BaseScene.blanketPress)
        {
            if (doneCall != null)
            {
                doneCall();
            }
            gameObject.SetActive(false);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                failedCall();
                gameObject.SetActive(false);
            }
        }
	}




}

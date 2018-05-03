/*
 *Author by roc. All rights reserved. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitlteTouch : MonoBehaviour {

    Title title;

    Button startBtn;
    Button singleBtn;
    Button dualBtn;

	// Use this for initialization
	void Start () {
        title = GameObject.Find("Title").transform.GetComponent<Title>();

        startBtn = transform.Find("Button_Start").GetComponent<Button>();
        singleBtn = transform.Find("Button_Single").GetComponent<Button>();
        dualBtn = transform.Find("Button_Dual").GetComponent<Button>();

        singleBtn.gameObject.SetActive(false);
        dualBtn.gameObject.SetActive(false);

        startBtn.onClick.AddListener(() => {
            title.EnterSelectMode();

            singleBtn.gameObject.SetActive(true);
            dualBtn.gameObject.SetActive(true);
        });

        singleBtn.onClick.AddListener(() => {
            title.EnterSingleMode();
        });

        dualBtn.onClick.AddListener(()=> {
            title.EnterDualMode();
        });
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

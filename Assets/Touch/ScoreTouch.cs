using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTouch : MonoBehaviour {

    UnityEngine.UI.Button backBtn;

	// Use this for initialization
	void Start () {
        backBtn = transform.Find("Button_Back").GetComponent<UnityEngine.UI.Button>();

        backBtn.onClick.AddListener(()=> {
            GameObject.Find("Main").GetComponent<ScoreScene>().BackToSceneSongList();
        });
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

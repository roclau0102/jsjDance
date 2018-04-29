using UnityEngine;
using System.Collections;

public class BeatLight : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    public void Show()
    {
        gameObject.SetActive(true);
        time = Time.time;
    }

    float time;
	
	// Update is called once per frame
	void Update () {
        if (Time.time - time > 0.3f)
        {
            gameObject.SetActive(false);
        }
	}
}

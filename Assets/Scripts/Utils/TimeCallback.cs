using UnityEngine;
using System.Collections;

public class TimeCallback : MonoBehaviour {
    public float liveTime = 2;
    float startTime = 0;
    public System.Action<GameObject> doneCallback;
    public object data;

	// Use this for initialization
	void Start () {
	
	}

    void OnEnable()
    {
        startTime = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.time - startTime > liveTime)
        {            
            gameObject.SetActive(false);
            if (doneCallback != null) doneCallback(gameObject);
        }
	}
}

using UnityEngine;
using System.Collections;

public class Alert : MonoBehaviour {
    public TextMesh text;
	// Use this for initialization
	public void Show(string t) {
        text.text = t;
        gameObject.SetActive(true);
	}


}

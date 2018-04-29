using UnityEngine;
using System.Collections;

public class ScoreBg : MonoBehaviour {
    public Texture2D[] bg;
	// Use this for initialization
	void Start () {
        tk2dSpriteFromTexture t = GetComponent<tk2dSpriteFromTexture>();
        t.texture = bg[DataUtils.characterID - 1];
        t.ForceBuild();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

using UnityEngine;
using System.Collections;

public class SwitchTk2dTextureLang : MonoBehaviour
{
    tk2dSpriteFromTexture sprite;
    public Texture2D enTexture2d;

	// Use this for initialization
	void Start () {
        sprite = GetComponent<tk2dSpriteFromTexture>();
        //if (Version.IsEN() && enTexture2d!=null)
        //{
        //    if(sprite.texture!= enTexture2d){
        //        sprite.texture = enTexture2d;
        //        sprite.ForceBuild();
        //    }
        //}
	}
}

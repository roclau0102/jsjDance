using UnityEngine;
using System.Collections;

public class CD : MonoBehaviour {    
    public tk2dSpriteFromTexture cdTexture;    
    public TextMesh songLength;
    public TextMesh rightPercent;    
    Texture2D texture2d;

    public void UpdateData(SongData sd, byte[] bytes)
    {
        StopAllCoroutines();
        StartCoroutine(DoUpdateData(sd, bytes));

       
    }

    internal void UpdateData(SongData sd, Texture2D texture2D)
    {
        cdTexture.texture = texture2d;
        cdTexture.ForceBuild();
        cdTexture.gameObject.SetActive(true);
        songLength.text = sd.time;
        rightPercent.text = DataUtils.GetMusicRightPercent(sd, DataUtils.difficult) + "%";
        cdTexture.transform.localEulerAngles = Vector3.zero;
        GetComponent<Animation>().Stop();
        GetComponent<Animation>().Play();
        cdTexture.GetComponent<Animation>().Stop();
        cdTexture.GetComponent<Animation>().enabled = false;
        cdTexture.GetComponent<Animation>().enabled = true;
        cdTexture.GetComponent<Animation>().Play("CDFadeIn");
        GetComponent<AudioSource>().Play();
    }

    IEnumerator DoUpdateData(SongData sd, byte[] bytes)
    {
        yield return new WaitForSeconds(0.1f);
        if (texture2d == null) texture2d = new Texture2D(1, 1);


        if (bytes == null || bytes.Length == 0)
        {
            cdTexture.gameObject.SetActive(false);
        }
        else
        {
            texture2d.LoadImage(bytes);            
        }
        UpdateData(sd, texture2d);        
        yield break;
    }

    


    internal void clearTexture()
    {
        cdTexture.gameObject.SetActive(false);        
    }

	// Use this for initialization
	void Start () {
        cdTexture.gameObject.SetActive(false);
	}




    
}

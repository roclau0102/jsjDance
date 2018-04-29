using UnityEngine;
using System.Collections;

public class PlayerScore : MonoBehaviour
{
    public tk2dTextMesh scoreTxt;
    public tk2dTextMesh[] juadeText;
    public tk2dTextMesh comboText;
    public tk2dTextMesh accurateText;
    public tk2dSprite winLevel;
    public GameObject winFrame;
    public GameObject[] grade;

    

	// Use this for initialization
	void Start () {
	
	}

    void PlayScoreSound()
    {
        Sounder.instance.Play("项目分数弹出音效");
    }

    void PlayJuadeSound()
    {
        Sounder.instance.Play("评价" + data.GetGrade().ToString().ToUpper());
    }

    public void SetWin(bool b)
    {
        if (!b)
        {
            DestroyImmediate(winFrame);
        }        
    }

    PlayerScoreData data;
 
	public void UpdateData (PlayerScoreData data, bool delay=false) {
        this.data = data;
        
        if (!delay)
        {
            PlayAnim();
        }
        else
        {
            Invoke("PlayAnim", 1);
        }  
	}

    void PlayAnim()
    {
        ITweenTextValue.instance.SetTextIntTween(scoreTxt, data.score, 0, 0.3f);
        ITweenTextValue.instance.SetTextIntTween(juadeText[0], data.juadeType2Count[0], 0.1f, 0.3f);
        ITweenTextValue.instance.SetTextIntTween(juadeText[1], data.juadeType2Count[1], 0.2f, 0.3f);
        ITweenTextValue.instance.SetTextIntTween(juadeText[2], data.juadeType2Count[2], 0.3f, 0.3f);
        ITweenTextValue.instance.SetTextIntTween(juadeText[3], data.juadeType2Count[3], 0.4f, 0.3f);
        ITweenTextValue.instance.SetTextIntTween(juadeText[4], data.juadeType2Count[4], 0.5f, 0.3f);
        ITweenTextValue.instance.SetTextIntTween(comboText, data.comboCount, 0.6f, 0.3f);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", data.accurate * 100, "time", 0.3f, "delay", 0.7f, "onupdate", "onUpdate"));
        grade[(int)data.GetGrade()].SetActive(true);
        GetComponent<Animation>().Play();
    }


    void onUpdate(float v)
    {
        accurateText.text = v.ToString("f1")+"%";
    }
}

using UnityEngine;
using System.Collections;

public class SongData{    
    public string cnName;
    public string enName;
    public string cover
    {
        get
        {
            return enName;
        }
        set
        {

        }
    }
    public string time;
    int _sec=-1;
    public int price;
    public int id;
    public bool local;
    public byte[] coverBytes;
    public int index;
    public Texture2D texture2d;

    public bool IsDownloaded()
    {        
        return Global.instance.CheckMp3Download(this);
    }

    public int sec
    {
        get
        {
            if (_sec == -1)
            {
                if(time==null){
                    _sec=0;
                }else{
                    string[] t = time.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
                    _sec = 0;

                    for (int i = 0; i < t.Length; i++)
                    {
                        switch(t.Length-i){
                            case 2:
                                _sec += int.Parse(t[i]) * 60;
                                break;
                            case 1:
                                _sec += int.Parse(t[i]);
                                break;
                        }
                    }
                }                
            }
            return _sec;
        }
    }
}

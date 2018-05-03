/*
 *Author by roc. All rights reserved. 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class Logcat : MonoBehaviour {

    static Logcat _instance;
    public static Logcat Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("Logcat");
                _instance = go.AddComponent<Logcat>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    StringBuilder _sb;

    public void Init()
    {
        _sb = new StringBuilder();
    }

	// Use this for initialization
	void Start () {
        Application.logMessageReceived += Application_logMessageReceived;
	}

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        //throw new System.NotImplementedException();
        if (type == LogType.Error || type == LogType.Exception)
            _sb.AppendFormat("[ATTENTION] -- {0}\n", condition);
        else
            _sb.AppendLine(condition);
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnDestroy()
    {
        Save();
    }

    private void Save()
    {
#if UNITY_EDITOR
        string pathDir = Application.streamingAssetsPath + "/log";
#else
        string pathDir = Application.persistentDataPath + "/log";
#endif
        if (!Directory.Exists(pathDir))
            Directory.CreateDirectory(pathDir);

        if (File.Exists(pathDir + "/log.txt"))
            File.Delete(pathDir + "/log.txt");

        FileInfo logcat = new FileInfo(pathDir + "/log.txt");
        using (FileStream fs = logcat.OpenWrite())
        {
            byte[] data = Encoding.UTF8.GetBytes(_sb.ToString());
            fs.Write(data, 0, data.Length);
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}

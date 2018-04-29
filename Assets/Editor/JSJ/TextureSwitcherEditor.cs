using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TextureSwitcher))]
public class TextureSwitcherEditor : Editor
{
    static GUIStyle style;
    static int selectType = 0;

    //在这里方法中就可以绘制面板。
    public override void OnInspectorGUI()
    {

        //得到Test对象
        TextureSwitcher test = (TextureSwitcher)target;
        if (style == null)
        {
            style = new GUIStyle();            
            style.normal.textColor = new Color(0, 1, 1);   //设置字体颜色的
            style.fontSize = 14;       //当然，这是字体颜色                   
        }

        EditorGUILayout.HelpBox("1.PLEASE ADD NEW ITEMS AND NAME THEY IN THE TEXTURE ARRAY FIRST.", MessageType.Info);

        base.OnInspectorGUI();
        GUILayout.Space(20);


        EditorGUILayout.HelpBox("2. PLEASE SELECT TEXTURES IN THE PROJECT PANEL,  AND PRESS OK", MessageType.Info);
        

        if (GUILayout.Button("Create", GUILayout.Height(50)))
        {
            object[] arr = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

            for (int k = 0; k < test.textures.Length; k++)
            {
                foreach (Texture2D t in arr)
                {
                    bool found = false;
                    for (int i = 0, c = test.textures[k].textures.Count; i < c; i++)
                    {
                        if (test.textures[k].textures[i].texture == t)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found) continue;

                    TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(t));
                    PerTexture2D per = new PerTexture2D();
                    per.texture = t;
                    per.maxSize = (MAX_SIZE)ti.maxTextureSize;
                    per.format = ti.textureFormat;
                    test.textures[k].textures.Add(per);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        GUILayout.Space(20);
        EditorGUILayout.HelpBox("2. NOW YOU CAN GO BACK TO PART1 TO EDIT PUBLISH SETTING, AND AFTER DONE, YOU CAN SWITCH TEXTURE QUALITY BY PRESS BLEW BTN", MessageType.Info);
        GUIContent[] contents = new GUIContent[test.textures.Length];
        GUILayout.Space(20);
        GUILayout.Label("CURRENT SETTING:");

        for (int i = 0; i < test.textures.Length; i++)
        {
            contents[i] = new GUIContent(test.textures[i].name);
        }
        selectType = GUILayout.SelectionGrid(selectType, contents, 5);

        GUILayout.Space(20);
        GUILayout.Label("QUICK SETTING:");
        GUILayout.Space(10);
        GUILayout.Label("FORMAT:");

        if (GUILayout.Button("[" + test.textures[selectType].name +"] ALL SET 2 ARBG16"))
        {
            foreach (PerTexture2D t in test.textures[selectType].textures)
            {
                t.format = TextureImporterFormat.ARGB16;
            }
            AssetDatabase.Refresh();
            Debug.Log("SUCCESS");
        }

        GUILayout.Label("SIZE:");

        if (GUILayout.Button("[" + test.textures[selectType].name + "] ALL SET 2 512"))
        {
            foreach (PerTexture2D t in test.textures[selectType].textures)
            {
                t.maxSize = MAX_SIZE.X512;
            }
            AssetDatabase.Refresh();
            Debug.Log("SUCCESS");
        }

        if (GUILayout.Button("[" + test.textures[selectType].name + "] ALL SET 2 1024"))
        {
            foreach (PerTexture2D t in test.textures[selectType].textures)
            {
                t.maxSize = MAX_SIZE.X1024;
            }
            AssetDatabase.Refresh();
            Debug.Log("SUCCESS");
        }


        if (GUILayout.Button("Switch", GUILayout.Height(50)))
        {
            AssetDatabase.StartAssetEditing();
            foreach (PerTexture2D t in test.textures[selectType].textures)
            {
                string path = AssetDatabase.GetAssetPath(t.texture);
                TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(path);
                PerTexture2D per = t;
                int size = (int)per.maxSize;
                if (size == 0)
                {
                    Debug.Log(per.maxSize + ":" + size );
                    continue;
                }
                ti.SetPlatformTextureSettings("Android", size , per.format,100, true);

                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);        
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
    }
}
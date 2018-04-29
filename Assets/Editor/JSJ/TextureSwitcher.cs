using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;


public class TextureSwitcher : ScriptableObject {    
    public PerTextureType[] textures;
}

[System.Serializable]
public class PerTextureType
{
    public string name;
    public List<PerTexture2D> textures;
}


[System.Serializable]
public class PerTexture2D
{
    public Texture2D texture;
    public TextureImporterFormat format;
    public MAX_SIZE maxSize = MAX_SIZE.X32;
}

[System.Serializable]
public enum MAX_SIZE
{
    X32 = 32,
    X64  = 64,
    X128 = 128,
    X256 = 256,
    X512 = 512,
    X1024 = 1024,
    X2048 = 2048,
    X4096 = 4096
}
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

public class GameInfo {

    
	
    public static PlatformData data= PlatformData.c("YMQ",
        PerPlatform.c("WX_DISPLAY", "乒乓球", PluginUtils.PLATFORM_TYPE.WX, PluginUtils.LANG_TYPE.EN, "/Plugins-wx/AndroidManifest.xml", "1.0.0",1),
        PerPlatform.c("WX_HIDE", "乒乓球", PluginUtils.PLATFORM_TYPE.WX, PluginUtils.LANG_TYPE.EN, "/Plugins-wx/AndroidManifest_hide.xml", "1.0.0", 1),
        PerPlatform.c("OS_DISPLAY", "乒乓球", PluginUtils.PLATFORM_TYPE.OS, PluginUtils.LANG_TYPE.EN, "/Plugins-os/AndroidManifest.xml", "1.0.0", 1),
        PerPlatform.c("OS_HIDE", "乒乓球", PluginUtils.PLATFORM_TYPE.OS, PluginUtils.LANG_TYPE.EN, "/Plugins-os/AndroidManifest_hide.xml", "1.0.0", 1)
        );

    public static PerPlatform GetPerPlatformByName(string name)
    {
        for (int i = 0; i < data.平台列表.Length; i++)
        {
            if (data.平台列表[i].艺名 == name)
            {
                return data.平台列表[i];
            }
        }
        return null;
    }

}
#endif

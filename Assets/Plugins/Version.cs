using System.Collections.Generic;
public class Version{ 
static public bool IsNone(){ return false; }
static public bool IsWX(){ return true; }
static public bool IsOS(){ return false; }
static public bool IsUSB(){ return false; }
static public bool IsCN(){ return true; }
static public bool IsEN(){ return false; }
public const bool SHOW_BUY_TIPS = true;
public const string packName = "com.waixing.action.jsjdance";
public const string gameName="舞动大师";
public enum PLAFTFORM_ENUM{OS_SHOW,WX_SHOW,WX_SHOW_NO_REGIST,WX_HIDE,OS_HIDE,TEL,OS_SHOW_LOGIN,USB,OS_HIDE_NO_REG,WX_XRDS,WX_XRDSS_DISPLAY,Think,SkyWorth_Dis_NoReg,HNDX_SHOW_NO_REG_NO_BUY};
public static PLAFTFORM_ENUM currentPlatform= PLAFTFORM_ENUM.WX_SHOW_NO_REGIST;
public static Dictionary<string, string> customData = new Dictionary<string, string>(){};}
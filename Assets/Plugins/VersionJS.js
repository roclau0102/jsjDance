#pragma strict 
static  public function IsNone(){ return false; }
static  public function IsWX(){ return true; }
static  public function IsOS(){ return false; }
static  public function IsUSB(){ return false; }
static  public function IsCN(){ return true; }
static  public function IsEN(){ return false; }
static public var packName:String = "com.waixing.action.jsjdance";
public  static var SHOW_BUY_TIPS:boolean = true;
static public var gameName:String="舞动大师";
public enum PLAFTFORM_ENUM{OS_SHOW,WX_SHOW,WX_SHOW_NO_REGIST,WX_HIDE,OS_HIDE,TEL,OS_SHOW_LOGIN,USB,OS_HIDE_NO_REG,WX_XRDS,WX_XRDSS_DISPLAY,Think,SkyWorth_Dis_NoReg,HNDX_SHOW_NO_REG_NO_BUY};
public static var currentPlatform:PLAFTFORM_ENUM= PLAFTFORM_ENUM.WX_SHOW_NO_REGIST;

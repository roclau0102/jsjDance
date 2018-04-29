using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// by james justnetfly@qq.com
/// </summary>
public class TableManager{

	//定义好表
	/// <summary>
	/// 开始加载 上方设置变量 下方设置加载
	/// </summary>
	public void StartLoad()
	{
        Load<CharacterData>((Resources.Load("Tables/Character") as TextAsset).text, ref Global.CHARACTER_TABLE);
        Load<PropData>((Resources.Load("Tables/Prop") as TextAsset).text, ref Global.PROP_TABLE);
	}



	#region 单例及初始化
	static TableManager tm;

	static public TableManager GetInstance(){
		
		return tm;
	}
	
	static public void Init(){
		if(tm==null){
			tm = new TableManager();
		}
	}


	public TableManager(){
		StartLoad();
	}
	#endregion

	
	#region 加载文件处理
	const int beginLine = 2;
	
	/// <summary>
	/// 从内存中读取数据
	/// </summary>
	/// <param name="str"></param>
	/// T Fanxing<T>(T t){
	public virtual void Load<T>(string text,ref Dictionary<int,T> dic)
	{	
        System.Type itemType = typeof(T);
        string[] h = { "\r\n" }; 
		string[] array = text.Split(h, System.StringSplitOptions.None);
		var firstLine = array[beginLine-1];
		char[] comma = { '\t' };
		var fields = firstLine.Split(comma, System.StringSplitOptions.None);

		string intType = typeof(int).ToString();
		string floatType = typeof(float).ToString();
		string stringType = typeof(string).ToString();
		string shortType = typeof(short).ToString();
		string byteType = typeof(byte).ToString();
		string boolType = typeof(bool).ToString();

		int v;
		float flt;
		byte bb =0;
		short s=0;

		int id = 0;
		for (int x = beginLine; x < array.Length; x++)
		{
			string strLine = array[x];
			string[] strs = strLine.Split(comma, System.StringSplitOptions.None);

			int.TryParse(strs[0],out id);
			if (id != 0)
			{
				if (!dic.ContainsKey(id))
				{
					var item = System.Activator.CreateInstance(itemType);
					for (int i = 0; i < strs.Length; i++)
					{
						if (fields[i] != "" ){
							var f = item.GetType().GetField(fields[i]);
							if(f!=null){
								string ftype = f.FieldType.ToString();
								if(ftype == intType){
									v=0;
									int.TryParse(strs[i],out v);
									f.SetValue(item,v);
								}else if(ftype == floatType){
									flt = 0;
									float.TryParse(strs[i],out flt);
									f.SetValue(item,flt );
								}else if(ftype == stringType){
									if(strs[i]!=""){ //如果前尾都是等于"的话，代表ＥＸＥＣＬ在所有的有逗号的字段中都自动在首末加了双引号，这里去掉这两个双引号
										if(strs[i].Substring(0,1)=="\"" && strs[i].Substring(strs[i].Length-1,1)=="\""){
											strs[i] = strs[i].Substring(1,strs[i].Length-2);
										}
										f.SetValue(item, strs[i]);												
									}
								}else if(ftype == shortType){
									s=0;
									short.TryParse(strs[i], out s);
									f.SetValue(item, s);
								}else if(ftype == byteType){
									bb =0;
									byte.TryParse(strs[i],out bb);
									f.SetValue(item, bb);
								}else if(ftype == boolType){
									f.SetValue(item, strs[i]=="1");
								}
							}
						}
					}
					dic[id] = (T)item;
				}else{
					Debug.Log("表[" + id + "]主键重复了！");
				}
			}
		}
		array = null;
	}
	#endregion
}


/**************************************************************************************************
	Copyright (C) 2016 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：CommonUtils.cs;
	作	者：W_X;
	时	间：2016 - 05 - 11;
	注	释：;
**************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonUtils
{

    // 获取一个Object的节点信息;
    public static string GetFullPath(this GameObject obj)
    {
        if (obj == null)
        {
            return "";
        }

        string path = obj.name;

        while (obj.transform.parent != null)
        {
            path = string.Format("{0}/{1}", obj.transform.parent.name, path);
            obj = obj.transform.parent.gameObject;
        }

        return path;
    }

    /// <summary>
    /// 将一个List拷贝入另一个List;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dest">目标</param>
    /// <param name="src">源数据</param>
    /// <param name="autoUniqueness">自动去除重复</param>
    public static void CopyFrom<T>(this List<T> dest, List<T> src, bool autoUniqueness = true)
	{
		if (src == null || dest == null || src.Count < 1)
		{
			return;
		}

		for (int i = 0; i < src.Count; i++)
		{
			T t = src[i];
			if (autoUniqueness && dest.Contains(t))
			{
				continue;
			}

			dest.Add(t);
		}
	}

    /// <summary>
	/// 将一个List拷贝入另一个List;
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="dest">目标</param>
	/// <param name="src">源数据</param>
	/// <param name="autoUniqueness">自动去除重复</param>
	public static void CopyFrom<T>(this List<T> dest, T[] src, bool autoUniqueness = true)
    {
        if (src == null || dest == null || src.Length < 1)
        {
            return;
        }

        for (int i = 0; i < src.Length; i++)
        {
            T t = src[i];
            if (autoUniqueness && dest.Contains(t))
            {
                continue;
            }

            dest.Add(t);
        }
    }

    public static Int32 GetMask(string str)
	{
		Int32 mask = 0;
		char[] charArray = str.ToCharArray();
		Int32 size = charArray.Length;
		for (Int32 i = 0; i < size; ++i)
		{
			if (charArray[size - i - 1] == '1')
			{
				mask |= (1 << i);
			}
		}
		return mask;
	}

	public static string SuperStringFormat(string str, string fightParam)
	{
		try
		{
			if (string.IsNullOrEmpty(fightParam))
			{
				return str;
			}
			string[] fightParamArray = fightParam.Split(new[] { ',' });
			string result = string.Format(str, fightParamArray);
			return result;
		}
		catch (System.Exception ex)
		{
			UnityEngine.Debug.LogError("严重错误！！！ 字符串和参数不匹配" + ex.StackTrace);
			return str;
		}
	}

    public static float[] GetFloatArr(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;

        string[] strs = str.Split(':');
        float[] arr = new float[strs.Length];
        for (int nIdx = 0; nIdx < strs.Length; nIdx++)
        {
            float.TryParse(strs[nIdx], out arr[nIdx]);
        }
        return arr;
    }

    public static int[] GetIntArr(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;

        string[] strs = str.Split(':');
        int[] arr = new int[strs.Length];
        for (int nIdx = 0; nIdx < strs.Length; nIdx++)
        {
            int.TryParse(strs[nIdx], out arr[nIdx]);
        }
        return arr;
    }

    public static string[] GetStrArr(string str, char ch)
    {
        if (string.IsNullOrEmpty(str)) return null;
        return str.Split(ch);
    }

    public static string SizeConvertToString(UInt32 size)
    {
        if ( size < 1024 )
        {
            return string.Format("{0}B", size);
        }

        if ( size < 1024 * 1024 )
        {
            return string.Format("{0}KB", (size / (1024f)).ToString("0") );
        }

        return string.Format("{0}MB", (size / (1024f * 1024f)).ToString("0"));
    }
}

/**************************************************************************************************
	Copyright (C) 2016 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：EditorCommon.cs;
	作	者：W_X;
	时	间：2017 - 02 - 09;
	注	释：;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public class EditorCommon
{
    static EditorCommon()
    {
        EditorApplication.update += OnUpdate;
    }

    public static void ClearConsole()
    {
        // This simply does "LogEntries.Clear()" the long way:  
        var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }

    public static void AddDefine(string strDefine)
    {
        if (Application.isPlaying)
        {
            UnityEditor.EditorUtility.DisplayDialog("Error", "运行时不能开启此选项", "好的");
            return;
        }

        if ( string.IsNullOrEmpty(strDefine) )
        {
            return;
        }

        string strCurDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(PackBundleTools.group);
        if ( string.IsNullOrEmpty(strCurDefine) )
        {
            strCurDefine = strDefine;
        }
        else if ( !strCurDefine.Contains(strDefine) )
        {
            strCurDefine = string.Format("{0};{1}", strCurDefine, strDefine);
        }

        EditorApplication.update += OnUpdate;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(PackBundleTools.group, strCurDefine);
    }

    public static void RemoveDefine(string strDefine)
    {
        if (Application.isPlaying)
        {
            UnityEditor.EditorUtility.DisplayDialog("Error", "运行时不能开启此选项", "好的");
            return;
        }

        if (string.IsNullOrEmpty(strDefine))
        {
            return;
        }
        string strCurDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(PackBundleTools.group);

        strCurDefine = strCurDefine.Replace(string.Format(";{0}", strDefine), "");
        strCurDefine = strCurDefine.Replace(strDefine, "");

        EditorApplication.update += OnUpdate;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(PackBundleTools.group, strCurDefine);
    }

    static void OnUpdate()
    {
        if (EditorApplication.isCompiling)
        {
            EditorUtility.DisplayProgressBar("编译中", "正在编译中......", 0.5f);
            return;
        }

        EditorApplication.update -= OnUpdate;
        EditorUtility.ClearProgressBar();
    }

    public static List<string> GetPrefabs(string folder)
    {
        return GetAllFiles(folder, "*.prefab");
    }

    public static List<string> GetBundles(string folder)
    {
        return GetAllFiles(folder, "*.ab");
    }

    public static List<string> GetTextures(string folder)
    {
        return GetAllFiles(folder, "*.png", "*.jpg", "*.tga");
    }

    public static List<string> GetAudio(string folder)
    {
        return GetAllFiles(folder, "*.wav", "*.ogg", "*.mp3");
    }

    // 获取导出角色资源;
    public static List<string> GetExportCharacters(string folder)
    {
        return GetAllFiles(folder, "*.prefab", "*.anim");
    }

    public static List<string> GetMaterials(string folder)
    {
        return GetAllFiles(folder, "*.mat");
    }

    public static List<string> GetAllFiles(string folder, params string[] searchPatterns)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return null;
        }

        if (!Directory.Exists(folder))
        {
            return null;
        }

        if (searchPatterns == null || searchPatterns.Length < 1)
        {
            searchPatterns = new string[] { "" };
        }

        List<string> fileList = new List<string>();
        for(int i=0;i< searchPatterns.Length;i++)
        {
            string searchPattern = searchPatterns[i];
            string[] files = null;
            if (string.IsNullOrEmpty(searchPattern))
            {
                files = Directory.GetFiles(folder, "", SearchOption.AllDirectories);
            }
            else
            {
                files = Directory.GetFiles(folder, searchPattern, SearchOption.AllDirectories);
            }

            for(int j=0;j<files.Length;j++)
            {
                string file = files[j];
                if (fileList.Contains(file))
                {
                    continue;
                }

                string path = file.Replace("\\", "/");
                fileList.Add(path);
            }
        }

        //string[] directories = Directory.GetDirectories(folder);
        //foreach (string directory in directories)
        //{
        //	List<string> files = GetAllFiles(directory, searchPattern);
        //	if (files == null || files.Count < 1 )
        //	{
        //		continue;
        //	}

        //	foreach (string file in files)
        //	{
        //		if (fileList.Contains(file))
        //		{
        //			continue;
        //		}

        //		fileList.Add(file);
        //	}
        //}

        return fileList;
    }

    public static bool IsPrefab(string path)
    {
        return IsRes(path, ".prefab");
    }

    public static bool IsAtlas(string path)
    {
        bool result = IsRes(path, ".prefab");
        if (!result)
        {
            return false;
        }

        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        return true;
    }

    public static bool IsTexture(string path)
    {
        return IsRes(path, ".jpg", ".tga", ".dds", ".png", ".dxt", ".psd", ".bmp", ".cubemap");
    }

    public static bool IsFont(string path)
    {
        return IsRes(path, ".ttf");
    }

    public static bool IsAnimatorController(string path)
    {
        return IsRes(path, ".controller");
    }

    public static bool IsModel(string path)
    {
        return IsRes(path, ".fbx");
    }

    public static bool IsRes(string path, params string[] extensions)
    {
        if (string.IsNullOrEmpty(path) || extensions == null || extensions.Length < 1)
        {
            return false;
        }

        string extension = System.IO.Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }
        extension = extension.ToLower();

        for (int i = 0; i < extensions.Length; i++)
        {
            if (string.Equals(extensions[i], extension))
            {
                return true;
            }
        }

        return false;
    }
}

/**************************************************************************************************
	Copyright (C) 2018 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：ShaderTools.cs;
	作	者：W_X;
	时	间：2018 - 01 - 25;
	注	释：;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class ShaderTools
{
    [MenuItem("Tools/Shader/检查选中Shader的LOD", priority = 120)]
    static void CheckSelectShader()
    {
        if (Selection.activeObject == null) return;

        Shader shader = Selection.activeObject as Shader;
        if (shader == null) return;

        int lod = GetShaderLOD(AssetDatabase.GetAssetPath(shader));
        Debug.Log(lod);
    }

    [MenuItem("Tools/Shader/检查所有Shader的LOD", priority = 110)]
    static void CheckAllShadersLOD()
    {
        // 获取所有的Shader;
        string[] allfiles = Directory.GetFiles("Assets", "*.shader", SearchOption.AllDirectories);
        List<string> shader_infos = CheckShaderLods(allfiles);

        if ( shader_infos == null || shader_infos.Count < 1 )
        {
            EditorUtility.DisplayDialog("ShaderLOD", "检测完毕", "OK");
            return;
        }

        string message = string.Format("检测完毕，有{0}个Shader存在LOD风险，详情请看LOD", shader_infos.Count);

        foreach (string shader_info in shader_infos)
        {
            Debug.LogError(shader_info);
            if ( shader_infos.Count < 5 )
            {
                message = string.Format("{0}\n{1}", message, shader_info);
            }
        }

        EditorUtility.DisplayDialog("ShaderLOD", message, "OK");


    }

    public static List<string> CheckShaderLods(string[] allFiles, int minLod = 100)
    {
        if ( allFiles == null )
        {
            return null;
        }

        List<string> shader_infos = new List<string>();

        for ( int i=0; i<allFiles.Length; i++ )
        {
            string file = allFiles[i];
            PackAssetBundleUtlis.ShowProgress(i, allFiles.Length, "检测ShaderLOD", file);

            int shader_lod = GetShaderLOD(file);
            if ( shader_lod <= minLod ) continue;

            shader_infos.Add(string.Format("{0} ----- minLod = {1}", file, shader_lod));
        }

        EditorUtility.ClearProgressBar();
        return shader_infos;
    }

    static int GetShaderLOD(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        if (!path.EndsWith(".shader"))
        {
            return 0;
        }

        string text = GetText(path);
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        int lod = GetShaderLODByText(text);
        return lod;
    }

    static int GetShaderLODByText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        // 匹配所有的LOD值;
        int lod = 0;
        string lodTag2 = string.Format("(?<=({0}))[.\\s\\S]*?(?=({1}))", "LOD ", "\n");
        MatchCollection mc = Regex.Matches(text, lodTag2);
        for (int i = 0; i < mc.Count; i++)
        {
            string value = mc[i].Value;

            // 过滤为数字;
            Match match = Regex.Match(value, "\\d+");

            if (match.Success)
            {
                try
                {
                    int matchLod = System.Convert.ToInt32(match.Value);
                    if (matchLod < lod || lod == 0)
                    {
                        lod = matchLod;
                    }
                }
                catch { }
            }
        }

        if (lod > 0)
        {
            if (text.ToLower().Contains("fallback \""))
            {
                lod = -2;
            }
        }

        return lod;
    }

    static string GetText(string file)
    {
        if (string.IsNullOrEmpty(file)) return null;
        if (!File.Exists(file)) return null;

        FileStream fs = File.Open(file, FileMode.Open);
        StreamReader reader = new StreamReader(fs);
        string strText = reader.ReadToEnd();
        fs.Close();

        return strText;
    }
}
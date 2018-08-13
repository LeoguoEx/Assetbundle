
//********************************************************************
//	BUILDCOMMON.CS 文件注释
//  文件名 :		BUILDCOMMON.CS
//  作者 :		Wang_X
//  创建时间 :	2012/11/28 13:54
//  文件描述 :	
//*********************************************************************

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

public enum BuildType
{
    BuildEditor = 1,
    BuildRes = 2,
    BuildAsset = 3,
}

public class BuildCommon
{
    public static string mSceneTempFolder = "Assets/SceneExportTemp/";
    static string mTmpConfigFolder = "Assets/Tmp/";

    public static string mFileListName = "FileList.t3lk";

    /// <summary>
    /// 将json数据写入文件中
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="data"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string WriteJsonToFile(string baseDir, string fileName, System.Object data, bool format = true)
    {
        if (!string.IsNullOrEmpty(baseDir) && !System.IO.Directory.Exists(baseDir))
        {
            System.IO.Directory.CreateDirectory(baseDir);
        }

        string strJson = string.Empty;//LitJson.JsonMapper.ToJson(data);
        if (format)
        {
            strJson = QuickFormatJson(strJson);
        }
        string filePath = System.IO.Path.Combine(baseDir, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        FileStream fs = File.Open(filePath, FileMode.Create);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write(strJson);
        writer.Flush();
        writer.Close();
        fs.Close();
        return filePath;
    }

    public static T ReadJsonFromFile<T>(string file) where T : class
    {
        if ( string.IsNullOrEmpty(file) )
        {
            return null;
        }

        if ( !File.Exists(file) )
        {
            return null;
        }

        FileStream fs = File.Open(file, FileMode.Open);
        StreamReader reader = new StreamReader(fs);
        string strJson = reader.ReadToEnd();

        T data = null;
        try
        {
            //data = LitJson.JsonMapper.ToObject<T>(strJson);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }        

        fs.Close();

        return data;
    }

    //public static string WriteToFile(string filePath, System.Object data, BuildType bldType = BuildType.BuildAsset, bool format = true)
    //{
    //    switch (bldType)
    //    {
    //        case BuildType.BuildAsset:
    //            {
    //                filePath = EditorSceneSetting.PkgDataFolder + "/" + filePath;
    //                if (!Directory.Exists(EditorSceneSetting.PkgDataFolder))
    //                {
    //                    Directory.CreateDirectory(EditorSceneSetting.PkgDataFolder);
    //                }
    //                break;
    //            }
    //        case BuildType.BuildRes:
    //            {
    //                string fileName = System.IO.Path.GetFileName(filePath);
    //                filePath = EditorSceneSetting.PkgResConfigFolser + "/" + fileName;
    //                if (!Directory.Exists(EditorSceneSetting.PkgResConfigFolser))
    //                {
    //                    Directory.CreateDirectory(EditorSceneSetting.PkgResConfigFolser);
    //                }
    //                break;
    //            }
    //        case BuildType.BuildEditor:
    //            {
    //                string fileName = System.IO.Path.GetFileName(filePath);
    //                filePath = EditorSceneSetting.PkgEditorFolder + "/" + fileName;
    //                if (!Directory.Exists(EditorSceneSetting.PkgEditorFolder))
    //                {
    //                    Directory.CreateDirectory(EditorSceneSetting.PkgEditorFolder);
    //                }
    //                break;
    //            }
    //        default:
    //            {
    //                break;
    //            }
    //    }

    //    string strJson = LitJson.JsonMapper.ToJson(data);
    //    if (format)
    //    {
    //        strJson = FormatJson(strJson);
    //    }
    //    if (File.Exists(filePath))
    //    {
    //        File.Delete(filePath);
    //    }

    //    FileStream fs = File.Open(filePath, FileMode.Create);
    //    StreamWriter writer = new StreamWriter(fs);
    //    writer.Write(strJson);
    //    writer.Flush();

    //    writer.Close();
    //    fs.Close();

    //    return filePath;
    //}

    //public static bool ReadFromFile<T>(string filePath, ref T data)
    //{
    //    if (string.IsNullOrEmpty(filePath) == true || File.Exists(filePath) == false)
    //    {
    //        return false;
    //    }

    //    FileStream fs = File.Open(filePath, FileMode.Open);
    //    StreamReader reader = new StreamReader(fs);

    //    try
    //    {
    //        data = LitJson.JsonMapper.ToObject<T>(reader);
    //    }
    //    catch (System.Exception ex)
    //    {
    //        reader.Close();
    //        fs.Close();
    //        Debug.LogError(ex.ToString());
    //        return false;
    //    }

    //    reader.Close();
    //    fs.Close();

    //    return true;

    //}

    public static bool ReadXmlFromFile<T>(string filePath, ref T data) where T : class
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        XmlSerializer x = new XmlSerializer(typeof(T));
        FileStream f = new FileStream(filePath, FileMode.Open);
        try
        {
            data = x.Deserialize(f) as T;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("ReadXmlFromFile Failed {0}, {1}, {2}", filePath, typeof(T).ToString(), ex.ToString()));
            f.Close();
            return false;
        }

        f.Close();

        return true;
    }

    public static bool WriteXmlToFile(System.Object obj, string filePath) 
    {
        if ( string.IsNullOrEmpty(filePath) )
        {
            return false;
        }

        string dirPath = Path.GetDirectoryName(filePath);
        if ( string.IsNullOrEmpty(dirPath) )
        {
            return false;
        }

        string strInfo = WriteToXmlString(obj);
        if ( string.IsNullOrEmpty(strInfo) )
        {
            return false;
        }

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        FileStream fs = File.Open(filePath, FileMode.Create);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write(strInfo);
        writer.Flush();
        writer.Close();
        fs.Close();

        return true;
    }

    public static string WriteToXmlString(System.Object obj)
    {
        string ret = "";
        try
        {
            XmlSerializer x = new XmlSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.UTF8);            
            x.Serialize(sw, obj);
            ret = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            ms.Close();
            sw.Close();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.Log(ex.ToString());
        }

        return ret;
    }
    

    /// <summary>
    /// Json格式化;
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static string FormatJson(string val)
    {
        string retval = "";
        string str = val;
        int pos = 0;
        int strLen = str.Length;

        char indentStr = '\t';
        char newLine = '\n';

        char charVar = new char();

        for (int i = 0; i < strLen; i++)
        {
            charVar = str[i];

            if (charVar == '}' || charVar == ']')
            {
                retval = retval + newLine;
                pos = pos - 1;

                for (int j = 0; j < pos; j++)
                {
                    retval = retval + indentStr;
                }

            }

            retval = retval + charVar;

            if (charVar == '{' || charVar == '[' || charVar == ',')
            {
                retval = retval + newLine;

                if (charVar == '{' || charVar == '[')
                {
                    pos = pos + 1;
                }

                for (int k = 0; k < pos; k++)
                {
                    retval = retval + indentStr;
                }

            }

        }

        return retval;
    }

    public static string QuickFormatJson(string val)
    {
        //格式化json字符串;
        JsonSerializer serializer = new JsonSerializer();
        TextReader tr = new StringReader(val);
        JsonTextReader jtr = new JsonTextReader(tr);
        object obj = serializer.Deserialize(jtr);
        if ( obj == null )
        {
            return val;
        }

        StringWriter textWriter = new StringWriter();
        JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            Indentation = 4,
            IndentChar = ' '
        };
        serializer.Serialize(jsonWriter, obj);
        return textWriter.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strType"></param>
    //public static void ClearDataFolder(string strType, BuildType bldType = BuildType.BuildAsset)
    //{
    //    string dataFolder = EditorSceneSetting.PkgDataFolder;
    //    switch (bldType)
    //    {
    //        case BuildType.BuildAsset:
    //            {
    //                dataFolder = EditorSceneSetting.PkgDataFolder;
    //                break;
    //            }
    //        case BuildType.BuildEditor:
    //            {
    //                return;
    //            }
    //        case BuildType.BuildRes:
    //            {
    //                dataFolder = EditorSceneSetting.PkgDataResFolder;
    //                break;
    //            }
    //        default:
    //            {
    //                dataFolder = EditorSceneSetting.PkgDataFolder;
    //                break;
    //            }
    //    }

    //    strType = strType.ToLower();

    //    //string[] directories = Directory.GetDirectories(dataFolder);
    //    //foreach ( string directory in directories )
    //    {
    //        string typeFolder = dataFolder + "/" + strType;
    //        bool typefolderExists = Directory.Exists(typeFolder); //Directory.GetDirectoryRoot(strType);

    //        if (typefolderExists == true)
    //        {
    //            // 清除临时文件夹下所有未删除的ab文件;
    //            string[] files = Directory.GetFiles(typeFolder);
    //            foreach (string file in files)
    //            {
    //                AssetDatabase.DeleteAsset(file);
    //            }

    //            Directory.Delete(typeFolder, true);
    //            Debug.Log("Delete : " + typeFolder);
    //        }
    //    }
    //    if (bldType == BuildType.BuildAsset)
    //    {
    //        dataListInfo.Clear(strType);
    //        BuildFileListInfo(false);
    //    }
    //}

    public static void DeleteFolder(string folderPath, bool quick = false)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            return;
        }

        if (quick)
        {
            string[] folders = Directory.GetDirectories(folderPath);
            foreach (string folder in folders)
            {

                DeleteFolder(folder, quick);
            }
        }

        string[] files = Directory.GetFiles(folderPath);
        foreach (string file in files)
        {
            if (File.Exists(file))
            {
                try
                {
                    Debug.Log(string.Format("Delete file = {0}", file));
                    UnityEditor.FileUtil.DeleteFileOrDirectory(file);
                    //AssetDatabase.DeleteAsset(file);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(string.Format("Delete file = {0}, error = {1}", file, ex.ToString()));
                }

            }
        }

        Directory.Delete(folderPath, true);
    }

    // 拷贝文件夹，且忽略git文件;
    public static void CopyFolder(string srcPath, string destPath, bool igroneGit = true)
    {
        if (!Directory.Exists(srcPath))
        {
            return;
        }

        if (!igroneGit)
        {
            UnityEditor.FileUtil.CopyFileOrDirectory(srcPath, destPath);
            return;
        }

        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        string[] folders = Directory.GetDirectories(srcPath);
        foreach (string folder in folders)
        {
            string folderName = System.IO.Path.GetFileName(folder);
            if (string.Equals(folderName, ".git"))
            {
                continue;
            }
            UnityEditor.FileUtil.CopyFileOrDirectory(folder,
                string.Format("{0}/{1}", destPath, folderName));
        }

        string[] files = Directory.GetFiles(srcPath);
        foreach (string file in files)
        {
            UnityEditor.FileUtil.CopyFileOrDirectory(file,
                string.Format("{0}/{1}", destPath, System.IO.Path.GetFileName(file)));
        }
    }

    public static void DeleteFileOrDirectory(string srcPath, bool igroneGit = true)
    {
        if (!Directory.Exists(srcPath))
        {
            return;
        }

        if (!igroneGit)
        {
            UnityEditor.FileUtil.DeleteFileOrDirectory(srcPath);
            return;
        }

        string[] folders = Directory.GetDirectories(srcPath);
        foreach (string folder in folders)
        {
            string folderName = System.IO.Path.GetFileName(folder);
            if (string.Equals(folderName, ".git"))
            {
                continue;
            }
            UnityEditor.FileUtil.DeleteFileOrDirectory(folder);
        }

        string[] files = Directory.GetFiles(srcPath);
        foreach (string file in files)
        {
            UnityEditor.FileUtil.DeleteFileOrDirectory(file);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void ClearTempFolder()
    {
        // 清除临时文件夹下所有未删除的prefab文件;
        string[] files = Directory.GetFiles(mSceneTempFolder);
        foreach (string file in files)
        {
            if (file.Contains(".prefab") == false)
            {
                continue;
            }

            AssetDatabase.DeleteAsset(file);
        }

        if (Directory.Exists(mTmpConfigFolder) == false)
        {
            Directory.CreateDirectory(mTmpConfigFolder);
        }
        files = Directory.GetFiles(mTmpConfigFolder);
        foreach (string file in files)
        {
            AssetDatabase.DeleteAsset(file);
        }
    }

    public static string GetExtension(string szAssetPath)
    {
        string extension = System.IO.Path.GetExtension(szAssetPath);
        return string.IsNullOrEmpty(extension) ? null : extension.ToLower();
    }

    /// <summary>
    /// 判断是否为资源;
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static bool IsResourceAssetNoFbx(string szAssetPath,string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".jpg"
                || extension == ".tga"
                || extension == ".dds"
                || extension == ".png"
                || extension == ".dxt"
                || extension == ".psd"
                || extension == ".bmp"
                || extension == ".wav"
                || extension == ".mp3"
                || extension == ".exr"
            )
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断是否为资源（手机版，FBX不需要打包）;
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static bool IsMobileSceneResource(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".jpg"
                || extension == ".tga"
                || extension == ".dds"
                || extension == ".png"
                || extension == ".dxt"
                || extension == ".psd"
                || extension == ".bmp"
                || extension == ".wav"
                || extension == ".mp3"
                || extension == ".exr"
            )
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断是shader需要另外打包
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static bool IsMobileShader(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".shader")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断是否为资源;
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static bool IsResourceAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".jpg"
                || extension == ".tga"
                || extension == ".dds"
                || extension == ".png"
                || extension == ".dxt"
                || extension == ".psd"
                || extension == ".bmp"
                || extension == ".fbx"
                || extension == ".wav"
                || extension == ".mp3"
                || extension == ".FBX"
                || extension == ".exr"
            )
        {
            return true;
        }

        return false;
    }

    public static bool IsModelAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".fbx")
        {
            return true;
        }

        return false;
    }

    public static bool IsBigModelAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension != ".fbx")
        {
            return false;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath(szAssetPath, typeof(GameObject)) as GameObject;
        if ( prefab == null )
        {
            return false;
        }

        MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
        if ( meshFilter != null && meshFilter.sharedMesh != null)
        {
            if (meshFilter.sharedMesh.vertexCount > 1024)
            {
                return true;
            }
        }

        //int skinVerCount = SkinMeshVertexCount(prefab);
        //if (skinVerCount > 1024)
        //{
        //    return true;
        //}

        return false;
    }

    public static int SkinMeshVertexCount(GameObject gObj)
    {
        if ( gObj == null )
        {
            return 0;
        }

        SkinnedMeshRenderer[] skinRenders = gObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        int count = 0;
        foreach (SkinnedMeshRenderer render in skinRenders )
        {
            if ( render == null )
            {
                continue;
            }
            
            count += (render.sharedMesh != null ? render.sharedMesh.vertexCount : 0);
        }
        return count;
    }

    public static bool IsScriptAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".cs")
        {
            return true;
        }

        return false;
    }


    public static bool IsShaderAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".shader")
        {
            return true;
        }

        return false;
    }


    public static bool IsMaterialAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".mat")
        {
            return true;
        }

        return false;
    }

    public static bool IstTTFFontAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".ttf")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断是否为声音资源;
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static bool IsAudioAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".wav"
                || extension == ".mp3"
            )
        {
            return true;
        }

        return false;
    }
    
    public static bool IsLightMap(string szAssetPath)
    {
        if (string.IsNullOrEmpty(szAssetPath))
        {
            return false;
        }
        if (szAssetPath.Contains("light.exr"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断是否为纹理资源;
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static bool IsTextureAsset(string szAssetPath, string extension, bool ingore = true)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".jpg"
                || extension == ".tga"
                || extension == ".dds"
                || extension == ".png"
                || extension == ".dxt"
                || extension == ".psd"
                || extension == ".bmp"
                || extension == ".cubemap"
            )
        {
            if (!ingore)
            {
                return true;
            }

            szAssetPath = szAssetPath.ToLower();
            if (szAssetPath.Contains("reflectionprobe-"))
            {
                // 反射贴图一定跟场景走，不单独打包;
                return false;
            }

            Texture tex = AssetDatabase.LoadAssetAtPath(szAssetPath, typeof(Texture)) as Texture;
            if (tex != null )
            {
                return Mathf.Max(tex.width, tex.height) > 64;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断是否为Prefab资源;
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static Font IsUIFontAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return null;
        }

        if (extension == ".prefab")
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath(szAssetPath, typeof(GameObject)) as GameObject;
            if (prefab == null)
            {
                return null;
            }

            return prefab.GetComponent<Font>();
        }

        return null;
    }

    public static List<string> GetTexturesFromMaterial(Material mat)
    {
        if (mat == null)
        {
            return new List<string>();
        }

        string filePath = AssetDatabase.GetAssetPath(mat);
        if (string.IsNullOrEmpty(filePath))
        {
            return new List<string>();
        }

        return GetDepTextures(filePath);
    }

    public static List<string> GetDepTextures(string filePath)
    {
        List<string> depList = new List<string>();
        if (string.IsNullOrEmpty(filePath))
        {
            return depList;
        }

        string[] ass = { filePath };
        string[] deps = AssetDatabase.GetDependencies(ass);

        foreach (string dep in deps)
        {
            if (!BuildCommon.IsTextureAsset(dep, BuildCommon.GetExtension(dep)))
            {
                continue;
            }

            depList.Add(dep);
        }

        return depList;
    }

    /// <summary>
    /// 判断是否为Prefab资源;
    /// </summary>
    /// <param name="szAssetPath"></param>
    /// <returns></returns>
    public static bool IsPrefabAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".prefab")
        {
            return true;
            //GameObject prefab = AssetDatabase.LoadAssetAtPath(szAssetPath, typeof(GameObject)) as GameObject;
            //if (prefab != null && (prefab.GetComponent<UIAtlas>() || prefab.GetComponent<UIFont>()))
            //{
            //    return true;
            //}
        }

        return false;
    }

    public static bool IsAtlasAsset(string szAssetPath, string extension)
    {
//        if (string.IsNullOrEmpty(extension))
//        {
//            return false;
//        }
//
//        if (extension == ".prefab")
//        {
//            GameObject prefab = AssetDatabase.LoadAssetAtPath(szAssetPath, typeof(GameObject)) as GameObject;
//            if (prefab != null && prefab.GetComponent<UIAtlas>() != null )
//            {
//                return true;
//            }
//        }

        return false;
    }

    public static bool IsAnimAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".anim")
        {
            return true;
        }

        return false;
    }

    public static bool IsControllerAsset(string szAssetPath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        if (extension == ".controller")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 根据传入的Object，创建一个Prefab;
    /// </summary>
    /// <param name="gameobj"></param>
    /// <returns></returns>
    public static UnityEngine.Object CreatePrefab(GameObject gameobj)
    {
        if (gameobj == null)
        {
            return null;
        }

        //Judge a file whether existence
        string localPath = BuildCommon.mSceneTempFolder + gameobj.name + ".prefab";
        if (AssetDatabase.LoadAssetAtPath(localPath, typeof(UnityEngine.Object)) != null)
        {
            //rename
            int i = 0;
            do
            {
                localPath = BuildCommon.mSceneTempFolder + gameobj.name + "_" + i + "_.prefab";
                i++;
            }
            while (AssetDatabase.LoadAssetAtPath(localPath, typeof(UnityEngine.Object)) != null);
        }

        UnityEngine.Object newPrefab = PrefabUtility.CreateEmptyPrefab(localPath);
        newPrefab = PrefabUtility.ReplacePrefab(gameobj, newPrefab);

        return newPrefab;
    }

    public static void Build()
    {
        //string mSceneListConfig = PackSetting.PkgDataFolder + "/config.xml";
        //Dictionary<string, string> sceneList = new Dictionary<string, string>();

        //if (File.Exists(mSceneListConfig) == true)
        //{
        //	FileStream fs = File.Open(mSceneListConfig, FileMode.Open);
        //	StreamReader reader = new StreamReader(fs);

        //	sceneList = LitJson.JsonMapper.ToObject<Dictionary<string, string>>(reader);

        //	reader.Close();
        //	fs.Close();
        //}

        //List<UnityEngine.Object> sceneConfigs = new List<UnityEngine.Object>();
        //List<string> tmpSceneConfigObj = new List<string>();

        //List<string> allConfigFiles = new List<string>();

        //foreach (KeyValuePair<string, string> iter in sceneList)
        //{
        //	// 不存在就跳过这次检测;
        //	if (File.Exists(iter.Value) == false)
        //	{
        //		continue;
        //	}

        //	allConfigFiles.Add(iter.Value);

        //	FileStream fs = File.Open(iter.Value, FileMode.Open);
        //	StreamReader reader = new StreamReader(fs);

        //	char[] buffer = new char[fs.Length];
        //	reader.Read(buffer, 0, buffer.Length);

        //	reader.Close();
        //	fs.Close();

        //	TextFileHolder holder = ScriptableObject.CreateInstance<TextFileHolder>();
        //	holder.bin = new string(buffer);

        //	string tmpConfig = mTmpConfigFolder;
        //	if (Directory.Exists(tmpConfig) == false)
        //	{
        //		Directory.CreateDirectory(tmpConfig);
        //	}
        //	tmpConfig = tmpConfig + iter.Key + ".asset";

        //	AssetDatabase.CreateAsset(holder, tmpConfig);
        //	AssetDatabase.SaveAssets();

        //	BuildCommon.ImportAsset(tmpConfig);
        //	UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(tmpConfig, typeof(TextFileHolder));
        //	if (obj == null)
        //	{
        //		AssetDatabase.DeleteAsset(tmpConfig);
        //		UnityEngine.Debug.LogError(tmpConfig + " load error");
        //	}
        //	else
        //	{
        //		obj.name = iter.Key;
        //		sceneConfigs.Add(obj);
        //		tmpSceneConfigObj.Add(tmpConfig);
        //	}

        //}

        //string configFolder = PackSetting.PkgDataFolder + "/";

        //string sceneAssetPath = configFolder + PackSetting.PkgAllSceneAsset;
        //if (Directory.Exists(configFolder) == false)
        //{
        //	Directory.CreateDirectory(configFolder);
        //}
        //BuildAssetBundleOptions op = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
        //UnityEngine.Object[] objs = sceneConfigs.ToArray();

        //string md5 = BuildCommon.GetMD5FromFiles(allConfigFiles.ToArray());

        //// // 调用接口，替换成BuildCommon.BuildAssetBundle;
        //BuildAssetBundle(PackSetting.PkgBasic, md5, null, objs, sceneAssetPath, op);


        //// 打包FileList
        //BuildFileListInfo();
    }

    //[MenuItem("T3/AssetBundle(ab包)/BuildFileList")]
    static void BuildFileList()
    {
        BuildFileListInfo(true);
    }

    public static void BuildFileListInfo(bool buildAb = true)
    {
        //if (dataListInfo == null)
        //{
        //	return;
        //}

        //string filePathAsset = PackSetting.PkgDataFolder + "/FileList.ab";
        ////System.Int32 versionID = MakeResourceVersion.version.GetVersion();
        //string md5 = BuildCommon.GetFileMD5FromPath(PackSetting.PkgDataFolder + "/" + mFileListName);
        //dataListInfo.AddDataInfo(PackSetting.PkgBasic, filePathAsset, md5);
        ////dataListInfo.AddDataInfo(PackSetting.PkgBasic, filePathAsset, md5, versionID);

        //dataListInfo.UpdateDataList();

        //string filePath = WriteToFile(mFileListName, dataListInfo, BuildType.BuildAsset,false);


        //// 不存在就跳过这次检测
        //if (File.Exists(filePath) == false)
        //{
        //	return;
        //}

        //FileStream fs = File.Open(filePath, FileMode.Open);
        //StreamReader reader = new StreamReader(fs);

        //char[] buffer = new char[fs.Length];
        //reader.Read(buffer, 0, buffer.Length);

        //reader.Close();
        //fs.Close();

        //if (!buildAb)
        //{
        //	return;
        //}

        //TextFileHolder holder = ScriptableObject.CreateInstance<TextFileHolder>();
        //holder.bin = new string(buffer);

        //string tmpConfig = mTmpConfigFolder;
        //if (Directory.Exists(tmpConfig) == false)
        //{
        //	Directory.CreateDirectory(tmpConfig);
        //}
        //tmpConfig = tmpConfig + "FileList.asset";

        //AssetDatabase.CreateAsset(holder, tmpConfig);
        //AssetDatabase.SaveAssets();

        //BuildCommon.ImportAsset(tmpConfig);
        //UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(tmpConfig, typeof(TextFileHolder));
        //if (obj == null)
        //{
        //	return;
        //}

        ////BuildAssetBundle(PackSetting.PkgBasic, versionID, md5, obj, null, filePathAsset, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
        //BuildAssetBundle(PackSetting.PkgBasic, md5, obj, null, filePathAsset, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);

        //dataListInfo.ClearAll();
        //mDataListInfo = null;

        //// 生成FileList时，顺便生成一下微端的FileList;
        //BuildMiniFileList.Build();
    }

    /// <summary>
    /// 获得一个Asset资源包的大小;
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static System.Int32 getAssetSize(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError(string.Format("{0}打包生成失败！", path));
            return 0;
        }
        FileInfo fileInfo = new FileInfo(path);

        return (System.Int32)fileInfo.Length;
    }

    public static string GetConfPackPath()
    {
        string path = null;
        string confFile = Application.dataPath + @"\Conf\Pack.config";
        if (!File.Exists(confFile))
        {
#if UNITY_IPHONE
			path = Application.dataPath + @"/../../conf";
			Debug.Log ( "GetConfPackPath " + path );
#else
            path = @"T:";
#endif

            return path;
        }
        StreamReader sr = new StreamReader(confFile);
        //Dictionary<string, string> PathMap = new Dictionary<string, string>();
        string line = null;
        line = sr.ReadLine();
        string[] sArray = line.Split('*');
        return sArray[1];
    }

    public static void ImportAsset(string path, bool isAll = false)
    {
        return;
        //if (isAll)
        //{
        //    AssetDatabase.ImportAsset(path,ImportAssetOptions.ImportRecursive);
        //}
        //else
        //{
        //    if (IsAudioAsset(path))
        //    {
        //        return;
        //    }
        //    AssetDatabase.ImportAsset(path);
        //}
    }

    private static Material s_Defauls_Mat;
    private static Material defaultMat
    {
        get
        {
            if (s_Defauls_Mat == null)
            {
                s_Defauls_Mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Default.mat");
            }
            return s_Defauls_Mat;
        }
    }

    public static void ChangeMaterial( GameObject gObj )
    {
        if (gObj == null)
        {
            return;
        }

        SkinnedMeshRenderer[] skinRenders = gObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        string matInfo = "";
        foreach (SkinnedMeshRenderer render in skinRenders)
        {
            if (render == null || render.sharedMaterial == null)
            {
                continue;
            }

            render.sharedMaterial.shader = null;
            matInfo = string.Format("{0}, mat = {1}", matInfo, render.sharedMaterial);
        }

    }
}
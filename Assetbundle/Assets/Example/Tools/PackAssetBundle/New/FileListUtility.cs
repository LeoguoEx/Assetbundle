
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：FileListUtility.cs;
	作	者：W_X;
	时	间：2017 - 08 - 18;
	注	释：FileList工具;
**************************************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;

public class FileListCompareInfo
{
    public string mode;
    public FileListCompareData data;

    public void WriteXml(XmlDocument xmlDoc, XmlNode node)
    {
        if (node == null || data == null || xmlDoc == null) return;

        XmlNode child_node = null;

        int total_file_cnt = 0;
        uint total_file_size = 0;
        if (data.addList.Count > 0)
        {
            child_node = xmlDoc.CreateElement("new");
            CPathTree.WriteXml(xmlDoc, child_node, data.addList, ref total_file_size);
            node.AppendChild(child_node);
        }
        if (data.modifiyList.Count > 0)
        {
            child_node = xmlDoc.CreateElement("modify");
            CPathTree.WriteXml(xmlDoc, child_node, data.modifiyList, ref total_file_size);
            node.AppendChild(child_node);
        }
        if (data.deleteList.Count > 0)
        {
            uint total_file_size1 = 0;
            child_node = xmlDoc.CreateElement("delete");
            CPathTree.WriteXml(xmlDoc, child_node, data.deleteList, ref total_file_size1);
            node.AppendChild(child_node);
        }

        total_file_cnt = data.addList.Count + data.modifiyList.Count;
        XmlAttribute attributeNode = xmlDoc.CreateAttribute("total_file_cnt");
        attributeNode.Value = total_file_cnt.ToString();
        node.Attributes.Append(attributeNode);

        attributeNode = xmlDoc.CreateAttribute("total_file_size");
        attributeNode.Value = CPathTree.GetSizeDes(total_file_size);
        node.Attributes.Append(attributeNode);
    }
}

public class CPathTree
{
    public string path = null;
    public uint size = 0;
    public int total_file_cnt = 0;
    public uint total_file_size = 0;
    public List<BundleInfo> data_list = null;

    public Dictionary<string, CPathTree> subPathTree = new Dictionary<string, CPathTree>();

    public void InsertPathTree(BundleInfo bundleInfo)
    {
        if (bundleInfo == null) return;

        total_file_cnt++;
        total_file_size += bundleInfo.size;

        string[] path_arr = bundleInfo.bundleName.Split('/');
        if (path_arr == null || path_arr.Length < 2) return;

        // 构建目录树;
        CPathTree _iterator = this;
        CPathTree pathTree = null;
        for (int nIdx = 0; nIdx < path_arr.Length - 1; ++nIdx)
        {
            if (!_iterator.subPathTree.TryGetValue(path_arr[nIdx], out pathTree))
            {
                pathTree = new CPathTree();
                pathTree.path = path_arr[nIdx];
                _iterator.subPathTree[path_arr[nIdx]] = pathTree;
            }
            _iterator = pathTree;
        }

        if (pathTree != null)
        {
            pathTree.size += bundleInfo.size;
            if (pathTree.data_list == null)
                pathTree.data_list = new List<BundleInfo>();
            pathTree.data_list.Add(bundleInfo);
        }
    }

    public static void WriteXml(XmlDocument xmlDoc, XmlNode node, List<BundleInfo> list, ref uint total_file_size)
    {
        if (list == null || xmlDoc == null || node == null) return;

        CPathTree parent = new CPathTree();
        for (int nIdx = 0; nIdx < list.Count; ++nIdx)
        {
            total_file_size += list[nIdx].size;
            parent.InsertPathTree(list[nIdx]);
        }

        string path = "";
        WriteXml(xmlDoc, node, parent, path);
    }

    public static string GetSizeDes(uint size)
    {
        int KSize = 1024;
        int MSize = 1024 * 1024;
        if (size < KSize)
            return size + "B";

        if(size < MSize)
            return (1.0f * size / KSize).ToString("F2") + "K";

        return (1.0f * size / MSize).ToString("F2") + "M";
    }

    static void WriteXml(XmlDocument xmlDoc, XmlNode node, CPathTree pathTree, string path)
    {
        if (pathTree == null || xmlDoc == null || node == null) return;

        if(string.IsNullOrEmpty(path))
        {
            path = pathTree.path;
        }
        else
        {
            path = path + "_" + pathTree.path;
        }

        XmlNode path_node = node;

        // 写入文件信息;
        XmlNode child_node = null;
        XmlAttribute attributeNode = null;
        if (pathTree.total_file_cnt > 0)
        {
            attributeNode = xmlDoc.CreateAttribute("total_file_cnt");
            attributeNode.Value = pathTree.total_file_cnt.ToString();
            path_node.Attributes.Append(attributeNode);

            attributeNode = xmlDoc.CreateAttribute("total_file_size");
            attributeNode.Value = GetSizeDes(pathTree.total_file_size);
            path_node.Attributes.Append(attributeNode);
        }

        if (pathTree.data_list != null)
        {
            path_node = xmlDoc.CreateElement(path);
            node.AppendChild(path_node);
            node = path_node;

            attributeNode = xmlDoc.CreateAttribute("bundleSize");
            attributeNode.Value = (1.0f * pathTree.size / 1024).ToString("F2") + "KB";
            node.Attributes.Append(attributeNode);

            for (int nIdx = 0; nIdx < pathTree.data_list.Count; ++nIdx)
            {
                BundleInfo bundleInfo = pathTree.data_list[nIdx];
                child_node = xmlDoc.CreateElement("file");
                path_node.AppendChild(child_node);

                attributeNode = xmlDoc.CreateAttribute("bundleName");
                attributeNode.Value = bundleInfo.bundleName;
                child_node.Attributes.Append(attributeNode);

                attributeNode = xmlDoc.CreateAttribute("size");
                attributeNode.Value = (1.0f * bundleInfo.size / 1024).ToString("F2") + "KB";
                child_node.Attributes.Append(attributeNode);
            }
        }

        if (pathTree.subPathTree.Count == 0) return;

        foreach(KeyValuePair<string, CPathTree> pairs in pathTree.subPathTree)
        {
            WriteXml(xmlDoc, path_node, pairs.Value, path);
        }
    }
}

public class FileListCompareAll
{
    public string time;
    public string newFileList;
    public string oldFileList;

    public List<FileListCompareInfo> infoList = new List<FileListCompareInfo>();

    new public string ToString()
    {
        string info = "";

        foreach (FileListCompareInfo comInfo in infoList)
        {
            info = string.Format("{0}\n{1}\n{2}\n----------------------------", info, comInfo.mode, comInfo.data.ToString());
        }
        return info;
    }

    public void SaveXml(string filePath)
    {
        if(string.IsNullOrEmpty(filePath))
        {
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        XmlNode declareNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(declareNode);

        XmlNode root = xmlDoc.CreateElement("root");
        xmlDoc.AppendChild(root);

        foreach (FileListCompareInfo comInfo in infoList)
        {
            XmlNode child_node = xmlDoc.CreateElement(comInfo.mode);
            root.AppendChild(child_node);
            comInfo.WriteXml(xmlDoc, child_node);
        }

        if (File.Exists(filePath))
            File.Delete(filePath);
        xmlDoc.Save(filePath);
    }
}

public class FileListUtility
{
    private static FileList s_FileList = new FileList();

    static string last_newFileList_dir
    {
        get
        {
            return EditorPrefs.GetString("last_newFileList_dir", ".");
        }
        set
        {
            EditorPrefs.SetString("last_newFileList_dir", value);
        }
    }
    static string last_oldFileList_dir
    {
        get
        {
            return EditorPrefs.GetString("last_oldFileList_dir", ".");
        }
        set
        {
            EditorPrefs.SetString("last_oldFileList_dir", value);
        }
    }
    
    public static void BuildArtFileList()
    {
        MD5Utils.BuildAllArtsFileList();
    }    
    
    public static void CompareFileListTxt()
    {        
        string newPath = EditorUtility.OpenFilePanel("选择新版FileList", last_newFileList_dir, "");
        string oldPath = EditorUtility.OpenFilePanel("选择旧版FileList", last_oldFileList_dir, "");

        if (string.IsNullOrEmpty(newPath) || string.IsNullOrEmpty(oldPath))
        {
            return;
        }
        last_newFileList_dir = Path.GetDirectoryName(newPath);
        last_oldFileList_dir = Path.GetDirectoryName(oldPath);

        FileListCompareAll compareAll = Compare(newPath, oldPath);
        if ( compareAll == null )
        {
            EditorUtility.DisplayDialog("比较失败", "比较失败，详情看Log", "好的");
            return;
        }

        if (!EditorUtility.DisplayDialog("比较完成", string.Format("{0}\n{1}\n{2}", newPath, oldPath, compareAll.ToString()) , "保存", "好的"))
        {
            return;
        }

        string savePath = EditorUtility.SaveFilePanel("保存", "", "filelist_compare", "txt");
        if ( string.IsNullOrEmpty(savePath) )
        {
            return;
        }
        
        BuildCommon.WriteJsonToFile("", savePath, compareAll);
        Debug.LogFormat("保存");

//         return;
// 
//         FileList newFileList = BuildCommon.ReadJsonFromFile<FileList>(newPath);
//         FileList oldFileList = BuildCommon.ReadJsonFromFile<FileList>(oldPath);
// 
//         if (newFileList == null || oldFileList == null)
//         {
//             return;
//         }
// 
// 
//         FileListCompareData comData = FileList.Compare(newFileList, oldFileList);
//         Debug.LogErrorFormat("{0}, {1}, {2}", comData.addList.Count, comData.deleteList.Count, comData.modifiyList.Count);
    }
    
    public static void CompareFileListXml()
    {
        string newPath = EditorUtility.OpenFilePanel("选择新版FileList", last_newFileList_dir, "");
        string oldPath = EditorUtility.OpenFilePanel("选择旧版FileList", last_oldFileList_dir, "");

        if (string.IsNullOrEmpty(newPath) || string.IsNullOrEmpty(oldPath))
        {
            return;
        }
        last_newFileList_dir = Path.GetDirectoryName(newPath);
        last_oldFileList_dir = Path.GetDirectoryName(oldPath);

        FileListCompareAll compareAll = Compare(newPath, oldPath);
        if (compareAll == null)
        {
            EditorUtility.DisplayDialog("比较失败", "比较失败，详情看Log", "好的");
            return;
        }

        if (!EditorUtility.DisplayDialog("比较完成", string.Format("{0}\n{1}\n{2}", newPath, oldPath, compareAll.ToString()), "保存", "好的"))
        {
            return;
        }

        string savePath = EditorUtility.SaveFilePanel("保存", "", "filelist_compare", "xml");
        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }

        compareAll.SaveXml(savePath);
        Debug.LogFormat("保存");
    }

    public static FileListCompareAll Compare(string newPath, string oldPath)
    {
        if (string.IsNullOrEmpty(newPath) || string.IsNullOrEmpty(oldPath))
        {
            return null;
        }

        if ( !File.Exists(newPath) || !File.Exists(oldPath))
        {
            return null;
        }

        FileList newFileList = ReadFileList(newPath);
        FileList oldFileList = ReadFileList(oldPath);

        //FileList newFileList = BuildCommon.ReadJsonFromFile<FileList>(newPath);
        //FileList oldFileList = BuildCommon.ReadJsonFromFile<FileList>(oldPath);

        if ( newFileList == null )
        {
            Debug.LogErrorFormat("{0} Load Failed!", newPath);
            return null;
        }

        if (oldFileList == null)
        {
            Debug.LogErrorFormat("{0} Load Failed!", oldPath);
            return null;
        }

        FileListCompareAll compareall = new FileListCompareAll();
        compareall.time = System.DateTime.Now.ToString();
        compareall.newFileList = string.Format("{0} : {1}", newPath, MD5Utils.GetMD5(newPath));
        compareall.oldFileList = string.Format("{0} : {1}", oldPath, MD5Utils.GetMD5(oldPath));

        for ( int i=1; i< (int)BundleUpdateMode.Update_End; i++ )
        {
            BundleUpdateMode mode = (BundleUpdateMode)i;
            FileListCompareData comData = FileList.Compare(newFileList, oldFileList, true, mode);
            if ( comData == null )
            {
                continue;
            }

            FileListCompareInfo info = new FileListCompareInfo();
            info.mode = mode.ToString();
            info.data = comData;

            compareall.infoList.Add(info);
        }

        return compareall;
    }

    private static FileList ReadFileList(string path)
    {
        string fileName = System.IO.Path.GetFileName(path);
        if ( string.Equals(fileName, ResourceConst.FileListName) )
        {
            return PackBundleTools.GetAssetBundleFileListFromAbsPath(path);
        }

        if (string.Equals(fileName, ResourceConst.FileListName + ".txt"))
        {
            return BuildCommon.ReadJsonFromFile<FileList>(path);
        }

        return null;
    }

    public static void BuildFileList( bool show_dialog )
    {
        Clear();

        DateTime dt1 = System.DateTime.UtcNow;
        // 获取AssetBundleManifest;
        string manifestPath = string.Format("{0}/{1}", PackAssetBundle.bundleBuildFolder, ResourceConst.PkgBundleFolder);
        AssetBundleManifest manifest = PackBundleTools.GetAssetBundleManifest(manifestPath);
        if (manifest == null)
        {
            return;
        }

        // 计算总控文件;
        AddBundleInfo(ResourceConst.PkgBundleFolder, ref s_FileList);

        // 计算Config;
        AddConfInfo(s_FileList);

        // 计算其它文件;
        string[] allBundleKeyList = manifest.GetAllAssetBundles();
        for (int i = 0; i < allBundleKeyList.Length; i++)
        {
            string file = allBundleKeyList[i];
            if (string.IsNullOrEmpty(file))
            {
                continue;
            }
            AddBundleInfo(file, ref s_FileList);

            PackAssetBundleUtlis.ShowProgress(i, allBundleKeyList.Length, "计算MD5：", file);
        }
        EditorUtility.ClearProgressBar();

        string strFileList = string.Format("{0}.txt", ResourceConst.FileListName);
        DateTime dt2 = System.DateTime.UtcNow;
        BuildCommon.WriteJsonToFile(PackAssetBundle.bundleBuildFolder, strFileList, s_FileList);

        FileDepencies.WriteMd5Info();

        DateTime dt3 = System.DateTime.UtcNow;
        BuildFileListAssetBundle(PackAssetBundle.bundleBuildFolder, strFileList, ResourceConst.FileListName);

        DateTime dt4 = System.DateTime.UtcNow;

        if (show_dialog)
        {
            string info = string.Format("FileList生成完成，总时长{0}秒", (dt4 - dt1).TotalSeconds.ToString("f1"));
            EditorUtility.DisplayDialog("打包完成", info, "好的");
        }
    }

    public static bool BuildFileListByConf()
    {
        Clear();
        
        // 读取旧的FileList;
        string strFileList = string.Format("{0}.txt", ResourceConst.FileListName);
        string fileListPath = string.Format("{0}/{1}", PackAssetBundle.bundleBuildFolder, strFileList);
        FileList fileList = BuildCommon.ReadJsonFromFile<FileList>(fileListPath);
        if ( fileList == null )
        {
            Debug.LogErrorFormat("ReadJsonFromFile {0} Failed!", fileListPath);
            return false;
		}
		// 计算FileList;
		AddConfInfo(fileList);
		return BuildFileListBundle(fileList, strFileList, ResourceConst.FileListName);
    }

	public static bool BuildFileListBundle(FileList fileList, string fileName, string bundleName)
	{
		try
		{
			BuildCommon.WriteJsonToFile(PackAssetBundle.bundleBuildFolder, fileName, fileList);

			BuildFileListAssetBundle(PackAssetBundle.bundleBuildFolder, fileName, bundleName);
		}
		catch (System.Exception ex)
		{
			Debug.LogError(ex.ToString());
			return false;
		}
		return true;
	}

	private static void BuildFileListAssetBundle(string folder, string fileList, string bundleName)
    {
        if ( string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileList))
        {
            return;
        }

        string filePath = string.Format("{0}/{1}", folder, fileList);
        string fileName = System.IO.Path.GetFileName(filePath);

        string tempFilePath = string.Format("Assets/{0}", fileName);
        if ( File.Exists(tempFilePath) )
        {
            File.Delete(tempFilePath);
        }
        File.Copy(filePath, tempFilePath);        
        AssetDatabase.Refresh();

        UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(tempFilePath);        
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetNames = new System.String[] { tempFilePath };
        build.assetBundleName = bundleName;

        AssetBundleBuild[] builds = new AssetBundleBuild[] { build };

        string out_path = string.Format("{0}/temp_filelist", folder);
        if ( !Directory.Exists(out_path) )
        {
            Directory.CreateDirectory(out_path);
        }

        BuildPipeline.BuildAssetBundles(out_path, builds, BuildAssetBundleOptions.None, PackBundleTools.platform);

        // 拷贝资源;
        string new_bundlePath = string.Format("{0}/../{1}", out_path, bundleName);
        string old_bundlePath = string.Format("{0}/{1}", out_path, bundleName);
        if ( File.Exists(new_bundlePath) )
        {
            File.Delete(new_bundlePath);
        }
        File.Copy(old_bundlePath, new_bundlePath);

        // 删除无用的资源;
        Directory.Delete(out_path, true);
        AssetDatabase.DeleteAsset(tempFilePath);
    }    

    static void CompareStatisitics(FileListCompareData comData)
    {
        if ( comData == null )
        {
            return;
        }


    }    

    static void Clear()
    {
        s_FileList.Clear();
    }

	
	static void AddConfInfo(FileList filelist)
    {
        if ( filelist == null )
        {
            return;
        }

        string confFolder = string.Format("{0}/{1}", PackAssetBundle.bundleBuildFolder, ResourceConst.ConfFolder);
        string[] files = Directory.GetFiles(confFolder);
        foreach ( string file in files )
        {
            string fileName = Path.GetFileName(file);


            string bundleName = string.Format("{0}/{1}", ResourceConst.ConfFolder, fileName);

            uint crc = FileToCRC32.GetFileCRC32Int(file);

            FileInfo fileInfo = new FileInfo(file);
            UInt32 size = (UInt32)fileInfo.Length;
            string md5 = MD5Utils.GetMD5(file);

            filelist.AddBundleInfo(bundleName, crc, size, md5);
        }
    }

    static void AddBundleInfo(string bundleName, ref FileList filelist)
    {
        if ( string.IsNullOrEmpty(bundleName) )
        {
            return;
        }

        string bundlePath = string.Format("{0}/{1}", PackAssetBundle.bundleBuildFolder, bundleName);

        uint crc = 0;
        if (!BuildPipeline.GetCRCForAssetBundle(bundlePath, out crc))
        {
            return;
        }

        FileInfo fileInfo = new FileInfo(bundlePath);
        UInt32 size = (UInt32)fileInfo.Length;
        string md5 = "";
        if ( string.Equals( bundleName, ResourceConst.PkgBundleFolder) )
        {
            md5 = MD5Utils.GetMD5(bundlePath);
        }
        else
        {
            md5 = FileDepencies.GetBundleMD5(bundlePath);
        }

		filelist.AddBundleInfo(bundleName, crc, size, md5);
    }
}
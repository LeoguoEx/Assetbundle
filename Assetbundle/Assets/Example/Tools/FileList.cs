
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：FileList.cs;
	作	者：W_X;
	时	间：2017 - 08 - 18;
	注	释：;
**************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BundleUpdateMode
{
    Update_CRC = 1,     // 基于CRC判断更新;
    Update_MD5 = 2,    // 基于MD5判断更新;
    Update_CRCANDMD5 = 3,   // CRC和MD5都不一样才更新;
    Update_CRCORMD5 = 4,      // CRC和MD5有一个不一样就更新;

    Update_End,
}

public class BundleInfo
{
    public string bundleName;
    public UInt32 crc;
    public UInt32 size;
    public string md5;
    public bool stream = false;

	public static bool Equals(BundleUpdateMode mode, BundleInfo left, BundleInfo right)
    {
        if ( left == null || right == null )
        {
            return true;
        }

        switch(mode)
        {
            case BundleUpdateMode.Update_CRC:
                return left.crc == right.crc;

            case BundleUpdateMode.Update_MD5:
                return string.Equals(left.md5, right.md5);

            case BundleUpdateMode.Update_CRCANDMD5:
                return left.crc == right.crc || string.Equals(left.md5, right.md5);

            case BundleUpdateMode.Update_CRCORMD5:
                return left.crc == right.crc && string.Equals(left.md5, right.md5);
        }

        return true;
    }
}

public class FileListCompareData
{
    public List<BundleInfo> addList = new List<BundleInfo>();
    public List<BundleInfo> deleteList = new List<BundleInfo>();
    public List<BundleInfo> modifiyList = new List<BundleInfo>();

    public UInt32 add_size
    {
        get
        {
            UInt32 totalsize = 0;
            foreach (BundleInfo bundleInfo in addList)
            {
                totalsize += bundleInfo.size;
            }
            return totalsize;
        }
    }

    public UInt32 delete_size
    {
        get
        {
            UInt32 totalsize = 0;
            foreach (BundleInfo bundleInfo in deleteList)
            {
                totalsize += bundleInfo.size;
            }
            return totalsize;
        }
    }

    public UInt32 modifiy_size
    {
        get
        {
            UInt32 totalsize = 0;
            foreach (BundleInfo bundleInfo in modifiyList)
            {
                totalsize += bundleInfo.size;
            }
            return totalsize;
        }
    }

    new public string ToString()
    {
        string strupdate_size = Size2String(add_size + modifiy_size);
        string stradd_size = Size2String(add_size);
        string strmod_size = Size2String(modifiy_size);
        string strdel_size = Size2String(delete_size);

        string strInfo = string.Format("本地打包较上次需要更新{0}", strupdate_size);
        strInfo = string.Format("{0}\n其中：\n增加：{1}", strInfo, stradd_size);
        strInfo = string.Format("{0}\n改变：{1}", strInfo, strmod_size);
        strInfo = string.Format("{0}\n删除：{1}", strInfo, strdel_size);
        return strInfo;
    }

    public string strInfo
    {
        get
        {
            string strupdate_size = Size2String(add_size + modifiy_size);
            string stradd_size = Size2String(add_size);
            string strmod_size = Size2String(modifiy_size);
            string strdel_size = Size2String(delete_size);

            string strInfo = string.Format("本地打包较上次需要更新{0} （增加：{1}，改变：{2}，删除：{3}）",
                strupdate_size, stradd_size, strmod_size, strdel_size);
            return strInfo;
        }
    }

    public static string Size2String(uint size)
    {
        float scale = 1f / 1024f;

        float mb_size = size * scale * scale;

        return string.Format("{0}MB", mb_size.ToString("0.00"));
    }
}

public class FileListUtils
{
    public static BundleUpdateMode s_updateMode = BundleUpdateMode.Update_CRC;
}

public class FileList
{
    /// <summary>
    /// 此数据已经无用了，为了保证打包数据不变，请使用FileListUtils.s_updateMode; 
    /// </summary>
    public static BundleUpdateMode s_updateMode = BundleUpdateMode.Update_CRCANDMD5;

    public List<BundleInfo> m_FileList = new List<BundleInfo>();

    public void Clear()
    {
        m_FileList.Clear();
    }

    public BundleInfo GetBundleInfo(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            return null;
        }

        for (int i = 0; i < m_FileList.Count; i++)
        {
            if (string.Equals(bundleName, m_FileList[i].bundleName))
            {
                return m_FileList[i];
            }
        }

        return null;
    }

    public BundleInfo AddBundleInfo(string bundleName, UInt32 crc, UInt32 size, string md5)
    {
        BundleInfo bundleInfo = GetBundleInfo(bundleName);
        if (bundleInfo == null)
        {
            bundleInfo = new BundleInfo();
            bundleInfo.bundleName = bundleName;
            m_FileList.Add(bundleInfo);
        }

        bundleInfo.crc = crc;
        bundleInfo.size = size;
        bundleInfo.md5 = md5;
        return bundleInfo;
    }

    public static FileListCompareData Compare(FileList newFileList, FileList oldFileList, bool remove_old = true, BundleUpdateMode mode = BundleUpdateMode.Update_CRC)
    {
        FileListCompareData comData = new FileListCompareData();
        if (newFileList == null)
        {
            return comData;
        }

        if (oldFileList == null)
        {
            comData.addList.CopyFrom(newFileList.m_FileList);
            return comData;
        }

        for (int i = 0; i < newFileList.m_FileList.Count; i++)
        {
            BundleInfo bundleInfo = newFileList.m_FileList[i];
            if (bundleInfo == null || string.IsNullOrEmpty(bundleInfo.bundleName))
            {
                continue;
            }

            BundleInfo oldBundleInfo = oldFileList.GetBundleInfo(bundleInfo.bundleName);
            if (oldBundleInfo == null)
            {
                comData.addList.Add(bundleInfo);
                continue;
            }

            if ( BundleInfo.Equals(mode, oldBundleInfo, bundleInfo) )
            {
                continue;
            }

            //if (oldBundleInfo.crc == bundleInfo.crc)
            //{
            //    continue;
            //}

            comData.modifiyList.Add(bundleInfo);
        }

        if (remove_old)
        {
            return comData;
        }

        for (int i = 0; i < oldFileList.m_FileList.Count; i++)
        {
            BundleInfo oldBundleInfo = oldFileList.m_FileList[i];
            if (oldBundleInfo == null || string.IsNullOrEmpty(oldBundleInfo.bundleName))
            {
                continue;
            }

            BundleInfo bundleInfo = newFileList.GetBundleInfo(oldBundleInfo.bundleName);
            if (bundleInfo != null)
            {
                continue;
            }
            comData.deleteList.Add(bundleInfo);
        }

        return comData;
    }

	// 微端判断下载，只做一步检测，资源不在才更新，资源存在跟我没关系;
	public static void GetMiniDiff(ref List<BundleInfo> downLoadList, ref List<BundleInfo> removeList, FileList localListInfo, FileList serverListInfo)
	{
		if (downLoadList == null || serverListInfo == null)
		{
			return;
		}
		downLoadList.Clear();
		List<BundleInfo> localList = localListInfo.m_FileList;
		List<BundleInfo> serverList = serverListInfo.m_FileList;
		if (serverList == null)
		{
			return;
		}
		for (int index = 0; index < serverList.Count; index++)
		{
			BundleInfo serverDataInfo = serverList[index];
			if (serverDataInfo == null)
			{
				continue;
			}

			BundleInfo localDataInfo = GetBundleInfo(serverDataInfo.bundleName, localList);

			// 如果本地FileList没有记录这个文件;
			if (localDataInfo == null)
			{
				downLoadList.Add(serverDataInfo);
				continue;
			}

#if !UNITY_EDITOR
				bool find = false;
				if (localDataInfo.stream)
				{
					find = FindStreamFile(serverDataInfo.bundleName);
				}
				else
				{
					find = FindLocalFile(serverDataInfo.bundleName);
				}

				// 如果列表记录了，但是本地又没有，只能再次下载;
				if (!find)
				{
					downLoadList.Add(serverDataInfo);
					continue;
				}
#else
			bool find = FindLocalFile(serverDataInfo.bundleName);
			if (!find)
			{
				downLoadList.Add(serverDataInfo);
				continue;
			}
#endif
		}
	}

	public static void GetDiff(ref List<BundleInfo> downLoadList, ref List<BundleInfo> removeList, FileList localListInfo, FileList serverListInfo)
    {
        if (downLoadList == null || removeList == null || localListInfo == null || serverListInfo == null)
        {
            return;
        }

        downLoadList.Clear();
        removeList.Clear();

        List<BundleInfo> localList = localListInfo.m_FileList;
        List<BundleInfo> serverList = serverListInfo.m_FileList;
        if (serverList == null)
        {
            return;
        }
        // 查找需要下载的资源;
        for (int index = 0; index < serverList.Count; index++)
        {
            BundleInfo serverDataInfo = serverList[index];
            if (serverDataInfo == null)
            {
                continue;
            }

            BundleInfo localDataInfo = GetBundleInfo(serverDataInfo.bundleName, localList);

            // 如果本地FileList没有记录这个文件;
            if (localDataInfo == null)
            {
                downLoadList.Add(serverDataInfo);
                continue;
            }            

            // 如果服务器上的比本地的要新，则需要重新下载;
            if (!BundleInfo.Equals(FileListUtils.s_updateMode, serverDataInfo, localDataInfo))
            {
                downLoadList.Add(serverDataInfo);
                continue;
            }

            //Debug.LogFormat("{5} s_updateMode = {0}, serverDataInfo = {1} - {2}, localDataInfo = {3} - {4}", s_updateMode, serverDataInfo.crc, serverDataInfo.md5, localDataInfo.crc, localDataInfo.md5, serverDataInfo.bundleName);

            //if (serverDataInfo.crc != localDataInfo.crc)
            //{
            //    downLoadList.Add(serverDataInfo);
            //    continue;
            //}

#if !UNITY_EDITOR
				bool find = false;
				if (localDataInfo.stream)
				{
					find = FindStreamFile(serverDataInfo.bundleName);
				}
				else
				{
					find = FindLocalFile(serverDataInfo.bundleName);
				}

				// 如果列表记录了，但是本地又没有，只能再次下载;
				if (!find)
				{
					downLoadList.Add(serverDataInfo);
					continue;
				}
#else
            bool find = FindLocalFile(serverDataInfo.bundleName);
            if (!find)
            {
                downLoadList.Add(serverDataInfo);
                continue;
            }
#endif
        }

        if (localList == null)
        {
            return;
        }
        // 查找需要删除的资源;
        for (int index = 0; index < localList.Count; index++)
        {
            BundleInfo localDataInfo = localList[index];
            BundleInfo serverDataInfo = GetBundleInfo(localDataInfo.bundleName, serverList);

            // 如果本地没有这个文件;
            if (serverDataInfo == null)
            {
                removeList.Add(localDataInfo);
                continue;
            }

        }

    }

    public static bool FindLocalFile(string dataPath)
    {
        string localPath = string.Format("{0}/{1}", Application.persistentDataPath, dataPath);
        bool find = System.IO.File.Exists(localPath);
        return find;
    }

    public static bool ResetAllFileFlag(ref FileList listInfo, bool isStreamFile)
    {
        return true;
    }

    public static bool GetStreamingAssetsDiff(ref List<BundleInfo> updateLoadList, ref FileList localListInfo, FileList streamListInfo)
    {
        if (updateLoadList == null || streamListInfo == null)
        {
            return true;
        }

        if (localListInfo == null)
        {
            localListInfo = new FileList();
        }

        // 是否是mini;
        bool isMiniClient = false;
        updateLoadList.Clear();

        List<BundleInfo> localList = localListInfo.m_FileList;
        List<BundleInfo> serverList = streamListInfo.m_FileList;
        if (serverList == null)
        {
            return true;
        }

        // 查找需要下载的资源;
        for (int index = 0; index < serverList.Count; index++)
        {
            BundleInfo streamDataInfo = serverList[index];
            if (streamDataInfo == null)
            {
                continue;
            }
            BundleInfo localDataInfo = GetBundleInfo(streamDataInfo.bundleName, localList);

            // 在安装包中 ;
            if (streamDataInfo.bundleName.EndsWith(".dat"))
            {
                streamDataInfo.stream = false;
            }
            else
            {
                streamDataInfo.stream = true;
            }

            bool find = FindStreamFile(streamDataInfo.bundleName);
            if (!find)
            {
                // 安装包里没有有文件;
                if (localDataInfo != null)
                {
                    localDataInfo.stream = false;
                }
                isMiniClient = true;
                continue;
            }
            updateLoadList.Add(streamDataInfo);
        }

        return isMiniClient;
    }

    protected static BundleInfo GetBundleInfo(string path, List<BundleInfo> dataList)
    {
        if (dataList == null)
        {
            return null;
        }
        for (int i = 0; i < dataList.Count; i++)
        {
            BundleInfo dataInfo = dataList[i];
            if (string.Equals(dataInfo.bundleName, path) == true)
            {
                return dataInfo;
            }
        }

        return null;
    }

    /// <summary>
    /// 查找安装包里的系统文件 ;
    /// </summary>
    /// <param name="dataPath"></param>
    /// <returns></returns>
    public static bool FindStreamFile(string dataPath)
    {
        dataPath = string.Format("{0}/{1}", ResourceConst.PkgBundleFolder, dataPath);
        bool find = false;
#if UNITY_ANDROID && !UNITY_EDITOR
		if (PluginTool.instance.AssetFileExist(dataPath))
#elif UNITY_IPHONE && !UNITY_EDITOR
		if (System.IO.File.Exists(string.Format("{0}/Raw/{1}", Application.dataPath, dataPath)))
#endif
        {
            find = true;
        }
        return find;
    }

    public static bool FindStreamFileEx(string dataPath)
    {
        bool find = false;
#if UNITY_ANDROID && !UNITY_EDITOR
		if (PluginTool.instance.AssetFileExist(dataPath))
        {
            find = true;
        }
#elif UNITY_IPHONE && !UNITY_EDITOR
		if (System.IO.File.Exists(string.Format("{0}/Raw/{1}", Application.dataPath, dataPath)))
        {
            find = true;
        }
#endif
        return find;
    }

    public static bool CheckAllRes(FileList fileList, string localBundlePath)
    {
        if (fileList == null || string.IsNullOrEmpty(localBundlePath))
        {
            return false;
        }

        bool find = true;
        // 本地没有安装包里有;
        List<BundleInfo> localList = fileList.m_FileList;

        // 查找需要下载的资源;
        for (int index = 0; index < localList.Count; index++)
        {
            BundleInfo localDataInfo = localList[index];
            if (localDataInfo == null)
            {
                continue;
            }

            if (localDataInfo.stream)
            {
                continue;
            }
            else
            {
                find = System.IO.File.Exists(string.Format("{0}/{1}", localBundlePath, localDataInfo.bundleName));
            }

            // 如果列表记录了，但是本地又没有，只能再次下载;
            if (!find)
            {
                return false;
            }
        }
        return true;
    }

	public static UInt64 GetListSize(List<BundleInfo> dataList)
	{
		if (dataList == null || dataList.Count < 1)
		{
			return 0;
		}

		UInt64 size = 0;

		for (int i = 0; i < dataList.Count; i++)
		{
			BundleInfo dataInfo = dataList[i];
			if (dataInfo == null)
			{
				continue;
			}

			size += dataList[i].size;
		}

		return size;
	}
}

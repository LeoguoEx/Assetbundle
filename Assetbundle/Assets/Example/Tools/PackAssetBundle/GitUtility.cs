
/**************************************************************************************************
	Copyright (C) 2017 - All Rights Reserved.
--------------------------------------------------------------------------------------------------------
	当前版本：1.0;
	文	件：CodeFile1.cs;
	作	者：W_X;
	时	间：2017 - 11 - 29;
	注	释：;
**************************************************************************************************/

using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;
using System.Text;
using System.IO;

public class GitUtility
{
    private static StringBuilder s_sb = new StringBuilder();
    //private static Encoding outEncoding = Encoding.GetEncoding("gb2312");
    private static Encoding outEncoding = Encoding.Default;

	private static string Git
	{
		get
		{
			string gitPath = "";
#if UNITY_IPHONE
			gitPath = "git";
#else
			string sPath = System.Environment.GetEnvironmentVariable("Path");

			var result = sPath.Split(';');
			for (int i = 0; i < result.Length; i++)
			{
			    if (result[i].Contains(@"\Git\bin"))
			    {
			        gitPath = result[i];
			        break;
			    }
			}
			if (string.IsNullOrEmpty(gitPath) == false)
			{
			    gitPath = System.IO.Path.Combine(gitPath, "git.exe");
			}
#endif

			return gitPath;
		}
	}

    private static string develop_path
    {
        get
        {
            return System.IO.Path.GetFullPath(string.Format("{0}/../", Application.dataPath));
        }
    }

	private static string data_path
	{
		get
		{
			return PackAssetBundle.bundleBuildFolder + "/";
		}
	}

    private static string code_path
    {
        get
        {
            return System.IO.Path.GetFullPath(string.Format("{0}/client-code/", Application.dataPath));
        }
    }

    private static string conf_path
    {
        get
        {
#if UNITY_IPHONE
			/*
	        if (!EditorUtility.DisplayDialog("注意", "确认conf和develop平级目录！", "我确认", "不确定，我去看看"))
	        {
	            return string.Empty;
	        } 
	        */
        	string srcConfFolder = string.Format("{0}/../../conf/", Application.dataPath);
#else
            string srcConfFolder = @"T://";
#endif

            return srcConfFolder;
        }
    }

    private static string common_path
    {
        get
        {
            return System.IO.Path.GetFullPath(string.Format("{0}/client-common/", Application.dataPath));
        }
    }

    public static void GetSingleGitInfo(string gitPath, string title, string working)
    {
        s_sb.AppendLine(title);
        GetGitInfo(gitPath, working, "log --pretty=format:\"commit %H\" -1");
        GetGitInfo(gitPath, working, "log --pretty=format:\"Author: %an <%ae>\" -1");
        GetGitInfo(gitPath, working, "log --pretty=format:\"Date: %cd\" -1");
        s_sb.AppendLine("---------------------------------------------------------------------------------------------------------");
        GetGitInfo(gitPath, working, "status");
        s_sb.AppendLine("");
        s_sb.AppendLine("");
        s_sb.AppendLine("");
    }

    // 获取上次更新到本次更新间的详细内容修改
    public static void GetDetailGitInfo(string gitPath, string title, string working, string last_time)
    {
        //last_time = "2018-01-23 10:45:17";
        string time_now = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        s_sb.AppendLine(title + "@" + time_now);
        string arg = string.Format("log --since=\"{0}\" --pretty=format:\"%s,%an\" --no-merges", last_time);
        GetGitInfo(gitPath, working, arg);
        Debug.Log(arg);
        s_sb.AppendLine("---------------------------------------------------------------------------------------------------------");
        s_sb.AppendLine("");
        s_sb.AppendLine("");
        s_sb.AppendLine("");
    }    

    public static void GetGitInfo(string path, string workingDirectory, string arguments)
    {
		string gitPath = Git;
		if (string.IsNullOrEmpty(gitPath))
		{
			return;
		}

        Process p = new Process();
        p.StartInfo.FileName = gitPath;
        p.StartInfo.Arguments = arguments;
        p.StartInfo.WorkingDirectory = workingDirectory;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.StandardOutputEncoding = outEncoding;
        p.StartInfo.RedirectStandardOutput = true;
        p.OutputDataReceived += OnOutputDataReceived;
        p.Start();
        p.BeginOutputReadLine();
        p.WaitForExit();

    }

    private static void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e != null && !string.IsNullOrEmpty(e.Data))
        {
            s_sb.AppendLine(e.Data);
            //Debug.Log(info);
            //byte[] bytes = Encoding.Default.GetBytes(e.Data);
            //byte[] bytes_new = Encoding.Convert(Encoding.Default, Encoding.UTF8, bytes);
            //Debug.Log(Encoding.UTF8.GetString(bytes_new));
        }
    }

	[MenuItem("Test/输出Git信息到Data")]
	public static void PrintGitToData()
	{
		s_sb.Length = 0;
		s_sb.AppendLine(string.Format("Time = {0}", System.DateTime.Now.ToString()));

		string gitPath = Git;
		if (string.IsNullOrEmpty(gitPath))
		{
			return;
		}

		GetSingleGitInfo(gitPath, "develop:", develop_path);
        GetSingleGitInfo(gitPath, "conf:", conf_path);
        s_sb.AppendLine("");
        s_sb.AppendLine("");
        s_sb.AppendLine("");
        s_sb.AppendLine("************************************************************");
        s_sb.AppendLine("Following info is not important, can be igored");
        s_sb.AppendLine("************************************************************");
		GetSingleGitInfo(gitPath, "client-code:", code_path);
		GetSingleGitInfo(gitPath, "client-common:", common_path);
		

		byte[] bytes = outEncoding.GetBytes(s_sb.ToString());
		string info = System.Text.Encoding.UTF8.GetString(bytes);

		string out_path = string.Format("{0}/gitlog.txt", PackAssetBundle.bundleBuildFolder);
		FileStream fs = File.Open(out_path, FileMode.Create);
		StreamWriter writer = new StreamWriter(fs);
		writer.Write(info);
		writer.Flush();
		writer.Close();
		fs.Close();
	}

	[MenuItem("Test/输出Git信息到StreamAssets")]
	public static void PrintGitToStreamAssets()
	{
		s_sb.Length = 0;
		s_sb.AppendLine(string.Format("Time = {0}", System.DateTime.Now.ToString()));

		string gitPath = Git;
		if (string.IsNullOrEmpty(gitPath))
		{
			return;
		}

		GetSingleGitInfo(Git, "develop:", develop_path);
		GetSingleGitInfo(Git, ResourceConst.PkgBundleFolder, data_path);
		GetSingleGitInfo(Git, "client-code:", code_path);
		GetSingleGitInfo(Git, "client-common:", common_path);
		GetSingleGitInfo(Git, "conf:", conf_path);

		byte[] bytes = outEncoding.GetBytes(s_sb.ToString());
		string info = System.Text.Encoding.UTF8.GetString(bytes);

		string out_path = string.Format("{0}/gitlog.txt", Application.streamingAssetsPath);
		FileStream fs = File.Open(out_path, FileMode.Create);
		StreamWriter writer = new StreamWriter(fs);
		writer.Write(info);
		writer.Flush();
		writer.Close();
		fs.Close();
	}

    //[MenuItem("Test/输出Git详细差异内容到Data")]
    // 输出的文本有乱码，这个问题暂时解决不了，不能使用
    public static void PrintGitModifyDetailToData()
    {
        s_sb.Length = 0;
        s_sb.AppendLine(string.Format("Time = {0}", System.DateTime.Now.ToString()));

        string gitPath = Git;
        if (string.IsNullOrEmpty(gitPath))
        {
            return;
        }

        string develop_time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string conf_time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string in_path = string.Format("{0}/log.txt", PackAssetBundle.bundleBuildFolder);
        //FileStream in_fs = File.Open(in_path, FileMode.OpenOrCreate);
        //StreamReader reader = new StreamReader(in_fs);
        //string line_str = reader.ReadLine();
        //while(line_str != null)
        //{
        //    if (line_str.StartsWith("##develop"))
        //    {
        //        develop_time = line_str.Split('@')[1];
        //    }
        //    else if(line_str.StartsWith("##conf"))
        //    {
        //        conf_time = line_str.Split('@')[1];
        //    }
        //    line_str = reader.ReadLine();
        //}
        //in_fs.Close();


        GetDetailGitInfo(gitPath, "##develop", develop_path, develop_time);
        GetDetailGitInfo(gitPath, "##conf", conf_path, conf_time);

        byte[] bytes = outEncoding.GetBytes(s_sb.ToString());
        string info = System.Text.Encoding.UTF8.GetString(bytes);
        //Debug.Log(s_sb.ToString());
        //Debug.Log(info);
        
        string out_path = string.Format("{0}/log.txt", PackAssetBundle.bundleBuildFolder);
        FileStream fs = File.Open(out_path, FileMode.Create);
        StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);
        writer.Write(info);
        writer.Flush();
        writer.Close();
        //byte[] wData = new UTF8Encoding().GetBytes("#!/bin/sh\n");
        //fs.Write(wData, 0, wData.Length);
        //wData = new UTF8Encoding().GetBytes(s_sb.ToString());
        //fs.Write(wData, 0, wData.Length);
        ////fs.Write(bytes, 0, bytes.Length);
        //fs.Flush();
        fs.Close();
    }

    // 将Data资源包的修改提交到Git，日志使用gitlog.txt中的文本
     public static void GitCommit()
    {
        string log_path = string.Format("{0}/gitlog.txt", PackAssetBundle.bundleBuildFolder);
        FileStream file = new FileStream(log_path, FileMode.Open);
        int byteCount = (int)file.Length;
        byte[] byData = new byte[byteCount];
        file.Seek(0, SeekOrigin.Begin);
        file.Read(byData, 0, byteCount);

        string log = Encoding.UTF8.GetString(byData);
        //Debug.Log(log_path);
        //Debug.Log(log);

        string curDir = System.Environment.CurrentDirectory;
        string commit_path = string.Format("{0}/../commit_data.sh", curDir);
        FileStream wFile = new FileStream(commit_path, FileMode.Create);
        byte[] wData = new UTF8Encoding().GetBytes("#!/bin/sh\n");
        wFile.Write(wData, 0, wData.Length);
        string cdCmd = string.Format("cd \"{0}/{1}\"\n", curDir, ResourceConst.PkgBundleFolder);
        wData = new UTF8Encoding().GetBytes(cdCmd);
        wFile.Write(wData, 0, wData.Length);
        string logParse = log.Replace("\"", "*");
        //Debug.Log(logParse);
        wData = new UTF8Encoding().GetBytes("git add -A\n");
        wFile.Write(wData, 0, wData.Length);
        wData = new UTF8Encoding().GetBytes("git commit -m\"" + logParse + "\"\n");
        wFile.Write(wData, 0, wData.Length);
        wData = new UTF8Encoding().GetBytes("git pull\n");
        wFile.Write(wData, 0, wData.Length);
        wData = new UTF8Encoding().GetBytes("git push\n");
        wFile.Write(wData, 0, wData.Length);


        wFile.Flush();
        wFile.Close();
        file.Close();
        
#if UNITY_IPHONE
        System.Diagnostics.Process.Start("sh", commit_path);
#else
        System.Diagnostics.Process.Start(commit_path);
#endif

        string conf_dir = "T:/";
        string conf_commit = string.Format("{0}/commit.sh", conf_dir);
        FileStream conf_file = new FileStream(conf_commit, FileMode.Create);
        byte[] conf_data = new UTF8Encoding().GetBytes("#!/bin/sh\n");
        conf_file.Write(conf_data, 0, conf_data.Length);
        string conf_cmd = string.Format("cd \"{0}\"\n", conf_dir);
        conf_data = new UTF8Encoding().GetBytes(conf_cmd);
        conf_file.Write(conf_data, 0, conf_data.Length);
        conf_data = new UTF8Encoding().GetBytes("git add -A\n");
        conf_file.Write(conf_data, 0, conf_data.Length);
        conf_data = new UTF8Encoding().GetBytes("git commit -m\"" + logParse + "\"\n");
        conf_file.Write(conf_data, 0, conf_data.Length);
        conf_data = new UTF8Encoding().GetBytes("git pull\n");
        conf_file.Write(conf_data, 0, conf_data.Length);
        conf_data = new UTF8Encoding().GetBytes("git push\n");
        conf_file.Write(conf_data, 0, conf_data.Length);


        conf_file.Flush();
        conf_file.Close();

#if UNITY_IPHONE
        System.Diagnostics.Process.Start("sh", conf_commit);
#else
        System.Diagnostics.Process.Start(conf_commit);
#endif
    }

     // 更新打包环境（develop、conf、client-code、client-common）
     public static void GitPull()
     {
         //if (EditorUtility.DisplayDialog("更新提示", "是否需要更新打包环境", "更新", "跳过"))
         //{
             string curDir = System.Environment.CurrentDirectory;
             string pull_path = string.Format("{0}/build_pull.sh", curDir);
             Debug.Log(pull_path);
             var proc = System.Diagnostics.Process.Start(pull_path);
             proc.WaitForExit();
         //}
     }
}

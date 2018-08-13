using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// copy files or directorys
/// </summary>
public class FileUtils
{

    public static void CopyDirectory(string srcdir, string destdir)
    {
        if (string.IsNullOrEmpty(srcdir) || string.IsNullOrEmpty(destdir))
        {
            return;
        }
        string[] entries = System.IO.Directory.GetFileSystemEntries(srcdir);
        foreach (string entry in entries)
        {
            string filename = System.IO.Path.Combine(destdir, System.IO.Path.GetFileName(entry));
            if (Directory.Exists(entry))
            {
                if (entry.ToLower().Contains(".git"))
                    continue;
                CopyDirectory(entry, filename);
            }
            else
            {
                //string suffix = System.IO.Path.GetExtension(entry);

                string directory = Path.GetDirectoryName(filename);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                System.IO.File.Copy(entry, filename, true);
            }
        }
    }

    public static void ClearDirectory(string dir)
    {
        if (Directory.Exists(dir))
        {
            string[] entries = System.IO.Directory.GetFileSystemEntries(dir);
            foreach (string entry in entries)
            {

                if (Directory.Exists(entry))
                {
                    Directory.Delete(entry, true);
                }
                else
                {
                    File.Delete(entry);
                }
            }
        }
        else
        {
            Directory.CreateDirectory(dir);
        }

    }
}

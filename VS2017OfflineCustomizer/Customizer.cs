﻿using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Collections.Generic;

namespace VS2017OfflineCustomizer
{
    class Customizer
    {
        private String CurrentPath;
        private String[] Paths;
        private WebClient webby;
        private int i, EdID;
        public List<String> SelLang = new List<String>(), SelWorkload = new List<String>();

        public Customizer(String CurrentPath)
        {
            Paths = new String[DataContainer.GetData("files").GetLength(0)];
            webby = new WebClient();
            this.CurrentPath = CurrentPath + "\\" + DataContainer.GetFolderName();
            CreatePaths();
        }

        public Boolean PreInit()
        {
            if (!(File.Exists(Paths[0]) && File.Exists(Paths[1]) && File.Exists(Paths[2])))
            {
                try
                {
                    if (!Directory.Exists(CurrentPath))
                    {
                        Directory.CreateDirectory(CurrentPath);
                    }
                    MessageBox.Show("VS files aren't here. I will download the latest .exe version avaible.\nI will freeze for 1-2 minutes,\nSorry.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    GetLatestFile();
                    DownloadExes();
                    MessageBox.Show("Download Completed.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                catch (Exception ex)
                {
                    Log(ex);
                    if(ex is UnauthorizedAccessException || ex is IOException)
                    {
                        MessageBox.Show("Access Denied. Retry running as Admin", "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (ex is WebException)
                    {
                        MessageBox.Show("Download Error. Check Network", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                    
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        private void Log(Exception ex)
        {
            File.WriteAllText(CurrentPath + "\\log.txt", ex.ToString());
        }

        private void GetLatestFile()
        {
            for(i= 0; i<DataContainer.GetData("files").GetLength(1); i++)
            {
                string html = webby.DownloadString(DataContainer.GetData("files")[i,1]);
                int start = html.LastIndexOf("downloadUrl = \"") + 15;
                int end = html.IndexOf("\";<") - start;
                DataContainer.GetData("files")[i,1] = html.Substring(start, end);
            }
        }

        public void AddRemoveID(String ID, List<String> List)
        {
            Boolean remove = false;
            foreach (String c in List)
            {
                if (c.Equals(ID))
                {
                    remove = true;
                    break;
                }
            }
            if (remove)
            {
                List.Remove(ID);
            }
            else
            {
                List.Add(ID);
            }
        }

        private void CreatePaths()
        {
            for(i = 0; i< DataContainer.GetData("files").GetLength(0); i++)
            {
                Paths[i] = CurrentPath + "\\" + DataContainer.GetData("files")[i, 0];
            }
        }

        private void DownloadExes()
        {
            for(i = 0; i< DataContainer.GetData("files").GetLength(0); i++)
            {
                webby.DownloadFile(DataContainer.GetData("files")[i, 1], Paths[i]);
            }
        }

        public String SelectEdition(String edition)
        {
            switch (edition)
            {
                case "Community":
                    EdID = 0;
                    return "Community";
                case "Professional":
                    EdID = 1;
                    return "Professional";
                case "Enterprise":
                    EdID = 2;
                    return "Enterprise";
                default:
                    return null;
            }
        }

        public void CheckForUpdate(Boolean visual, String currver)
        {
            String ver = "";
            try
            {
                ver = webby.DownloadString(DataContainer.GetVersionOnline());
                if (!ver.Equals(currver))
                {
                    if(MessageBox.Show("There is a update avaible!\nDo you want to open the topic?", "Check for Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(DataContainer.GetMyDigitalTopic());
                    }
                }
                else
                {
                    if (visual)
                    {
                        MessageBox.Show("You are using the latest version.", "Check for Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (WebException wex)
            {
                Log(wex);
                if(visual)
                {
                    MessageBox.Show("Network unavaible.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public String GetArgs(String saveto)
        {
            String args = "--layout \"" + saveto + "\"";
            if (SelWorkload.Count > 0 && SelWorkload.Count < 17)
            {
                String workarg = " --add";
                foreach (String s in SelWorkload)
                {
                    workarg = workarg + " " + DataContainer.GetWorkload_prefix() + s;
                }
                args = args + workarg;
            }
            if (SelLang.Count > 0 && SelLang.Count <14)
            {
                String langarg = " --lang";
                foreach (String s in SelLang)
                {
                    langarg = langarg + " " + s;
                }
                args = args + langarg;
            }

            return args;
        }

        public int GetID()
        {
            return EdID;
        }

        public String[] GetPaths()
        {
            return Paths;
        }
    }
}

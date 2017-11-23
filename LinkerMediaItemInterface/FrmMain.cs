using LinkerMediaItemInterface.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Collections;

namespace LinkerMediaItemInterface
{
    public partial class FrmMain : Form
    {
        private Dart.PowerTCP.Ftp.Ftp ftp1;
        private Dart.PowerTCP.Ftp.Ftp ftp2;
        bool isStart;
        public bool IsStart
        {
            get
            {
                return isStart;
            }

            set
            {
                isStart = value;
                isRepeat = true;
                if (isStart)
                {
                    this.btnStart.Image = global::LinkerMediaItemInterface.Properties.Resources.stop;

                    this.btnStart.Text = "停止";
                }
                else
                {
                    this.btnStart.Image = global::LinkerMediaItemInterface.Properties.Resources.Start;

                    this.btnStart.Text = "启动";
                }
            }
        }

        Thread ManagerThread;
        Thread ExcThread;
        public FrmMain()
        {
            InitializeComponent();
            ManagerThread = new Thread(new ThreadStart(this.ThreadManager));
            ManagerThread.IsBackground = true;
            ManagerThread.Start();
            IsStart = true;
        }

        private void Init()
        {
            try
            {
                if (!Directory.Exists(Globals.DefaultDir + "\\output"))
                {
                    Directory.CreateDirectory(Globals.DefaultDir + "\\output");
                }

            }
            catch (Exception ex)
            {

                LogService.WriteErr("系统错误，方法：Init() 信息：" + ex.Message);
            }
        }
        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="pContent"></param>
        private void AppendMessageLine(string pContent)
        {
            this.txtInfo.AppendText(DateTime.Now + ":  " + pContent + "\n");
        }


        /// <summary>
        /// 管理线程
        /// </summary>
        private void ThreadManager()
        {
            try
            {
                while (true)
                {
                    Application.DoEvents();

                    if (this.isStart)
                    {
                        if (ExcThread != null)
                        {

                            if (!ExcThread.IsAlive)
                            {
                                TimeSpan span = DateTime.Now - Globals.ExecuteStopDate;

                                if (span.Seconds >= Globals.RequestInterval)
                                {
                                    ExcThread = new Thread(new ThreadStart(this.TaskFunc));

                                    ExcThread.IsBackground = true;
                                    ExcThread.Start();
                                }


                            }

                        }
                        else
                        {
                            ExcThread = new Thread(new ThreadStart(this.TaskFunc));
                            ExcThread.IsBackground = true;
                            ExcThread.Start();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //Globals.WriteLocalLog("程序错误，方法：ThreadManager 错误信息：" + ex.Message);
            }
        }

        #region //任务执行

        private void TaskFunc()
        {
            try
            {
                if (Globals.BatchImport == 1)
                {
                    this.Invoke(new Tasks(this.BatchTask));
                }
                else
                {
                    this.Invoke(new Tasks(this.Task));
                }
            }
            catch { }
        }

        private delegate void Tasks();
        private bool isRepeat = true;
        private void Task()
        {
            try
            {
                if (txtInfo.Text.Length > txtInfo.MaxLength)
                    txtInfo.Text = "";
                Application.DoEvents();

                DataTable taskDataTable = new DataTable();
                using (SqlConnection conn = new SqlConnection(Globals.X1ConnectionStr))
                {
                    string sql = "select top 5 * from ExportTasks where TaskStatus<>-1 order by ChannelID desc, TaskDateTime desc";
                    using (SqlDataAdapter sd = new SqlDataAdapter(sql, conn))
                    {
                        sd.Fill(taskDataTable);
                    }
                }
                if (taskDataTable == null || taskDataTable.Rows.Count <= 0)
                {
                    if (isRepeat)
                    {
                        AppendMessageLine("未获取到新任务！");
                        LogService.Write("未获取到新任务！");
                    }
                    isRepeat = false;
                    return;
                }
                isRepeat = true;
                AppendMessageLine("获取到" + taskDataTable.Rows.Count + "条新任务！");
                LogService.Write("获取到" + taskDataTable.Rows.Count + "条新任务！");
                foreach (DataRow item in taskDataTable.Rows)
                {
                    try
                    {
                        Application.DoEvents();
                        if (item["ChannelID"].ToString() == "-1")
                        {
                            this.doProgram(item);
                        }
                        else
                        {
                            this.doItem(item);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.UpdateTaskStatus(item["ProgramID"].ToString(), -1);
                        AppendMessageLine("导出任务失败 节目ID：" + item["ProgramID"].ToString() + " 错误信息：" + ex.Message);
                        LogService.WriteErr("导出任务失败 节目ID：" + item["ProgramID"].ToString() + " 错误信息：" + ex.Message);
                        continue;
                    }

                }
                if (!isStart)
                {
                    AppendMessageLine("任务停止！");
                    LogService.Write("任务停止！");
                    return;
                }

            }
            catch (Exception ex)
            {
                AppendMessageLine("程序错误，方法：Task 错误信息：" + ex.Message);
                LogService.WriteErr("程序错误，方法：Task 错误信息：" + ex.Message);
            }
            finally
            {
                Globals.ExecuteStopDate = DateTime.Now;
                Common.FlushMemory();
            }
        }

        #region //任务导出
        private void doItem(DataRow item)
        {
            INIClass ini = new INIClass(Application.StartupPath + "\\config.ini");
            string air5Str = ini.IniReadValue("conn", "air5_" + item["ChannelID"].ToString());
            if (air5Str.Trim().Length == 0)
            {
                return;
            }
            using (SqlConnection conn = new SqlConnection(air5Str))
            {
                string sql = "select (select top 1 CategoryName from Categories where CategoryID=items.Artist1 " +
                             "or CategoryID=items.Artist3) as CreatorName,(select top 1 CategoryName from Categories " +
                             "inner join Stacks on Categories.CategoryID=Stacks.StackID where Stacks.ItemID=items.ItemID) as ClassName," +
                             "(select top 1 StationID from Stations where StationNumber = " + item["ChannelID"].ToString() + " ) as StationID," +
                            "(select top 1 StationName from Stations where StationNumber = " + item["ChannelID"].ToString() + " ) as StationName," +
                            item["ChannelID"].ToString() + "as ChannelID," +
                             " * from items inner join StorageZone on items.SoundStorageID = StorageZone.StorageID " +
                             "and StorageZone.ZoneID=" + Globals.Zone + " where ItemID='" + item["ProgramID"] + "'";

                using (SqlDataAdapter sd = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    sd.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        Application.DoEvents();
                        string errorInfo = "";
                        //TreeListNode node = this.treeList_Export.AppendNode(new object[] { row["Title"], "获取到任务", DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"), "" }, null, null);
                        AppendMessageLine("任务获取成功，名称：" + row["Title"] + "  频率：" + row["StationName"]);
                        LogService.Write("任务获取成功，名称：" + row["Title"] + "  频率：" + row["StationName"]);
                        string source = "";
                        if (this.downLoadProgram(row, out source, out errorInfo))
                        {
                            Application.DoEvents();
                            errorInfo = "";
                            //node.SetValue("treeListColumn2", "音频下载完成");
                            AppendMessageLine("音频下载成功！");
                            LogService.Write("音频下载成功！");

                            if (this.SaveXML(row, true, source, out errorInfo))
                            {
                                //node.SetValue("treeListColumn2", "导出成功");
                                AppendMessageLine("导出成功");
                                LogService.Write("导出成功");
                                //node.SetValue("treeListColumn6", DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));

                                if (this.deleteTask(item["ProgramID"].ToString()))
                                {
                                    AppendMessageLine("删除任务成功\n\t");
                                    LogService.Write("删除任务成功\n\t");
                                }
                                else
                                {
                                    AppendMessageLine("删除任务失败，ProgramID：" + row["ProgramID"].ToString() + "\n\t");
                                    LogService.Write("删除任务失败，ProgramID：" + row["ProgramID"].ToString() + "\n\t");
                                }
                            }
                            else
                            {
                                //node.SetValue("treeListColumn2", "导出失败");
                                AppendMessageLine("导出失败,错误信息：" + errorInfo + "\n\t");
                                LogService.Write("导出失败,错误信息：" + errorInfo + "\n\t");
                                this.UpdateTaskStatus(item["ProgramID"].ToString(), -1);
                            }
                        }
                        else
                        {
                            //node.SetValue("treeListColumn2", "音频下载失败");
                            AppendMessageLine("音频下载失败,错误信息：" + errorInfo + "\n\t");
                            LogService.Write("音频下载失败,错误信息：" + errorInfo + "\n\t");
                            this.UpdateTaskStatus(item["ProgramID"].ToString(), -1);
                        }
                    }
                }
            }
        }

        private void doProgram(DataRow item)
        {
            using (SqlConnection conn = new SqlConnection(Globals.X1ConnectionStr))
            {
                string sql = "select folder.foldername as classname,* from program inner join folder on " +
                             "program.folderid = folder.folderid inner join StorageZoneInfo on program.StorageID " +
                             "= StorageZoneInfo.StorageID and StorageZone=" + Globals.Zone +
                             " where ProgramID='" + item["ProgramID"] + "'";
                using (SqlDataAdapter sd = new SqlDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    sd.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        Application.DoEvents();
                        string errorInfo = "";
                        //TreeListNode node = this.treeList_Export.AppendNode(new object[] { row["Title"], "获取到任务", DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"), "" }, null, null);

                        AppendMessageLine("任务获取成功，名称：" + row["Title"] + "  类型 ：制作素材");
                        LogService.Write("任务获取成功，名称：" + row["Title"] + "  类型 ：制作素材");
                        string source = "";
                        if (this.downLoadProgram(row, out source, out errorInfo))
                        {
                            //node.SetValue("treeListColumn2", "音频下载完成");
                            AppendMessageLine("音频下载成功！");
                            LogService.Write("音频下载成功！");

                            if (this.SaveXML(row, false, source, out errorInfo))
                            {
                                //node.SetValue("treeListColumn2", "导出成功");
                                errorInfo = "";
                                AppendMessageLine("导出成功");
                                LogService.Write("导出成功");
                                if (this.deleteTask(item["ProgramID"].ToString()))
                                {
                                    AppendMessageLine("删除任务成功\n\t");
                                    LogService.Write("删除任务成功\n\t");
                                }
                                else
                                {
                                    AppendMessageLine("删除任务失败，ProgramID：" + row["ProgramID"].ToString() + "\n\t");
                                    LogService.Write("删除任务失败，ProgramID：" + row["ProgramID"].ToString() + "\n\t");
                                }

                            }
                            else
                            {
                                //node.SetValue("treeListColumn2", "导出失败");
                                AppendMessageLine("导出失败,错误信息：" + errorInfo + "\n\t");
                                LogService.Write("导出失败,错误信息：" + errorInfo + "\n\t");
                                this.UpdateTaskStatus(row["ProgramID"].ToString(), -1);
                            }
                        }
                        else
                        {
                            //node.SetValue("treeListColumn2", "音频下载失败");
                            AppendMessageLine("音频下载失败,错误信息：" + errorInfo + "\n\t");
                            LogService.Write("音频下载失败,错误信息：" + errorInfo + "\n\t");
                            this.UpdateTaskStatus(row["ProgramID"].ToString(), -1);
                        }
                    }
                }
            }
        }

        private bool deleteTask(string id)
        {
            using (SqlConnection conn = new SqlConnection(Globals.X1ConnectionStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.Transaction = conn.BeginTransaction();

                        cmd.CommandText = "delete from ExportTasks where programID='" + id + "'";
                        cmd.ExecuteNonQuery();

                        cmd.Transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        if (cmd.Transaction != null)
                        {
                            cmd.Transaction.Rollback();
                        }
                    }
                    return false;
                }
            }

        }

        private bool UpdateTaskStatus(string id, int TaskStatus)
        {
            using (SqlConnection conn = new SqlConnection(Globals.X1ConnectionStr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        conn.Open();
                        cmd.Connection = conn;
                        cmd.Transaction = conn.BeginTransaction();

                        cmd.CommandText = "update   ExportTasks  set TaskStatus=" + TaskStatus + " where programID='" + id + "'";
                        cmd.ExecuteNonQuery();

                        cmd.Transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        if (cmd.Transaction != null)
                        {
                            cmd.Transaction.Rollback();
                        }
                        return false;
                    }
                }
            }
        }

        #endregion


        private bool SaveXML(DataRow row, bool isOnair, string targetFile, out string exp)
        {
            exp = "";
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode xmlnode = xmlDoc.CreateXmlDeclaration("1.0", null, null);
                xmlDoc.AppendChild(xmlnode);
                XmlNode rootNode = xmlDoc.CreateElement("ImportAllowRequest");
                string fileName = GetTimeStamp();
                fileName = (isOnair ? row["ItemID"].ToString() : row["ProgramID"].ToString() )+ "_" + fileName;
                #region ImportAllowRequest

                #region DBID
                XmlNode DBIDNode = xmlDoc.CreateElement("DBID");
                DBIDNode.InnerText = Globals.LinkerMedia_DBID;
                rootNode.AppendChild(DBIDNode);

                #endregion

                #region Station
                XmlNode ChnNameNode = xmlDoc.CreateElement("ChnName");//StationName,StationID
                ChnNameNode.InnerText = isOnair ? (row["ChannelID"].ToString() + row["StationName"].ToString()) : "制作素材";
                rootNode.AppendChild(ChnNameNode);

                #endregion

                #region Detail

                XmlNode DetailNode = xmlDoc.CreateElement("Detail");

                #region SourceEntity
                XmlNode SourceEntityNode = xmlDoc.CreateElement("SourceEntity");

                XmlNode SystemNameNode = xmlDoc.CreateElement("SystemName");
                SystemNameNode.InnerText = "杭州联汇媒资系统";
                SourceEntityNode.AppendChild(SystemNameNode);

                XmlNode SystemIDNode = xmlDoc.CreateElement("SystemID");
                SystemIDNode.InnerText = "0";
                SourceEntityNode.AppendChild(SystemIDNode);

                DetailNode.AppendChild(SourceEntityNode);
                #endregion

                #region CreateUser
                XmlNode CreateUserNode = xmlDoc.CreateElement("CreateUser");

                XmlNode UserNameNode = xmlDoc.CreateElement("UserName");
                UserNameNode.InnerText = Globals.LinkerMedia_UserName;
                CreateUserNode.AppendChild(UserNameNode);

                XmlNode UserIDNode = xmlDoc.CreateElement("UserID");
                UserIDNode.InnerText = Globals.LinkerMedia_UserID;
                CreateUserNode.AppendChild(UserIDNode);

                DetailNode.AppendChild(CreateUserNode);
                #endregion

                #region CreateDate
                XmlNode CreateDateNode = xmlDoc.CreateElement("CreateDate");
                CreateDateNode.InnerText = DateTime.Now.ToString();
                DetailNode.AppendChild(CreateDateNode);
                #endregion

                #region ResourceEntryList
                XmlNode ResourceEntryListNode = xmlDoc.CreateElement("ResourceEntryList");
                #region ResourceEntry
                XmlNode ResourceEntryNode = xmlDoc.CreateElement("ResourceEntry");

                #region ResourceID
                XmlNode ResourceIDNode = xmlDoc.CreateElement("ResourceID");

                #region UniqueID
                XmlNode UniqueIDNode = xmlDoc.CreateElement("UniqueID");
                UniqueIDNode.InnerText = isOnair ? row["ItemID"].ToString() : row["ProgramID"].ToString();
                ResourceIDNode.AppendChild(UniqueIDNode);
                #endregion

                ResourceEntryNode.AppendChild(ResourceIDNode);
                #endregion

                #region Title
                XmlNode TitleNode = xmlDoc.CreateElement("Title");

                #region ResourceName
                XmlNode ResourceNameNode = xmlDoc.CreateElement("ResourceName");
                ResourceNameNode.InnerText = row["Title"].ToString();
                TitleNode.AppendChild(ResourceNameNode);
                #endregion

                ResourceEntryNode.AppendChild(TitleNode);
                #endregion

                #region ResourceType
                XmlNode ResourceTypeNode = xmlDoc.CreateElement("ResourceType");
                ResourceTypeNode.InnerText = "0";
                ResourceEntryNode.AppendChild(ResourceTypeNode);
                #endregion

                #region SecretLevel
                XmlNode SecretLevelNode = xmlDoc.CreateElement("SecretLevel");
                SecretLevelNode.InnerText = "0";
                ResourceEntryNode.AppendChild(SecretLevelNode);
                #endregion

                #region Keyword
                XmlNode KeywordNode = xmlDoc.CreateElement("Keyword");
                KeywordNode.InnerText = row["Title"].ToString();
                ResourceEntryNode.AppendChild(KeywordNode);
                #endregion

                #region CreateUser
                CreateUserNode = xmlDoc.CreateElement("CreateUser");

                #region UserName
                UserNameNode = xmlDoc.CreateElement("UserName");
                UserNameNode.InnerText = Globals.LinkerMedia_UserName; //row["CreatorName"].ToString();
                CreateUserNode.AppendChild(UserNameNode);
                #endregion

                ResourceEntryNode.AppendChild(CreateUserNode);
                #endregion

                #region CreateDate
                CreateDateNode = xmlDoc.CreateElement("CreateDate");
                CreateDateNode.InnerText = isOnair ? row["FileDate"].ToString() : row["CreateDateTime"].ToString();
                ResourceEntryNode.AppendChild(CreateDateNode);
                #endregion

                #region MasterFileEntity
                XmlNode MasterFileEntityNode = xmlDoc.CreateElement("MasterFileEntity");

                #region FileID
                XmlNode FileIDNode = xmlDoc.CreateElement("FileID");
                FileIDNode.InnerText = isOnair ? row["ItemID"].ToString() : row["ProgramID"].ToString();
                MasterFileEntityNode.AppendChild(FileIDNode);
                #endregion

                #region Keyword
                KeywordNode = xmlDoc.CreateElement("Keyword");
                KeywordNode.InnerText = row["Title"].ToString(); ;
                MasterFileEntityNode.AppendChild(KeywordNode);
                #endregion

                #region FileFormat
                XmlNode FileFormatNode = xmlDoc.CreateElement("FileFormat");

                #region FileType
                XmlNode FileTypeNode = xmlDoc.CreateElement("FileType");
                FileTypeNode.InnerText = "0";
                FileFormatNode.AppendChild(FileTypeNode);
                #endregion

                #region AudioInfo
                XmlNode AudioInfoNode = xmlDoc.CreateElement("AudioInfo");

                #region MasterType
                XmlNode MasterTypeNode = xmlDoc.CreateElement("MasterType");
                MasterTypeNode.InnerText = "0";
                AudioInfoNode.AppendChild(MasterTypeNode);
                #endregion

                #region SubType
                XmlNode SubTypeNode = xmlDoc.CreateElement("SubType");
                SubTypeNode.InnerText = "0";
                AudioInfoNode.AppendChild(SubTypeNode);
                #endregion

                #region Channels
                XmlNode ChannelsNode = xmlDoc.CreateElement("Channels");
                ChannelsNode.InnerText = "2";
                AudioInfoNode.AppendChild(ChannelsNode);
                #endregion

                #region FileLen
                XmlNode FileLenNode = xmlDoc.CreateElement("FileLen");
                FileLenNode.InnerText = row["Duration"].ToString();
                AudioInfoNode.AppendChild(FileLenNode);
                #endregion

                #region BitRate
                XmlNode BitRateNode = xmlDoc.CreateElement("BitRate");
                BitRateNode.InnerText = "256";
                AudioInfoNode.AppendChild(BitRateNode);
                #endregion

                #region SamplingFrequency
                XmlNode SamplingFrequencyNode = xmlDoc.CreateElement("SamplingFrequency");
                SamplingFrequencyNode.InnerText = "0";
                AudioInfoNode.AppendChild(SamplingFrequencyNode);
                #endregion

                #region BitDepth
                XmlNode BitDepthNode = xmlDoc.CreateElement("BitDepth");
                BitDepthNode.InnerText = "0";
                AudioInfoNode.AppendChild(BitDepthNode);
                #endregion

                #region BlockAlign
                XmlNode BlockAlignNode = xmlDoc.CreateElement("BlockAlign");
                BlockAlignNode.InnerText = "0";
                AudioInfoNode.AppendChild(BlockAlignNode);
                #endregion

                FileFormatNode.AppendChild(AudioInfoNode);
                #endregion

                MasterFileEntityNode.AppendChild(FileFormatNode);
                #endregion

                #region FileSize
                XmlNode FileSizeNode = xmlDoc.CreateElement("FileSize");
                FileInfo info = new FileInfo(targetFile);
                FileSizeNode.InnerText = info.Length.ToString();
                MasterFileEntityNode.AppendChild(FileSizeNode);
                #endregion

                #region FilePath
                XmlNode FilePathNode = xmlDoc.CreateElement("FilePath");
                #region HTTP
                XmlNode HTTPNode = xmlDoc.CreateElement("HTTP");
                HTTPNode.InnerText = fileName + ".s48"; //(isOnair ? row["ItemID"].ToString() : row["ProgramID"].ToString()) + ".s48";
                FilePathNode.AppendChild(HTTPNode);
                #endregion
                MasterFileEntityNode.AppendChild(FilePathNode);
                #endregion

                #region FileName
                XmlNode FileNameNode = xmlDoc.CreateElement("FileName");

                FileNameNode.InnerText = row["Title"].ToString();

                MasterFileEntityNode.AppendChild(FileNameNode);
                #endregion

                ResourceEntryNode.AppendChild(MasterFileEntityNode);
                #endregion

                ResourceEntryListNode.AppendChild(ResourceEntryNode);
                #endregion

                DetailNode.AppendChild(ResourceEntryListNode);
                #endregion

                rootNode.AppendChild(DetailNode);

                #endregion

                #endregion

                xmlDoc.AppendChild(rootNode); 
               
                string Path = Globals.DefaultDir + "\\output\\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
               

                xmlDoc.Save(Path + "\\" + fileName + ".xml");
                info.MoveTo(Path + "\\" + fileName + ".s48");

                return true;

            }
            catch (Exception ex)
            {
                exp = ex.Message;
                return false;
            }

        }
        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        private bool downLoadProgram(DataRow row, out string sourceFile, out string errorInfo)
        {
            errorInfo = "";
            sourceFile = Globals.DefaultDir + "\\temp\\" + Guid.NewGuid() + ".s48";
            try
            {
                using (this.ftp1 = new Dart.PowerTCP.Ftp.Ftp())
                {
                    this.ftp1.Server = row["FTPServer1"].ToString();
                    this.ftp1.ServerPort = int.Parse(row["FTPPort1"].ToString());
                    this.ftp1.Username = row["FTPUser1"].ToString();
                    this.ftp1.Password = row["FTPPassword1"].ToString();

                    this.ftp1.Get(row["SoundFileName"].ToString().Replace("\\", "/"), sourceFile);
                    return true;
                }
            }
            catch
            {

                try
                {
                    using (this.ftp1 = new Dart.PowerTCP.Ftp.Ftp())
                    {
                        this.ftp1.Server = row["FTPServer2"].ToString();
                        this.ftp1.ServerPort = int.Parse(row["FTPPort2"].ToString());
                        this.ftp1.Username = row["FTPUser2"].ToString();
                        this.ftp1.Password = row["FTPPassword2"].ToString();
                        this.ftp1.Get(row["SoundFileName"].ToString().Replace("\\", "/"), sourceFile);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    errorInfo = ex.Message;
                    return false;
                }
            }

        }

        #region 后台批量导出

        private void BatchTask()
        {
            try
            {
                if (txtInfo.Text.Length > txtInfo.MaxLength)
                    txtInfo.Text = "";
                Application.DoEvents();
                DataTable dtSys = GetSys();
                if (dtSys == null || dtSys.Rows.Count <= 0)
                {
                    AppendMessageLine("未获取到播出系统信息！");
                    LogService.Write("未获取到播出系统信息！");
                    return;
                }
                INIClass ini = new INIClass(Application.StartupPath + "\\config.ini");
                foreach (DataRow sysRow in dtSys.Rows)
                {
                    string FileDate = "1900-01-01 00:00:00.000";
                    string LastFileDate = "1900-01-01 00:00:00.000";
                    try
                    {
                        string air5Str = ini.IniReadValue("conn", sysRow["Keys"].ToString());
                        if (air5Str.Trim().Length == 0)
                        {
                            continue;
                        }
                        using (SqlConnection conn = new SqlConnection(air5Str))
                        {
                            string strSysWorkGroupID = "  select g.WorkgroupID,g.WorkgroupName,s.StationNumber from Workgroups g,Workgroup2Stations w2s,Stations s where g.WorkgroupID=w2s.WorkgroupID  and s.StationID=w2s.StationID  and w2s.InStations=1 and s.StationName='" + sysRow["Name"].ToString() + "' ";

                            DataTable SysWorkGroupIDDT = new DataTable();
                            using (SqlDataAdapter sd = new SqlDataAdapter(strSysWorkGroupID, conn))
                            {
                                sd.Fill(SysWorkGroupIDDT);

                            }
                            if (SysWorkGroupIDDT.Rows.Count <= 0)
                            {
                                continue;
                            }

                            FileDate = ini.IniReadValue("FileDate", sysRow["Keys"].ToString());
                            string strSQL = "select ItemID,FileDate from Items p where  p.AudioExists=1   and p.WorkgroupID='" + SysWorkGroupIDDT.Rows[0]["WorkgroupID"].ToString() + "'  and p.Released=1 and p.FileDate>'" + FileDate + "' and p.Deleted=0  and (p.CategoryType=5 OR p.CategoryType=11) order by FileDate";

                            DataTable ItemsDT = new DataTable();
                            using (SqlDataAdapter sd = new SqlDataAdapter(strSQL, conn))
                            {
                                sd.Fill(ItemsDT);

                            }
                            if (ItemsDT.Rows.Count > 0)
                            {
                                AppendMessageLine("频率：《" + sysRow["Name"].ToString() + "》 共有" + ItemsDT.Rows.Count + "条数据需要导出（FileDate：" + FileDate + "）");
                                LogService.Write("频率：《" + sysRow["Name"].ToString() + "》 共有" + ItemsDT.Rows.Count + "条数据需要导出（FileDate：" + FileDate + "）");
                                foreach (DataRow item in ItemsDT.Rows)
                                {
                                    try
                                    {
                                        LastFileDate = item["FileDate"].ToString();
                                        if (Globals.myExportTasksManager.IsExistExportTask(item["ItemID"].ToString(), sysRow["Keys"].ToString()))
                                        {

                                            AppendMessageLine("任务已存在, ItemID:" + item["ItemID"].ToString() + " 频率：" + sysRow["Keys"].ToString());
                                            LogService.Write("任务已存在, ItemID:" + item["ItemID"].ToString() + " 频率：" + sysRow["Keys"].ToString());
                                            ini.IniWriteValue("FileDate", sysRow["Keys"].ToString(), LastFileDate);
                                        }
                                        else
                                        {

                                            doItem(sysRow["Keys"].ToString(), SysWorkGroupIDDT.Rows[0]["StationNumber"].ToString(), item["ItemID"].ToString(), conn);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        LogService.WriteErr("导出失败, ItemID:" + item["ItemID"].ToString() + " 频率：" + sysRow["Keys"].ToString() + "错误信息：" + ex.Message);
                                        continue;
                                    }

                                }
                                ini.IniWriteValue("FileDate", sysRow["Keys"].ToString(), LastFileDate);
                            }
                            else
                            {
                                AppendMessageLine("频率：《" + sysRow["Name"].ToString() + "》 共有" + ItemsDT.Rows.Count + "条数据需要导出（FileDate：" + FileDate + "）");
                                LogService.Write("频率：《" + sysRow["Name"].ToString() + "》 共有" + ItemsDT.Rows.Count + "条数据需要导出（FileDate：" + FileDate + "）");
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogService.WriteErr("导出失败, 频率：" + sysRow["Keys"].ToString() + "错误信息：" + ex.Message);
                        continue;
                    }

                }


                if (!isStart)
                {
                    AppendMessageLine("任务停止！");
                    LogService.Write("任务停止！");
                    return;
                }

            }
            catch (Exception ex)
            {
                AppendMessageLine("程序错误，方法：BatchTask 错误信息：" + ex.Message);
                LogService.WriteErr("程序错误，方法：BatchTask 错误信息：" + ex.Message);
            }
            finally
            {
                Globals.ExecuteStopDate = DateTime.Now;
                Common.FlushMemory();
            }
        }

        private void doItem(string ChannelID, string StationNumber, string ItemId, SqlConnection conn)
        {

            string sql = "select (select top 1 CategoryName from Categories where CategoryID=items.Artist1 " +
                         "or CategoryID=items.Artist3) as CreatorName,(select top 1 CategoryName from Categories " +
                         "inner join Stacks on Categories.CategoryID=Stacks.StackID where Stacks.ItemID=items.ItemID) as ClassName," +
                         "(select top 1 StationID from Stations where StationNumber = " + StationNumber + " ) as StationID," +
                        "(select top 1 StationName from Stations where StationNumber = " + StationNumber + " ) as StationName," +
                              StationNumber + "as ChannelID," +
                         " * from items inner join StorageZone on items.SoundStorageID = StorageZone.StorageID " +
                         "and StorageZone.ZoneID=" + Globals.Zone + " where ItemID='" + ItemId + "'";

            using (SqlDataAdapter sd = new SqlDataAdapter(sql, conn))
            {
                DataTable dt = new DataTable();
                sd.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    Application.DoEvents();
                    string errorInfo = "";
                    //TreeListNode node = this.treeList_Export.AppendNode(new object[] { row["Title"], "获取到任务", DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"), "" }, null, null);
                    AppendMessageLine("任务获取成功，名称：" + row["Title"] + "  频率：" + row["StationName"]);
                    LogService.Write("任务获取成功，名称：" + row["Title"] + "  频率：" + row["StationName"]);
                    string source = "";
                    if (this.downLoadProgram(row, out source, out errorInfo))
                    {
                        Application.DoEvents();
                        errorInfo = "";

                        AppendMessageLine("音频下载成功！");
                        LogService.Write("音频下载成功！");

                        if (this.SaveXML(row, true, source, out errorInfo))
                        {

                            AppendMessageLine("导出成功");
                            LogService.Write("导出成功");
                            Globals.myExportTasksManager.AddExportTask(ItemId, ChannelID, 1);

                        }
                        else
                        {

                            AppendMessageLine("导出失败,错误信息：" + errorInfo + "\n\t");
                            LogService.Write("导出失败,错误信息：" + errorInfo + "\n\t");
                            Globals.myExportTasksManager.AddExportTask(ItemId, ChannelID, -1);
                        }
                    }
                    else
                    {
                        AppendMessageLine("音频下载失败,错误信息：" + errorInfo + "\n\t");
                        LogService.Write("音频下载失败,错误信息：" + errorInfo + "\n\t");
                        Globals.myExportTasksManager.AddExportTask(ItemId, ChannelID, -1);
                    }
                }
            }
        }
        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns></returns>
        private DataTable GetSys()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(Globals.X1ConnectionStr))
                {
                    //string sql = "SELECT   ID, Name, Description, WebServiceURL, WebServiceURL2, Keys, Category, CategoryName, CreateUserName, CreateUserID, CreateDateTime, ModifyUserName, ModifyUserID, ModifyDateTime FROM      Systems order by Category, Keys";
                    string sql = "SELECT Name,Keys  FROM  Systems Syss  where Syss.CategoryName='播出系统' order by   Keys";

                    using (SqlDataAdapter sd = new SqlDataAdapter(sql, conn))
                    {
                        sd.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.WriteErr("获取系统错误：" + ex.Message);
                dt = null;
            }
            return dt;

        }
        #endregion

        #endregion

        #region//窗体事件
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsStart)
            {
                MessageBox.Show("程序运行中，请先停止！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }

            if (ManagerThread != null && ManagerThread.IsAlive)
            {
                ManagerThread.Abort();
            }

        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //判断是否已经最小化于托盘 
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示 
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点 
                this.Activate();
                //任务栏区显示图标 
                this.ShowInTaskbar = true;
                //托盘区图标隐藏 
                notifyIcon.Visible = false;
            }
        }
        private void FrmMain_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮 
            if (WindowState == FormWindowState.Minimized)
            {
                //托盘显示图标等于托盘图标对象 
                //注意notifyIcon1是控件的名字而不是对象的名字 

                //隐藏任务栏区图标 
                this.ShowInTaskbar = false;
                //图标显示在托盘区 
                notifyIcon.Visible = true;
            }
        }

        private void btnShowForm_Click(object sender, EventArgs e)
        {
            notifyIcon_MouseDoubleClick(sender, null);
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            IsStart = !IsStart;

        }

        private void btnSetting_Click(object sender, EventArgs e)
        {

            if (IsStart)
                MessageBox.Show("程序运行中，无法设置！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void btnCloseForm_Click(object sender, EventArgs e)
        {
            if (IsStart)
            {
                notifyIcon_MouseDoubleClick(sender, null);
                MessageBox.Show("程序运行中，请先停止！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Close();
            }
        }
        #endregion

        private void FrmMain_Load(object sender, EventArgs e)
        {
            Init();
        }
    }
}

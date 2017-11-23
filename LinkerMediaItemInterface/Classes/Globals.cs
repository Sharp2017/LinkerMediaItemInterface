using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinkerMediaItemInterface.DBHelp;

namespace LinkerMediaItemInterface.Classes
{
    public class Globals
    {
        public static int Zone = 2;
        public static string X1ConnectionStr = "";
        public static string DefaultDir = "";  
        public static string DefaultProgramLibID = "";
        public static string DefaultStorageID = "";
        public static int RequestInterval = 1;

        /// <summary>
        /// 联汇的数据库ID
        /// </summary>
        public static string LinkerMedia_DBID = "85";
        /// <summary>
        ///联汇的用户名
        /// </summary>
        public static string LinkerMedia_UserName = "lx";
        /// <summary>
        /// 联汇的用户ID
        /// </summary>
        public static string LinkerMedia_UserID = "134";
        /// <summary>
        /// 检查停止时间
        /// </summary>
        public static DateTime ExecuteStopDate;
        /// <summary>
        /// 是否批量导入
        /// </summary>
        public static int BatchImport = 0;
        /// <summary>
        /// 任务数据管理类
        /// </summary>
        public static ExportTasksManager myExportTasksManager = new ExportTasksManager();

    }
}

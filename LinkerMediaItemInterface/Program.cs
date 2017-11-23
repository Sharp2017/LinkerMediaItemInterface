using LinkerMediaItemInterface.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LinkerMediaItemInterface
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (Common.IsAppRunning("LinkerMediaItemInterface"))
                {
                    MessageBox.Show("程序正在运行，请先退出！", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                INIClass ini = new INIClass(Application.StartupPath + "\\config.ini");
                Globals.X1ConnectionStr = ini.IniReadValue("conn", "x1");
                Globals.DefaultDir = ini.IniReadValue("dir", "path");
                Globals.Zone = int.Parse(ini.IniReadValue("zone", "zone"));
               
                Globals.RequestInterval = int.Parse(ini.IniReadValue("Interval", "RequestInterval"));

                Globals.LinkerMedia_DBID =   ini.IniReadValue("LinkerMedia", "DBID");
                Globals.LinkerMedia_UserName =  ini.IniReadValue("LinkerMedia", "UserName");
                Globals.LinkerMedia_UserID = ini.IniReadValue("LinkerMedia", "UserID");

                Globals.BatchImport = int.Parse(ini.IniReadValue("ImportType", "BatchImport"));

                Application.Run(new FrmMain());
            }
            catch (Exception ex)
            { 
                LogService.WriteErr(ex.Message);
            }
        }
    }
}

using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System;


namespace LinkerMediaItemInterface.Classes
{
    /// <summary>
    /// 本地日志服务
    /// </summary>
    public class LogService
    {
        public static string LogDir = "Logs";
        private static object o = new object();

        // Methods
        public LogService()
        {
            o = new object();
            LogDir = "Logs";
        }

        public static void Write(string content)
        {
            string fileName = "Log[" + DateTime.Now.ToString("yyyy-MM-dd") + "].log";
            string prompStr = "[" + DateTime.Now.ToString() + "]:";
            WriteFile(fileName, prompStr, content);
        }

        public static void Write(string logType, string content)
        {
            string fileName = logType + "[" + DateTime.Now.ToString("yyyy-MM-dd") + "].log";
            string prompStr = "[" + DateTime.Now.ToString() + "]:";
            WriteFile(fileName, prompStr, content);
        }

        public static void WriteDebug(Exception ex)
        {
            WriteDebug("", ex);
        }
        public static void WriteDebug(string logType, Exception ex)
        {
            object obj2 = ("" + "异常消息：" + ex.Message + "\r\n") + "\t 引发异常的对象：" + ex.Source + "\r\n";
            string content = string.Concat(new object[] { obj2, "\t 引发异常的方法：", ex.TargetSite, "\r\n" }) + "\t 异常时的堆栈：" + ex.StackTrace + "\r\n";
            string fileName = logType + "DEBUG[" + DateTime.Now.ToString("yyyy-MM-dd") + "].log";
            string prompStr = "[" + DateTime.Now.ToString() + "]:";
            WriteFile(fileName, prompStr, content);
        }

        public static void WriteErr(string content)
        {
            string fileName = "Err[" + DateTime.Now.ToString("yyyy-MM-dd") + "].log";
            string prompStr = "[" + DateTime.Now.ToString() + "]:";
            WriteFile(fileName, prompStr, content);
        }

        public static void WriteFile(string fileName, string prompStr, string content)
        {
            lock (o)
            {
                StreamWriter writer = null;
                try
                {
                    string startupPath = Application.StartupPath;
                    if (LogDir != "")
                    {
                        startupPath = (LogDir[1] == ':') ? LogDir : (startupPath + @"\" + LogDir);
                    }
                    if (!Directory.Exists(startupPath))
                    {
                        Directory.CreateDirectory(startupPath);
                    }
                    string path = startupPath + @"\" + fileName;

                    if (File.Exists(path))
                    {
                        writer = File.AppendText(path);
                    }
                    else
                    {
                        writer = File.CreateText(path);
                    }
                    try
                    {
                        writer.WriteLine(prompStr + " " + content);
                    }
                    finally
                    {

                        writer.Close();
                    }
                }
                catch
                {

                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                    }

                }

            }
        }

        public static void WriteObject(object o)
        {
            WriteObject("", o);
        }

        public static void WriteObject(string logType, object o)
        {
            StringBuilder builder = new StringBuilder("");
            if (o != null)
            {
                Type type = o.GetType();
                builder.AppendLine("类型名:" + type.Name);
                FieldInfo[] fields = o.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo info in fields)
                {
                    builder.Append("\t" + info.Name + ":");
                    object obj2 = info.GetValue(o);
                    if (obj2 == null)
                    {
                        builder.AppendLine("NULL");
                    }
                    else if (info.FieldType.IsValueType)
                    {
                        builder.AppendLine(Convert.ToString(obj2));
                    }
                    else
                    {
                        builder.AppendLine(obj2 as string);
                    }
                }
            }
            else
            {
                builder.AppendLine("对象为NULL");
            }
            string fileName = logType + "Log_ObjectInfo[" + DateTime.Now.ToString("yyyy-MM-dd") + "].log";
            string prompStr = "[" + DateTime.Now.ToString() + "]:";
            WriteFile(fileName, prompStr, builder.ToString());
        }
    }
}

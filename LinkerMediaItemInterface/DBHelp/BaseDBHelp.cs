using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinkerMediaItemInterface.Classes;

namespace LinkerMediaItemInterface.DBHelp
{
    public class BaseDBHelp
    {
        protected SQLiteHelper mySQLiteHelper;
        protected string connectionString = "Data Source=" + System.IO.Directory.GetCurrentDirectory() + @"\Data\DB\ExportTasksDB.db";

        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        public BaseDBHelp()
        {
            CreatDBHelp();
        }
        #region Base
        /// <summary>
        /// 创建数据库操作对象
        /// </summary>
        public void CreatDBHelp()
        {
            try
            {
                 mySQLiteHelper = new SQLiteHelper(connectionString);
            }
            catch (Exception ex)
            {
                LogService.WriteErr("创建数据库操作对象失败，错误信息：" + ex.Message);
                throw ex;
            }
        }

        public void Open()
        {
            try
            {
                mySQLiteHelper.Open();
            }
            catch (Exception ex)
            {
                LogService.WriteErr("打开数据库连接失败，错误信息：" + ex.Message);
                throw ex;
            }
        }
        public void Close()
        {
            try
            {
                mySQLiteHelper.Close();
            }
            catch (Exception ex)
            {
                LogService.WriteErr("关闭数据库连接失败，错误信息：" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                //开始事务
                mySQLiteHelper.BeginTransaction();
            }
            catch (Exception ex)
            {
                LogService.WriteErr("开始事务失败，错误信息：" + ex.Message);
                throw ex;
            }

        }
        /// <summary>
        /// 提交事务
        /// </summary>
        /// <returns></returns>
        public bool CommitTransaction()
        {
            try
            {
                //提交事务
                return mySQLiteHelper.CommitTransaction();
            }
            catch (Exception ex)
            {
                LogService.WriteErr("提交事务失败，错误信息：" + ex.Message);
                throw ex;
            }

        }

        /// <summary>
        /// 事务回滚
        /// </summary>
        /// <returns></returns>
        public bool RollbackTransaction()
        {
            try
            {
                //提交事务
                return mySQLiteHelper.RollbackTransaction();
            }
            catch (Exception ex)
            {
                LogService.WriteErr("事务回滚失败，错误信息：" + ex.Message);
                throw ex;
            }

        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;

namespace LinkerMediaItemInterface.DBHelp
{
    public class ExportTasksManager : BaseDBHelp
    {
        //ExportTasks

        /// <summary>
        /// 添加成功的任务
        /// </summary>
        /// <param name="pArtist"></param>
        public bool AddExportTask(string ItemID, string ChannelID, int TaskStatus)
        {
            try
            {
                this.Open();


                string SQLString =
                    "Insert into ExportTasks  (ChannelID,ProgramID,TaskStatus,TaskDateTime) VALUES (@ChannelID,@ProgramID,@TaskStatus,@TaskDateTime)";

                SQLiteParameter[] param = new SQLiteParameter[5];
                param[0] = new SQLiteParameter("@ChannelID", DbType.String);
                param[0].Value = ChannelID;

                param[1] = new SQLiteParameter("@ProgramID", DbType.String);
                param[1].Value = ItemID;

                param[2] = new SQLiteParameter("@TaskStatus", DbType.Int32);
                param[2].Value = TaskStatus;

                param[3] = new SQLiteParameter("@TaskDateTime", DbType.DateTime);
                param[3].Value = DateTime.Now;


                return mySQLiteHelper.Exc(SQLString, param);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Close();
            }
        }


        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="ChannelID"></param>
        /// <returns></returns>
        public bool IsExistExportTask(string ItemID, string ChannelID)
        {
            try
            {
                this.Open();
                DataTable dt = null;
                string strSQL = "";
                SQLiteParameter[] param = new SQLiteParameter[2];

                strSQL = "select * from ExportTasks where  ChannelID=@ChannelID and  ProgramID=@ItemID  ";
                param[0] = new SQLiteParameter("@ChannelID", DbType.String);
                param[0].Value = ChannelID;
                param[1] = new SQLiteParameter("@ItemID", DbType.String);
                param[1].Value = ItemID;

                DataSet ds = mySQLiteHelper.Query(strSQL, param);
                if (ds != null)
                {
                    dt = ds.Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Close();
            }
        }

        /// <summary>
        /// 根据ID更新信息
        /// </summary>
        /// <param name="pArtist"></param>
        public bool UpDateExportTask(string ItemID, string ChannelID, int TaskStatus)
        {
            try
            {
                this.Open();

                string SQLString = "Update ExportTasks SET TaskStatus=@TaskStatus  Where ItemID=@ItemID and ChannelID=@ChannelID";
                    SQLiteParameter[] param = new SQLiteParameter[3];
                    param[0] = new SQLiteParameter("@TaskStatus", DbType.Int32);
                    param[0].Value = TaskStatus;
                    param[1] = new SQLiteParameter("@ItemID", DbType.String);
                    param[1].Value = ItemID;

                    param[1] = new SQLiteParameter("@ChannelID", DbType.String);
                    param[1].Value = ChannelID;

                    return mySQLiteHelper.Exc(SQLString, param);
                 
            }
            catch (Exception ex)
            { 
                throw ex;
            }
            finally
            {
                this.Close();
            }
        }
    }
}

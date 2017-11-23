using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;

namespace LinkerMediaItemInterface.DBHelp
{
    public class SQLiteHelper
    {
        static string _connectionString = "Data Source=" + System.IO.Directory.GetCurrentDirectory() + @"\Data\DB\ExportTasksDB.db"; 
        SQLiteConnection _connection;
        SQLiteTransaction trans = null;

        public SQLiteHelper()
        {
            _connection = new SQLiteConnection(_connectionString);

        }
        public SQLiteHelper(string connectionString)
            : this()
        {
            _connectionString = connectionString;
        }

        public void Open()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }
        public void Close()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
        }

        public DataSet Query(string sql)
        {
            return Query(sql, null);
        }
        /// <summary>
        /// 创建并开始事务
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                trans = _connection.BeginTransaction();

            }
            catch (Exception ex)
            {
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
                if (trans != null)
                {
                    trans.Commit();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <returns></returns>
        public bool RollbackTransaction()
        {
            try
            {
                if (trans != null)
                {
                    trans.Rollback();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public DataSet Query(string sql, params SQLiteParameter[] parameters)
        {
            try
            {
                SQLiteCommand command = ExcDbCommand(sql, parameters);
                DataSet ds = new DataSet();
                SQLiteDataAdapter da = new SQLiteDataAdapter(command);
                da.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                return null;
                throw ex;
            }

        }

        public bool Exc(string sql)
        {
            try
            {
                return Exc(sql, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public bool Exc(string sql, params SQLiteParameter[] parameters)
        {
            try
            {

                SQLiteCommand command = ExcDbCommand(sql, parameters);
                int result = command.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public SQLiteDataReader Read(string sql)
        {
            return Read(sql, null);
        }
        public SQLiteDataReader Read(string sql, params SQLiteParameter[] parameters)
        {
            SQLiteCommand command = ExcDbCommand(sql, parameters);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;
        }
        SQLiteCommand ExcDbCommand(string sql, SQLiteParameter[] parameters)
        {
            SQLiteCommand command = new SQLiteCommand(sql, _connection);
            command.CommandType = CommandType.Text;
            if (parameters == null || parameters.Length == 0)
                return command;
            foreach (SQLiteParameter param in parameters)
            {
                if (param != null)
                    command.Parameters.Add(param);
            }
            return command;
        }
    }
}

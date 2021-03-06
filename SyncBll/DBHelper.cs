﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using MySql.Data.MySqlClient;

namespace SyncBll
{
    public class DBHelper
    {
        //数据库连接字符串(web.config来配置)，可以动态更改ConnectionString支持多数据库.		
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["wordConn"].ConnectionString;

        #region 原生
        /// <summary>
        /// 执行SQL语句，返回影响的记录数，没有传入事务
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params MySqlParameter[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {

                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran(List<String> SQLStringList)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                MySqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    int count = 0;
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch
                {
                    tx.Rollback();
                    return 0;
                }
            }
        }



        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>
        /// <param name="SqlParameterList">多条SQL参数</param>
        /// <returns></returns>
        public static int ExecuteSqlTran(List<String> SQLStringList, List<MySqlParameter[]> SqlParameterList)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                MySqlTransaction tx = conn.BeginTransaction();
                int ac = 0;
                try
                {
                    int count = 0;
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            MySqlParameter[] arr = null;
                            if (SqlParameterList.Count > n)
                            {
                                arr = SqlParameterList[n];
                            }
                            PrepareCommand(cmd, conn, tx, strsql, arr);
                            count += cmd.ExecuteNonQuery();
                            ac++;
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch (Exception ex)
                {
                    var a = ac;
                    var b = SQLStringList[ac];
                    var c = SqlParameterList[ac];

                    tx.Rollback();
                    throw ex;
                    //return 0;
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务，并返回插入内容的主键。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>
        /// <param name="SqlParameterList">多条SQL参数</param>
        /// <returns></returns>
        public static List<int> ExecuteSqlTranReturn(List<String> SQLStringList, List<MySqlParameter[]> SqlParameterList)
        {
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                MySqlTransaction tx = conn.BeginTransaction();
                int ac = 0;
                try
                {
                    List<int> master = new List<int>();
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            MySqlParameter[] arr = null;
                            if (SqlParameterList.Count > n)
                            {
                                arr = SqlParameterList[n];
                            }
                            PrepareCommand(cmd, conn, tx, strsql, arr);
                            //count += cmd.ExecuteNonQuery();
                            master.Add(Convert.ToInt32(cmd.ExecuteScalar()));
                            ac++;
                        }
                    }
                    tx.Commit();
                    return master;
                }
                catch (Exception ex)
                {
                    var a = ac;
                    var b = SQLStringList[ac];
                    var c = SqlParameterList[ac];

                    tx.Rollback();
                    throw ex;
                    //return 0;
                }
            }
        }
        #region ExecuteScalar
        public static object ExecuteScalar(string sql, MySqlParameter[] paras)
        {
            object obj;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sql, paras);
                        obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();

                    }
                    catch
                    {

                        return 0;
                    }
                }
            }
            return obj;
        }
        #endregion

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader(string strSQL)
        {
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = new MySqlCommand(strSQL, connection);
            try
            {
                connection.Open();
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                throw e;
            }

        }
        #endregion

        #region 扩展

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }



        #region 获取单条记录
        /// <summary>
        /// 获取单条记录Scalar
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        public static object GetScalarFile(string sqlStr, params IDataParameter[] dbParameters)
        {
            object queryObject = null;

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {

                MySqlCommand cmd = new MySqlCommand(sqlStr, conn);
                using (cmd)
                { //cmd.CommandTimeout = 3000;
                    cmd.Parameters.AddRange(dbParameters);
                    conn.Open();
                    queryObject = cmd.ExecuteScalar();

                }

            }

            return queryObject;
        }
        #endregion
        #region SetCommand、Adapter



        #endregion
        /// <summary>
        /// 获取单个实体
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="func"></param>
        /// <param name="dbParameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDataInfo<T>(string sqlStr, Func<IDataReader, T> func, params IDataParameter[] dbParameters)
        {
            var list = GetDataInfolList(sqlStr, func, dbParameters);
            var t = default(T);
            if (list != null && list.Any())
            {
                t = list.First();
            }
            return t;
        }

        /// <summary>
        /// 获取实体list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStr"></param>
        /// <param name="func"></param>
        /// <param name="dbParameters"></param>
        /// <returns></returns>
        public static List<T> GetDataInfolList<T>(string sqlStr, Func<IDataReader, T> func, params IDataParameter[] dbParameters)
        {
            var list = new List<T>();
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand(sqlStr, conn);
                using (cmd)
                {  //cmd.CommandTimeout = 3000;
                    cmd.Parameters.AddRange(dbParameters);
                    conn.Open();
                    IDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        list.Add(func(rdr));
                    }

                }

            }

            return list;
        }




        public static DbParameter AddParameterWithValue(string parameterName, object value)
        {
            DbParameter dbParameter = GetDbParameter();
            if (dbParameter == null) return null;

            dbParameter.ParameterName = parameterName;
            dbParameter.Value = value;
            dbParameter.Direction = ParameterDirection.Input;

            return dbParameter;

        }

        protected static DbParameter GetDbParameter()
        {
            return new MySqlParameter();
        }
        #endregion


        public static int ExecuteStatement(String statement, List<MySqlParameter> parameters, CommandType commandtext = CommandType.Text)
        {
            int results = 0;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new MySqlCommand(statement, connection);
                command.CommandType = commandtext;
                if (parameters != null)
                    foreach (var p in parameters)
                        command.Parameters.Add(p);
                results = command.ExecuteNonQuery();
                command.Parameters.Clear();
            }
            return results;
        }

        /// <summary>
        /// 返回数据集合
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="statement">SQL</param>
        /// <param name="readFunc">FUN</param>
        /// <param name="parameters">参数</param>
        /// <param name="commandtext">执行类型</param>
        /// <returns></returns>
        public static List<T> ExecuteStatements<T>(String statement, Func<MySqlDataReader, T> readFunc, List<MySqlParameter> parameters, CommandType commandtext = CommandType.Text)
        {
            List<T> results = new List<T>();
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new MySqlCommand(statement, connection);
                command.CommandType = commandtext;
                if (parameters != null)
                    foreach (var p in parameters)
                        command.Parameters.Add(p);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                            results.Add(readFunc(reader));
                }
            }
            return results;
        }

        public static T ExecuteStatement<T>(String statement, Func<MySqlDataReader, T> readFunc, List<MySqlParameter> parameters, CommandType commandtext = CommandType.Text)
        {
            T results = default(T);
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var command = new MySqlCommand(statement, connection);
                command.CommandType = commandtext;
                if (parameters != null)
                    foreach (var p in parameters)
                        command.Parameters.Add(p);
                using (var reader = command.ExecuteReader())
                {
                    results = readFunc(reader);
                }
            }
            return results;
        }


        public static void ExecuteStatementList(String statement, Action<MySqlDataReader> readFunc, List<MySqlParameter> parameters, CommandType commandtext = CommandType.Text)
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    var command = new MySqlCommand(statement, connection);
                    command.CommandType = commandtext;
                    if (parameters != null)
                        foreach (var p in parameters)
                            command.Parameters.Add(p);
                    using (var reader = command.ExecuteReader())
                    {
                        readFunc(reader);
                    }
                }
                catch
                {

                    throw;
                }

            }
        }



        #region 事务
        /// <summary>
          /// 用指定的数据库连接字符串执行一个命令并返回一个数据集的第一列
          /// </summary>
          /// <remarks>
          ///例如:
          /// Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
          /// </remarks>
          ///<param name="connectionString">一个有效的连接字符串</param>
          /// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
          /// <param name="cmdText">存储过程名称或者sql命令语句</param>
          /// <param name="commandParameters">执行命令所用参数的集合</param>
          /// <returns>用 Convert.To{Type}把类型转换为想要的 </returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
          /// 准备执行一个命令
          /// </summary>
          /// <param name="cmd">sql命令</param>
          /// <param name="conn">OleDb连接</param>
          /// <param name="trans">OleDb事务</param>
          /// <param name="cmdType">命令类型例如 存储过程或者文本</param>
          /// <param name="cmdText">命令文本,例如:Select * from Products</param>
          /// <param name="cmdParms">执行命令的参数</param>
        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, CommandType cmdType, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                foreach (MySqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }


        /// <summary>
          ///使用现有的SQL事务执行一个sql命令（不返回数据集）
          /// </summary>
          /// <remarks>
          ///举例:
          /// int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
          /// </remarks>
          /// <param name="trans">一个现有的事务</param>
          /// <param name="cmdType">命令类型(存储过程, 文本, 等等)</param>
          /// <param name="cmdText">存储过程名称或者sql命令语句</param>
          /// <param name="commandParameters">执行命令所用参数的集合</param>
          /// <returns>执行命令所影响的行数</returns>
        public static int ExecuteNonQuery(MySqlTransaction trans, CommandType cmdType, string cmdText, params MySqlParameter[] commandParameters)
        {
            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        #endregion



    }
}

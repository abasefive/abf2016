using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace DAL
{
    public class GetConnection
    {
        private static MySqlConnection _connection;
        /// <summary>
        /// 获取数据库连接桥
        /// </summary>
        private static MySqlConnection Connection
        {
            get
            {
                //string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
                //string connectionString = "server=localhost;user id=root; password=root; database=system; pooling=false";
                //server=222.222.222.222;port=3306;uid=user;pwd=;database=basename;远程连接的

                string connectionString = "server=121.40.175.139;port=3306;uid=test;pwd=test;database=fuyue_test";
                if (_connection == null)
                {
                    _connection = new MySqlConnection(connectionString);
                    _connection.Open();
                }
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }
                if (_connection.State == ConnectionState.Broken)
                {
                    _connection.Close();
                    _connection.Open();
                }
                return GetConnection._connection;
            }

        }
        /// <summary>
        /// 获取表数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static MySqlDataReader GetDataRead(string sql)
        {
            MySqlCommand command = new MySqlCommand(sql, Connection);
            MySqlDataReader read = command.ExecuteReader();

            return read;
        }
        public static int NoSelect(string sql)
        {
            MySqlCommand command = new MySqlCommand(sql, Connection);
            int row = command.ExecuteNonQuery();
            return row;
        }
        public static DataTable GetDataTable(string sql)
        {
            MySqlCommand command = new MySqlCommand(sql, Connection);

            DataTable dt = new DataTable();

            MySqlDataAdapter sda = new MySqlDataAdapter(command);
            sda.Fill(dt);
            return dt;
        }
        /// <summary>
        /// 执行sql语句，返回一行一列。。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public static string GetScalar(string sql)
        {
            MySqlCommand command = new MySqlCommand(sql, Connection);
            return command.ExecuteScalar().ToString();
        }


    }
}

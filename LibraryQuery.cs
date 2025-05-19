using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WA_Progetto
{
    internal class LibraryQuery
    {
        public DataSet ExecuteQWithParam(string q, SqlParameter param, SqlConnection cnn)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlCommand cmd = new SqlCommand(q, cnn))
                {
                    cmd.Parameters.Add(param);
                    cnn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close();
            }
            return ds;
        }
        public DataSet ExecuteQ(string q, SqlConnection cnn)
        {
            DataSet ds = new DataSet();
            try
            {
                SqlCommand cmd = new SqlCommand(q, cnn);
                cnn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close();
            }
            return ds;
        }

        public List<string> GetRequiredColumns(string tableName, SqlConnection cnn)
        {
            List<string> requiredColumns = new List<string>();
            string query = @"SELECT COLUMN_NAME 
                     FROM INFORMATION_SCHEMA.COLUMNS 
                     WHERE TABLE_NAME = @tableName AND IS_NULLABLE = 'NO' AND COLUMNPROPERTY(object_id(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 0";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, cnn))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    cnn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requiredColumns.Add(reader["COLUMN_NAME"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close();
            }
            return requiredColumns;
        }
        public List<string> GetAllColumnNames(string tableName, SqlConnection cnn)
        {
            List<string> columns = new List<string>();
            string query = @"SELECT COLUMN_NAME 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = @tableName
                        ORDER BY ORDINAL_POSITION";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, cnn))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    cnn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string c = reader["COLUMN_NAME"].ToString();
                            if (c == "Order")
                            {
                                columns.Add("[" + c + "]");
                            }
                            else
                            {
                                columns.Add(c);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close();
            }
            return columns;
        }
        public List<string> GetForeignKeyColumns(string tableName, SqlConnection cnn)
        {
            List<string> foreignKeys = new List<string>();
            string query = @"SELECT kcu.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                        ON rc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
                        WHERE kcu.TABLE_NAME = @tableName
                        ORDER BY kcu.ORDINAL_POSITION";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, cnn))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    cnn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreignKeys.Add(reader["COLUMN_NAME"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close();
            }
            return foreignKeys;
        }
        public string GetReferencedTable(string tableName, string fkColumn, SqlConnection cnn)
        {
            string refTable = null;
            string query = @"SELECT PK.TABLE_NAME AS ReferencedTable  
                               FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC  
                               INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE FK  
                               ON RC.CONSTRAINT_NAME = FK.CONSTRAINT_NAME  
                               INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE PK  
                               ON RC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME  
                               WHERE FK.TABLE_NAME = @tableName AND FK.COLUMN_NAME = @fkColumn";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, cnn))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    cmd.Parameters.AddWithValue("@fkColumn", fkColumn);
                    cnn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            refTable = reader["ReferencedTable"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close();
            }
            return refTable;
        }
    }
}

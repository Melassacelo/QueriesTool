using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WA_Progetto
{
    internal class LibraryScript
    {
        public string FinalScript(List<string> Tables_name, List<string> values, List<string> columnNames, List<string> queriesM)
        {
            string columns = string.Join(",\n\t\t\t", columnNames);
            string parameters = $"@NewIdQueries";
            foreach (string v in values)
            {
                if (string.IsNullOrEmpty(v))
                {
                    parameters += ",\n\t\t\t NULL";
                }
                else
                {
                    parameters += ",\n\t\t\t '" + v + "'";
                }
            }
            string queriesM1 = string.Join("\n", queriesM);
            string query = $"DECLARE @NewIdQueries int\n" +
                $"DECLARE @NewIdQueriesParameter int\n\n" +
                $"IF NOT EXISTS (SELECT 1\n\t\t\t" +
                $"FROM [dbo].[{Tables_name[0]}]\n\t\t\t" +
                $"WHERE [Name]='{values[0]}')\n\t" +
                $"BEGIN\n\t\t" +
                $"SET @NewIdQueries = (SELECT ISNULL(MAX({columnNames[0]}), 0) + 1\n\t\t\t" +
                $"FROM [dbo].[{Tables_name[0]}])\n\n\t\t" +
                $"INSERT INTO [dbo].{Tables_name[0]}\n\t\t\t" +
                $"({columns})\n\t\t" +
                $"VALUES ({parameters});\n\t" +
                $"END\n\n" + queriesM1;
            return query;
        }
        public string DataGridViewScript(DataGridView dgv, List<string> Tables_name, List<string> strings, string columns2, int y)
        {
            string qM = "";
            if (dgv.Tag.ToString() == Tables_name[2])
            {
                qM = $"IF NOT EXISTS (SELECT 1\n\t\t\t" +
                    $"FROM [dbo].[{dgv.Tag.ToString()}]\n\t\t\t" +
                    $"WHERE [Description]='{dgv.Rows[y].Cells[3].Value}')\n\t" +
                    $"BEGIN\n\t\t" +
                    $"SET @NewIdQueriesParameter = (SELECT ISNULL(MAX({strings[0]}), 0) + 1\n\t\t\t" +
                    $"FROM [dbo].[{dgv.Tag.ToString()}])\n\n\t\t";
            }
            string parameters2 = "";
            for (int k = 2; k < dgv.Rows[y].Cells.Count; k++)
            {
                if (dgv.Rows[y].Cells[k].Value.ToString() != "")
                {
                    parameters2 += ",\n\t\t\t '" + dgv.Rows[y].Cells[k].Value + "'";
                }
                else
                {
                    parameters2 += ",\n\t\t\t NULL";
                }

            }
            if (dgv.Tag.ToString() == Tables_name[3])
            {
                qM += $"INSERT INTO [dbo].[{dgv.Tag.ToString()}]\n\t\t\t" +
                    $"({columns2})\n\t\t" +
                    $"VALUES ((SELECT ISNULL(MAX({strings[0]}), 0) + 1 FROM [dbo].[{dgv.Tag.ToString()}]),\n\t\t\t" +
                    $"@NewIdQueriesParameter{parameters2});\n";
            }else if (dgv.Tag.ToString() == Tables_name[2])
            {
                qM += $"INSERT INTO [dbo].[{dgv.Tag.ToString()}]\n\t\t\t" +
                    $"({columns2})\n\t\t" +
                    $"VALUES (@NewIdQueriesParameter,\n\t\t\t" +
                    $"@NewIdQueries{parameters2});\n";
            }
            else
            {
                qM += $"INSERT INTO [dbo].[{dgv.Tag.ToString()}]\n\t\t\t" +
                    $"({columns2})\n\t\t" +
                    $"VALUES ((SELECT ISNULL(MAX({strings[0]}), 0) + 1 FROM [dbo].[{dgv.Tag.ToString()}]),\n\t\t\t" +
                    $"@NewIdQueries{parameters2});\n";
            }

            if (dgv.Tag.ToString() == Tables_name[2])
            {
                qM += $"\tEND\n";
            }
            return qM;
        }
        public (object, bool) TextBoxScript(TextBox txt, DataGridView dgv, List<string> s, int j)
        {
            object resultString = DBNull.Value;
            bool resultBool = true;
            if (txt.Text != "")
            {
                if (CorrectValue(txt.Text, dgv.Columns[j].ValueType))
                {
                    resultString = txt.Text;
                }
                else
                {
                    resultBool = false;
                }
            }
            else if (s.Contains(dgv.Columns[j].HeaderText))
            {
                resultBool = false;
            }
            return (resultString, resultBool);
        }
        public (object, bool) ComboBoxScript(ComboBox cbx, LibraryQuery LQ, SqlConnection cnn, DataGridView dgv, List<string> s, int j, List<string> Tables_Name, DataTable dt=null)
        {
            object resultString = DBNull.Value;
            bool resultBool = true;
            if (cbx.Text != "")
            {
                if (cbx.Tag != null)
                {
                    if (!Tables_Name.Contains(cbx.Tag.ToString()))
                    {
                        string column1 = LQ.GetAllColumnNames(cbx.Tag.ToString(), cnn)[0];
                        string column2 = LQ.GetAllColumnNames(cbx.Tag.ToString(), cnn)[1];
                        string query = $"SELECT {column1} FROM {cbx.Tag.ToString()} WHERE {column2} = '{cbx.Text}'";
                        DataSet ds = LQ.ExecuteQ(query, cnn);
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 && int.TryParse(ds.Tables[0].Rows[0][0].ToString(), out int idValue))
                        {
                            resultString = idValue.ToString();
                        }
                        else
                        {
                            resultBool = false;
                        }
                    }
                    else
                    {
                        foreach (DataRow dr in dt.Rows) 
                        {
                            if (dr[2].ToString()==cbx.Text)
                            {
                                resultString = dr[0].ToString();
                            }
                        }
                    }
                }
                else
                {
                    resultString = cbx.Text;
                }
            }
            else if (s.Contains(dgv.Columns[j].HeaderText))
            {
                resultBool = false;
            }
            return (resultString, resultBool);
        }
        private bool CorrectValue(string value, Type type)
        {
            if (type == typeof(int))
            {
                return int.TryParse(value, out int val);
            }
            else if (type == typeof(string))
            {
                return true;
            }
            else if (type == typeof(double))
            {
                return double.TryParse(value, out double val);
            }
            else if (type == typeof(decimal))
            {
                return decimal.TryParse(value, out decimal val);
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.TryParse(value, out DateTime val);
            }
            else if (type == typeof(bool))
            {
                return bool.TryParse(value, out bool val);
            }
            else
            {
                return false;
            }
        }
    }
}

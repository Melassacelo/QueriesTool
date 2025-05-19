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
            string columns = string.Join(", ", columnNames);
            string parameters = $"@NewIdQueries";
            foreach (string v in values)
            {
                if (string.IsNullOrEmpty(v))
                {
                    parameters += ", NULL";
                }
                else
                {
                    parameters += ", '" + v + "'";
                }
            }
            string queriesM1 = string.Join("\n", queriesM);
            string query = $"DECLARE @NewIdQueries int\n IF NOT EXISTS (\nSELECT 1  FROM [dbo].[{Tables_name[0]}] WHERE [Name]='{values[0]}'\n) BEGIN\n \tSET @NewIdQueries = (SELECT ISNULL(MAX({columnNames[0]}), 0) + 1 FROM [dbo].[{Tables_name[0]}])\n" +
                $" INSERT INTO [dbo].{Tables_name[0]} ({columns}) VALUES ({parameters});\n END\n" + queriesM1;
            return query;
        }
        public string DataGridViewScript(DataGridView dgv, List<string> Tables_name, List<string> strings, string columns2, int y)
        {
            string qM = "";
            if (dgv.Tag.ToString() == Tables_name[2])
            {
                qM = $"IF NOT EXISTS\n(SELECT 1 FROM [dbo].[{dgv.Tag.ToString()}] WHERE [Description]='{dgv.Rows[y].Cells[1].Value}') \nBEGIN\n";
            }
            string parameters2 = "";
            for (int k = 0; k < dgv.Rows[y].Cells.Count; k++)
            {
                if (dgv.Rows[y].Cells[k].Value.ToString() != "")
                {
                    parameters2 += ", '" + dgv.Rows[y].Cells[k].Value + "'";
                }
                else
                {
                    parameters2 += ", NULL";
                }

            }
            qM += $"INSERT INTO [dbo].[{dgv.Tag.ToString()}] ({columns2}) VALUES ((SELECT ISNULL(MAX({strings[0]}), 0) + 1 FROM [dbo].[{dgv.Tag.ToString()}]), @NewIdQueries{parameters2});\n";
            if (dgv.Tag.ToString() == Tables_name[2])
            {
                qM += $"END\n";
            }
            return qM;
        }

        public (string, bool) TextBoxScript(TextBox txt, DataGridView dgv, List<string> s, int j)
        {
            string resultString = null;
            bool resultBool = true;
            if (txt.Text != "")
            {
                resultString = txt.Text;
            }
            else if (s.Contains(dgv.Columns[j].HeaderText))
            {
                resultBool = false;
            }
            return (resultString, resultBool);
        }
        public (string, bool) ComboBoxScript(ComboBox cbx, LibraryQuery LQ, SqlConnection cnn, DataGridView dgv, List<string> s, int j)
        {
            string resultString = null;
            bool resultBool = true;
            if (cbx.Text != "")
            {
                if (cbx.Tag != null)
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
                    resultString = cbx.Text;
                }
            }
            else if (s.Contains(dgv.Columns[j].HeaderText))
            {
                resultBool = false;
            }
            return (resultString, resultBool);
        }
    }
}

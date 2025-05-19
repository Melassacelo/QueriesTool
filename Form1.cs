using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WA_Progetto
{
    public partial class Form1 : Form
    {
        static string Database_name = "QueriesDb2"; //viene utilizata nella stringa di connesione e nella scrittura della query
        static string Connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        static SqlConnection cnn = new SqlConnection(Connection);
        //Variabili globali
        List<string> Tables_name = new List<string>();
        static Form frm_Querie = null;
        static DataGridViewRow row = null;
        //libreria contenente le query utilizzate
        static LibraryQuery LQ = new LibraryQuery();

        public Form1()
        {
            InitializeComponent();
            Tables_name.Add("Queries");
            Tables_name.Add("Queries_CrossModules");
            Tables_name.Add("Queries_Parameter");
            string query = $"SELECT * FROM {Tables_name[0]}";
            dgv_Tabella.DataSource = LQ.ExecuteQ(query, cnn).Tables[0];
        }
        private void txb_searchBar_TextChanged(object sender, EventArgs e) //barra di ricerca per nome
        {
            string query = $"SELECT * FROM {Tables_name[0]} WHERE Name LIKE @searchText + '%'";
            dgv_Tabella.DataSource = LQ.ExecuteQWithParam(query, new SqlParameter("@searchText", txb_searchBar.Text), cnn).Tables[0];
        }
        private void txb_SearchId_TextChanged(object sender, EventArgs e) //barra di ricerca per id
        {
            string query = $"SELECT * FROM {Tables_name[0]} WHERE ID_Queries LIKE @searchId + '%'";
            dgv_Tabella.DataSource = LQ.ExecuteQWithParam(query, new SqlParameter("@searchId", txb_SearchId.Text), cnn).Tables[0];
        }
        private void dgv_Tabella_SelectionChanged(object sender, EventArgs e) //disattivazione e attivazione pulsante duplicazione
        {
            if (dgv_Tabella.SelectedRows.Count > 0)
            {
                btn_Duplicate.Enabled = true;
            }
            else
            {
                btn_Duplicate.Enabled = false;
            }
        }
        private void btn_Create_Form(object sender, EventArgs e) //metodo condiviso fra i pulsanti create new e duplicate
        {
            Button btnCreate = sender as Button;
            bool existing = btnCreate.Text == "Duplicate";
            row = dgv_Tabella.Rows[0];
            if (existing)
            {
                row = dgv_Tabella.Rows[dgv_Tabella.SelectedRows[0].Index];
            }
            frm_Querie = new Form //Creazione Form
            {
                Text = $"Creazione nuovo record",
                Size = new Size(750, 50 + 30 * row.Cells.Count),
                StartPosition = FormStartPosition.CenterParent
            };

            int y = 10;
            for (int i = 1; i < row.Cells.Count; i++)
            {
                Label lbl = new Label //Creazione LAbel
                {
                    Text = dgv_Tabella.Columns[i].HeaderText,
                    Location = new Point(10, y + 3),
                    AutoSize = true
                };
                frm_Querie.Controls.Add(lbl);
                if (row.Cells[i].ValueType == typeof(bool)) //Controllo tipologia dati
                {
                    ComboBox cb = new ComboBox //Creazione Combobox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Location = new Point(170, y),
                        Width = 240
                    };
                    cb.Items.Add("True");
                    cb.Items.Add("False");
                    if (existing)
                    {
                        cb.SelectedIndex = row.Cells[i].Value != null && (bool)row.Cells[i].Value ? 0 : 1;
                    }
                    frm_Querie.Controls.Add(cb);
                }
                else
                {
                    TextBox txt = new TextBox //Creazione TextBox
                    {
                        Text = "",
                        Location = new Point(170, y),
                        Width = 240,
                    };
                    if (existing)
                    {
                        txt.Text = row.Cells[i].Value.ToString();
                    }
                    frm_Querie.Controls.Add(txt);
                }

                y = y + 30;
            }
            DataGridView dgv1 = new DataGridView //Creazione datagridview per tabella Query_CrossModules
            {
                Tag = "Queries_CrossModules",
                Location = new Point(430, 10),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 50,
                DataSource = LQ.ExecuteQ($"SELECT Id_Module, [Order] FROM Queries_CrossModules WHERE ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM[{Database_name}].[dbo].[{Tables_name[0]}])", cnn).Tables[0]

            };
            dgv1.DoubleClick += ModuleCreation;
            dgv1.KeyDown += (s, ev) => RowElimination(s, ev, dgv1); //Creazione datagridview per tabella Query_parameters
            frm_Querie.Controls.Add(dgv1);
            DataGridView dgv2 = new DataGridView
            {
                Tag = "Queries_Parameter",
                Location = new Point(430, frm_Querie.Height / 2 - 30),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 50,
                DataSource = LQ.ExecuteQ($"SELECT Name, Description, Id_Queries_Parameter_Type, [Order], Id_Queries_Parameter_Relation, Active, Mandatory FROM Queries_Parameter WHERE ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM[{Database_name}].[dbo].[{Tables_name[0]}])", cnn).Tables[0]

            };
            dgv2.DoubleClick += ModuleCreation;
            dgv2.KeyDown += (s, ev) => RowElimination(s, ev, dgv2);
            frm_Querie.Controls.Add(dgv2);

            Button btn_Confirm = new Button //pulsante per la creazione del file.sql
            {
                Text = "Confirm",
                Location = new Point(10, frm_Querie.Height - 75),
                Width = 710,
            };
            btn_Confirm.Click += btn_Confirm_onClick;
            frm_Querie.Controls.Add(btn_Confirm);
            frm_Querie.ShowDialog(this);

        }
        private void btn_Confirm_onClick(object sender, EventArgs e) //metodo creazione file.sql
        {
            bool correct = true;
            List<string> columnNames = new List<string>();
            List<string> values = new List<string>();
            List<string> queriesM = new List<string>();
            columnNames.Add(dgv_Tabella.Columns[0].HeaderText);
            List<string> s = LQ.GetRequiredColumns(Tables_name[0], cnn);
            s.RemoveAt(0);
            int g = 1;
            //inserimento datatype
            for (int i = 0; i < frm_Querie.Controls.Count; i++) //recupero dati inseriti
            {
                if (frm_Querie.Controls[i] is Label lbl) //Label
                {
                    if (lbl.Text == "Order")
                    {
                        columnNames.Add("[" + lbl.Text + "]");
                        g++;
                    }
                    else
                    {
                        columnNames.Add(lbl.Text);
                        g++;
                    }
                }

                else if (frm_Querie.Controls[i] is TextBox txt) //TextBox
                {
                    if (txt.Text != "")
                    {
                        values.Add("'" + txt.Text + "'");
                    }
                    else if (s.Contains(columnNames[columnNames.Count - 1]))
                    {
                        correct = false;
                    }
                    else
                    {
                        columnNames.RemoveAt(columnNames.Count - 1);
                    }
                }

                else if (frm_Querie.Controls[i] is ComboBox cbx) //Combobox
                {
                    if (cbx.Text != "")
                    {
                        values.Add("'" + cbx.Text + "'");
                    }
                    else if (s.Contains(columnNames[columnNames.Count - 1]))
                    {
                        correct = false;
                    }
                    else
                    {
                        columnNames.RemoveAt(columnNames.Count - 1);
                    }
                }
                else if (frm_Querie.Controls[i] is DataGridView dgv) //Datagridview
                {
                    List<string> strings = LQ.GetAllColumnNames(dgv.Tag.ToString(), cnn);
                    string columns2 = string.Join(", ", strings);
                    for (int y = 0; y < dgv.Rows.Count - 1; y++)
                    {
                        string qM = "";
                        if (dgv.Tag.ToString() == Tables_name[2])
                        {
                            qM = $"IF NOT EXISTS\n(SELECT 1 FROM [{Database_name}].[dbo].[{dgv.Tag.ToString()}] WHERE [Description]='{dgv.Rows[y].Cells[1].Value}') \nBEGIN\n";
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
                        qM += $"INSERT INTO [{Database_name}].[dbo].[{dgv.Tag.ToString()}] ({columns2}) VALUES ((SELECT ISNULL(MAX({strings[0]}), 0) + 1 FROM [{Database_name}].[dbo].[{dgv.Tag.ToString()}]), @NewIdQueries{parameters2});\n";
                        if (dgv.Tag.ToString() == Tables_name[2])
                        {
                            qM += $"END\n";
                        }
                        queriesM.Add(qM);
                    }
                }
            }

            if (correct) //composizione e creazione file.sql
            {
                string columns = string.Join(", ", columnNames);
                string parameters = $"@NewIdQueries" + ", " + string.Join(", ", values);
                string queriesM1 = string.Join("\n", queriesM);
                string query = $"DECLARE @NewIdQueries int\n IF NOT EXISTS (\nSELECT 1  FROM [{Database_name}].[dbo].[{Tables_name[0]}] WHERE [Name]={values[0]}\n) BEGIN\n \tSET @NewIdQueries = (SELECT ISNULL(MAX({columnNames[0]}), 0) + 1 FROM [{Database_name}].[dbo].[{Tables_name[0]}])\n" +
                    $" INSERT INTO [{Database_name}].[dbo].{Tables_name[0]} ({columns}) VALUES ({parameters});\n END\n" + queriesM1;
                values[0] = values[0].Replace(" ", "_");
                string name = null;
                foreach (char c in values[0])
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                    {
                        name += c;
                    }
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(name + ".SQL"))
                    {
                        sw.WriteLine(query);
                    }
                    MessageBox.Show("SQL creata con successo");
                    frm_Querie.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore durante la creazione del file SQL: " + ex.Message);
                }

                frm_Querie.Close();
            }
            else //message di errore
            {
                string sd = string.Join(", ", s);
                MessageBox.Show($"Name must be new and {sd} are required");
            }

        }

        private void ModuleCreation(object sender, EventArgs e) //modulo per inserimento Queries_CrossModules e Queries_Parameters
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv != null)
            {
                Form frm_Module = new Form //Form
                {
                    Text = $"Creazione nuovo record",
                    Size = new Size(450, 100 + 30 * dgv.Rows[0].Cells.Count),
                    StartPosition = FormStartPosition.CenterParent
                };
                int y = 10;
                List<string> fkColumns = LQ.GetForeignKeyColumns(dgv.Tag.ToString(), cnn);
                for (int i = 0; i < dgv.Rows[0].Cells.Count; i++)
                {
                    Label lbl = new Label //Label
                    {
                        Text = dgv.Columns[i].HeaderText,
                        Location = new Point(10, y + 3),
                        AutoSize = true
                    };
                    frm_Module.Controls.Add(lbl);
                    if (fkColumns.Contains(dgv.Columns[i].HeaderText)) //Controllo campi associati a tabelle esterne
                    {

                        string refInfo = LQ.GetReferencedTable(dgv.Tag.ToString(), dgv.Columns[i].HeaderText, cnn);
                        string colum = LQ.GetAllColumnNames(refInfo, cnn)[1];
                        string query = $"SELECT {colum} FROM {refInfo}";
                        DataSet ds = LQ.ExecuteQ(query, cnn);
                        ComboBox cb = new ComboBox //Combobox campi esterni
                        {
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Location = new Point(170, y),
                            Width = 240,
                            Tag = refInfo
                        };

                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            cb.Items.Add(dr[0].ToString());
                        }
                        frm_Module.Controls.Add(cb);
                    }
                    else
                    {
                        if (dgv.Rows[0].Cells[i].ValueType == typeof(bool))
                        {
                            ComboBox cb = new ComboBox //combobox 
                            {
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                Location = new Point(170, y),
                                Width = 240
                            };
                            cb.Items.Add("True");
                            cb.Items.Add("False");
                            frm_Module.Controls.Add(cb);
                        }
                        else
                        {
                            TextBox txt = new TextBox //textbox
                            {
                                Text = "",
                                Location = new Point(170, y),
                                Width = 240,
                            };
                            frm_Module.Controls.Add(txt);
                        }
                    }
                    y = y + 30;
                }
                Button btn_ConfirmM = new Button //pulsante conferma
                {
                    Text = "Confirm",
                    Location = new Point(10, frm_Module.Height - 75),
                    Width = 410,
                };
                btn_ConfirmM.Click += (s, ev) => btn_ConfirmM_onClick(s, ev, frm_Module, dgv);
                frm_Module.Controls.Add(btn_ConfirmM);
                frm_Module.ShowDialog();
            }
        }
        private void RowElimination(object sender, KeyEventArgs e, DataGridView dgv) //Eliminazione Riga aggiunta
        {
            if (dgv.SelectedRows.Count > 0 && e.KeyCode == Keys.Back)
            {
                DataTable dt = dgv.DataSource as DataTable;
                dt.Rows.RemoveAt(dgv.SelectedRows[0].Index);
                dgv.DataSource = dt;
            }
        }

        private void btn_ConfirmM_onClick(object sender, EventArgs e, Form frm, DataGridView dgv) //conferma inserimento querie associata
        {
            DataTable dt = dgv.DataSource as DataTable;
            DataRow rowt = dt.NewRow();
            bool correct = true;
            int j = 0;
            List<string> s = LQ.GetRequiredColumns(dgv.Tag.ToString(), cnn);
            for (int i = 0; i < frm.Controls.Count; i++)
            {
                if (frm.Controls[i] is TextBox txt)
                {
                    if (txt.Text != "")
                    {
                        rowt[j] = txt.Text;
                        j++;
                    }
                    else if (s.Contains(dgv.Columns[j].HeaderText))
                    {
                        correct = false;
                    }
                    else
                    {
                        rowt[j] = DBNull.Value;
                        j++;
                    }
                }
                else if (frm.Controls[i] is ComboBox cbx)
                {
                    if (cbx.Text != "")
                    {
                        if (cbx.Tag != null)
                        {
                            string column1 = LQ.GetAllColumnNames(cbx.Tag.ToString(), cnn)[0];
                            string column2 = LQ.GetAllColumnNames(cbx.Tag.ToString(), cnn)[1];
                            string query = $"SELECT {column1} FROM {cbx.Tag.ToString()} WHERE {column2} = '{cbx.Text}'";
                            DataSet ds = LQ.ExecuteQ(query, cnn);
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                if (int.TryParse(ds.Tables[0].Rows[0][0].ToString(), out int idValue))
                                {
                                    rowt[j] = idValue;
                                }
                                else
                                {
                                    correct = false;
                                }
                            }
                            else
                            {
                                correct = false;
                            }
                        }
                        else
                        {
                            rowt[j] = cbx.Text;
                        }
                        j++;
                    }
                    else if (s.Contains(dgv.Columns[j].HeaderText))
                    {
                        correct = false;
                    }
                    else
                    {
                        rowt[j] = null;
                    }
                }
            }
            if (correct)
            {
                dt.Rows.Add(rowt);
                dgv.DataSource = dt;
            }
            else
            {
                MessageBox.Show(string.Join(", ", s) + "are required");
            }
        }
        private bool CanChangeType(string sourceType, Type targetType)
        {
            if (string.IsNullOrEmpty(sourceType))
                return false;

            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Boolean:
                    bool b;
                    return bool.TryParse(sourceType, out b);

                case TypeCode.Int32:
                    int i32;
                    return int.TryParse(sourceType, out i32);

                case TypeCode.Single:
                    float f;
                    return float.TryParse(sourceType, out f);

                case TypeCode.Double:
                    double d;
                    return double.TryParse(sourceType, out d);

                case TypeCode.DateTime:
                    DateTime dt;
                    return DateTime.TryParse(sourceType, out dt);

                case TypeCode.String:
                    return true;

                default:
                    return false;
            }
        }
    }
}

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
        static string Connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        static SqlConnection cnn = new SqlConnection(Connection);
        //Variabili globali
        List<string> Tables_name = new List<string>();
        //libreria contenente le query utilizzate
        static LibraryQuery LQ = new LibraryQuery();
        static LibraryScript LS = new LibraryScript();

        public Form1()
        {
            InitializeComponent();
            Tables_name.Add("Queries");
            Tables_name.Add("Queries_CrossModules");
            Tables_name.Add("Queries_Parameter");
            dgv_Tabella.DataSource = LQ.ExecuteQ($"SELECT * FROM {Tables_name[0]}", cnn).Tables[0];
        }
        private void txb_searchBar_TextChanged(object sender, EventArgs e) //barra di ricerca per nome
        {
            dgv_Tabella.DataSource = LQ.ExecuteQWithParam($"SELECT * FROM {Tables_name[0]} WHERE Name LIKE @searchText + '%'", new SqlParameter("@searchText", txb_searchBar.Text), cnn).Tables[0];
        }
        private void txb_SearchId_TextChanged(object sender, EventArgs e) //barra di ricerca per id
        {
            dgv_Tabella.DataSource = LQ.ExecuteQWithParam($"SELECT * FROM {Tables_name[0]} WHERE ID_Queries LIKE @searchId + '%'", new SqlParameter("@searchId", txb_SearchId.Text), cnn).Tables[0];
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
            DataGridViewRow row = dgv_Tabella.Rows[0];
            if (existing)
            {
                row = dgv_Tabella.Rows[dgv_Tabella.SelectedRows[0].Index];
            }
            Form frm_Querie = new Form //Creazione Form
            {
                Text = $"Creazione nuovo record",
                Size = new Size(750, 50 + 30 * row.Cells.Count),
                StartPosition = FormStartPosition.CenterParent
            };

            int y = 10;
            List<string> fkColumns = LQ.GetForeignKeyColumns(Tables_name[0], cnn);
            for (int i = 1; i < row.Cells.Count; i++)
            {
                Label lbl = new Label //Creazione LAbel
                {
                    Text = dgv_Tabella.Columns[i].HeaderText,
                    Location = new Point(10, y + 3),
                    AutoSize = true
                };
                frm_Querie.Controls.Add(lbl);
                frm_Querie.Controls.Add(CreateInputControl(row.Cells[i].ValueType, fkColumns, dgv_Tabella.Columns[i].HeaderText, null, row.Cells[i].Value, y, existing));

                y = y + 30;
            }
            DataGridView dgv1 = new DataGridView //Creazione datagridview per tabella Query_CrossModules
            {
                Tag = "Queries_CrossModules",
                Location = new Point(430, 10),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 50,
                DataSource = LQ.ExecuteQ($"SELECT Id_Module, [Order] FROM Queries_CrossModules WHERE ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM [dbo].[{Tables_name[0]}])", cnn).Tables[0]

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
                DataSource = LQ.ExecuteQ($"SELECT Name, Description, Id_Queries_Parameter_Type, [Order], Id_Queries_Parameter_Relation, Active, Mandatory FROM Queries_Parameter WHERE ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM [dbo].[{Tables_name[0]}])", cnn).Tables[0]

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
            btn_Confirm.Click += (s, ev) => btn_Confirm_onClick(s, ev, frm_Querie);
            frm_Querie.Controls.Add(btn_Confirm);
            frm_Querie.ShowDialog(this);

        }
        private void btn_Confirm_onClick(object sender, EventArgs e, Form frm_Querie) //metodo creazione file.sql
        {
            bool correct = true;
            List<string> columnNames = LQ.GetAllColumnNames(Tables_name[0], cnn);
            List<string> values = new List<string>();
            List<string> queriesM = new List<string>();
            List<string> s = LQ.GetRequiredColumns(Tables_name[0], cnn);
            s.RemoveAt(0);
            int j = 1;
            //inserimento datatype
            for (int i = 0; i < frm_Querie.Controls.Count; i++) //recupero dati inseriti
            {
                if (frm_Querie.Controls[i] is TextBox txt) //TextBox
                {
                    values.Add(LS.TextBoxScript(txt, dgv_Tabella, s, j).Item1);
                    correct = LS.TextBoxScript(txt, dgv_Tabella, s, j).Item2;
                    j++;
                }

                else if (frm_Querie.Controls[i] is ComboBox cbx) //Combobox
                {
                    values.Add(LS.ComboBoxScript(cbx, LQ, cnn, dgv_Tabella, s, j).Item1);
                    correct = LS.ComboBoxScript(cbx, LQ, cnn, dgv_Tabella, s, j).Item2;
                    j++;
                }
                else if (frm_Querie.Controls[i] is DataGridView dgv) //Datagridview
                {
                    List<string> strings = LQ.GetAllColumnNames(dgv.Tag.ToString(), cnn);
                    string columns2 = string.Join(", ", strings);
                    for (int y = 0; y < dgv.Rows.Count - 1; y++)
                    {
                        queriesM.Add(LS.DataGridViewScript(dgv, Tables_name, strings, columns2, y));
                    }
                }
            }

            if (correct) //composizione e creazione file.sql
            {
                string query = LS.FinalScript(Tables_name, values, columnNames, queriesM);
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
                    frm_Module.Controls.Add(CreateInputControl(dgv.Rows[0].Cells[i].ValueType, fkColumns, dgv.Columns[i].HeaderText, dgv.Tag.ToString(), dgv.Rows[0].Cells[i].Value, y,false));
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
                    rowt[j] = LS.TextBoxScript(txt, dgv, s, j).Item1;
                    correct = LS.TextBoxScript(txt, dgv, s, j).Item2;
                    j++;
                }
                else if (frm.Controls[i] is ComboBox cbx)
                {
                    rowt[j] = LS.ComboBoxScript(cbx, LQ, cnn, dgv, s, j).Item1;
                    correct = LS.ComboBoxScript(cbx, LQ, cnn, dgv, s, j).Item2;
                    j++;
                }
            }
            if (correct)
            {
                dt.Rows.Add(rowt);
                dgv.DataSource = dt;
            }
            else
            {
                MessageBox.Show(string.Join(", ", s) + " are required");
            }
        }
        private Control CreateInputControl(Type valueType, List<string> fkColumns, string Header, string tag,  object value, int y, bool existing)
        {

            if (fkColumns.Contains(Header)) //Controllo campi associati a tabelle esterne
            {

                string refInfo = LQ.GetReferencedTable(tag, Header, cnn);
                string colum = LQ.GetAllColumnNames(refInfo, cnn)[1];
                DataSet ds = LQ.ExecuteQ($"SELECT {colum} FROM {refInfo}", cnn);
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
                return cb;
            }
            else
            {
                if (valueType == typeof(bool)) //Controllo tipologia dati
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
                        cb.SelectedIndex = value != null && (bool)value ? 0 : 1;
                    }
                    return cb;
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
                        txt.Text = value.ToString();
                    }
                    return txt;
                }
            }
        }
    }
}

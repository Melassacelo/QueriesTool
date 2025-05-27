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
        int a = 1;
        //libreria contenente le query utilizzate
        static LibraryQuery LQ = new LibraryQuery();
        static LibraryScript LS = new LibraryScript();

        public Form1()
        {
            InitializeComponent();
            Tables_name.AddRange(new[] { "Queries", "Queries_CrossModules", "Queries_Parameter", "Queries_Parameter_Detail" });
            dgv_Tabella.DataSource = LQ.ExecuteQ($"SELECT * FROM {Tables_name[0]}", cnn).Tables[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txb_SearchId.Text) && !string.IsNullOrEmpty(txb_searchBar.Text))
            {
                dgv_Tabella.DataSource = LQ.ExecuteQ($"SELECT * FROM {Tables_name[0]} WHERE Name LIKE '%{txb_searchBar.Text}%' AND ID_Queries LIKE '%{txb_SearchId.Text}%'", cnn).Tables[0];
            }
            else if (!string.IsNullOrEmpty(txb_SearchId.Text))
            {
                dgv_Tabella.DataSource = LQ.ExecuteQWithParam($"SELECT * FROM {Tables_name[0]} WHERE ID_Queries LIKE '%' + @searchId + '%'", new SqlParameter("@searchId", txb_SearchId.Text), cnn).Tables[0];
            }
            else if (!string.IsNullOrEmpty(txb_searchBar.Text))
            {
                dgv_Tabella.DataSource = LQ.ExecuteQWithParam($"SELECT * FROM {Tables_name[0]} WHERE Name LIKE '%' + @searchText + '%'", new SqlParameter("@searchText", txb_searchBar.Text), cnn).Tables[0];
            }
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
                Size = new Size(1050, 50 + 30 * row.Cells.Count),
                StartPosition = FormStartPosition.CenterParent
            };
            int y = 10;
            List<string> fkColumns = LQ.GetForeignKeyColumns(Tables_name[0], cnn);
            if (fkColumns.Contains("ID_Agreement")) 
            {
                fkColumns.Remove("ID_Agreement");
            }
            for (int i = 1; i < row.Cells.Count; i++)
            {
                Label lbl = new Label //Creazione LAbel
                {
                    Text = dgv_Tabella.Columns[i].HeaderText,
                    Location = new Point(10, y + 3),
                    AutoSize = true,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top,
                };
                frm_Querie.Controls.Add(lbl);
                frm_Querie.Controls.Add(CreateInputControl(row.Cells[i].ValueType, fkColumns, dgv_Tabella.Columns[i].HeaderText, Tables_name[0], row.Cells[i].Value, y, existing));

                y = y + 30;
            }
            DataGridView dgv1 = new DataGridView //Creazione datagridview per tabella Query_CrossModules
            {
                Tag = "Queries_CrossModules",
                Location = new Point(430, 10),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 50,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
            };
            if (existing)
            {
                dgv1.DataSource = LQ.ExecuteQ($"SELECT * FROM Queries_CrossModules WHERE ID_Queries = {row.Cells[0].Value}", cnn).Tables[0];
            }
            else
            {
                dgv1.DataSource = LQ.ExecuteQ($"SELECT * FROM Queries_CrossModules WHERE ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM [dbo].[{Tables_name[0]}])", cnn).Tables[0];
            }
            dgv1.DoubleClick += (s, ev) => ModuleCreation(s, ev);
            dgv1.KeyDown += (s, ev) => RowElimination(s, ev, dgv1); //Creazione datagridview per tabella Query_parameters
            frm_Querie.Controls.Add(dgv1);
            DataGridView dgv2 = new DataGridView
            {
                Tag = "Queries_Parameter",
                Location = new Point(430, frm_Querie.Height / 2 - 30),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 50,
                DataSource = LQ.ExecuteQ($"SELECT * FROM Queries_Parameter WHERE ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM [dbo].[{Tables_name[0]}])", cnn).Tables[0],
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
            };
            if (existing)
            {
                dgv2.DataSource = LQ.ExecuteQ($"SELECT * FROM Queries_Parameter WHERE ID_Queries = {row.Cells[0].Value}", cnn).Tables[0];
            }
            else
            {
                dgv2.DataSource = LQ.ExecuteQ($"SELECT * FROM Queries_Parameter WHERE ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM [dbo].[{Tables_name[0]}])", cnn).Tables[0];
            }
            dgv2.DoubleClick += (s, ev) => ModuleCreation(s, ev);
            dgv2.KeyDown += (s, ev) => RowElimination(s, ev, dgv2);
            frm_Querie.Controls.Add(dgv2);

            DataGridView dgv3 = new DataGridView
            {
                Tag = "Queries_Parameter_Detail",
                Location = new Point(730, 10),
                Width = 290,
                ReadOnly = true,
                Height = frm_Querie.Height - 90,
                DataSource = LQ.ExecuteQ($"SELECT * FROM Queries_Parameter_Detail WHERE ID_Queries_Parameter = (SELECT ISNULL(MAX(ID_Queries_Parameter),0) +1 FROM [dbo].[{Tables_name[2]}])", cnn).Tables[0],
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
            };
            if (existing)
            {
                DataTable dt = new DataTable();
                DataTable dt2 = dgv2.DataSource as DataTable;
                foreach (DataRow drv in dt2.Rows)
                {
                    DataTable dt1 = LQ.ExecuteQ($"SELECT * FROM Queries_Parameter_Detail WHERE ID_Queries_Parameter = {drv[0]}", cnn).Tables[0];
                    dt.Merge(dt1);
                }
                dgv3.DataSource = dt;
            }
            else
            {
                dgv3.DataSource = LQ.ExecuteQ($"SELECT * FROM Queries_Parameter_Detail WHERE ID_Queries_Parameter = (SELECT ISNULL(MAX(ID_Queries_Parameter),0) +1 FROM [dbo].[{Tables_name[2]}])", cnn).Tables[0];
            }
            dgv3.DoubleClick += (s, ev) => ModuleCreation(s,ev, dgv2.DataSource as DataTable);
            dgv3.KeyDown += (s, ev) => RowElimination(s, ev, dgv3);
            frm_Querie.Controls.Add(dgv3);
            dgv2.SelectionChanged += (s, ev) => SelectionDetail(s, ev, dgv3);
            Button btn_Confirm = new Button //pulsante per la creazione del file.sql
            {
                Text = "Generate .sql script",
                Location = new Point(10, frm_Querie.Height - 75),
                Width = 1010,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Top,
            };
            btn_Confirm.Click += (s, ev) => btn_Confirm_onClick(s, ev, frm_Querie);
            frm_Querie.Controls.Add(btn_Confirm);
            frm_Querie.Load += FormLoad;
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
            for (int i = 0; i < frm_Querie.Controls.Count && correct; i++) //recupero dati inseriti
            {
                if (frm_Querie.Controls[i] is TextBox txt) //TextBox
                {
                    values.Add(LS.TextBoxScript(txt, dgv_Tabella, s, j).Item1.ToString());
                    correct = LS.TextBoxScript(txt, dgv_Tabella, s, j).Item2;
                    j++;
                }

                else if (frm_Querie.Controls[i] is ComboBox cbx) //Combobox
                {
                    values.Add(LS.ComboBoxScript(cbx, LQ, cnn, dgv_Tabella, s, j, Tables_name).Item1.ToString());
                    correct = LS.ComboBoxScript(cbx, LQ, cnn, dgv_Tabella, s, j, Tables_name).Item2;
                    j++;
                }
                else if (frm_Querie.Controls[i] is DataGridView dgv) //Datagridview
                {
                    DataTable dt = dgv.DataSource as DataTable;
                    dt.DefaultView.RowFilter = "";
                    List<string> strings = LQ.GetAllColumnNames(dgv.Tag.ToString(), cnn);
                    string columns2 = string.Join(", ", strings);
                    if (dgv.Tag.ToString() != Tables_name[3])
                    {
                        for (int y = 0; y < dgv.Rows.Count - 1; y++)
                        {
                            queriesM.Add(LS.DataGridViewScript(dgv, Tables_name, strings, columns2, y));
                            if (dgv.Tag.ToString() == Tables_name[2])
                            {
                                DataGridView dgv1 = frm_Querie.Controls[i+1] as DataGridView;
                                DataTable dt1 = dgv1.DataSource as DataTable;
                                dt1.DefaultView.RowFilter = $"{dgv1.Columns[1].HeaderText} = '{dgv.Rows[y].Cells[0].Value}'";
                                for (int x = 0; x < dgv1.Rows.Count -1; x++)
                                {
                                    queriesM.Add(LS.DataGridViewScript(dgv1, Tables_name, LQ.GetAllColumnNames(dgv1.Tag.ToString(), cnn), string.Join(", ", LQ.GetAllColumnNames(dgv1.Tag.ToString(), cnn)), x));
                                }
                            }
                        }
                    }
                }
            }

            if (correct) //composizione e creazione file.sql
            {
                string query = LS.FinalScript(Tables_name, values, columnNames, queriesM);
                values[0] = values[0].Replace(" ", "_");
                string name = SanitizeFileName(values[0].Replace(" ", "_"));
                try
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "SQL files (*.sql)|*.sql|All files (*.*)|*.*";
                        saveFileDialog.FileName = name + ".sql";
                        saveFileDialog.Title = "Scegli dove salvare il file SQL";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                            {
                                sw.WriteLine(query);
                            }
                            MessageBox.Show("SQL creata con successo");
                            frm_Querie.Close();
                        }
                    }
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

        private void ModuleCreation(object sender, EventArgs e, DataTable dt = null) //modulo per inserimento Queries_CrossModules e Queries_Parameters
        {
            DataGridView dgv = sender as DataGridView;
            DataGridViewRow dgvr = dgv.Rows[0];
            bool existing = false;
            if (dgv.SelectedRows.Count > 0)
            {
                dgvr = dgv.SelectedRows[0];
                if (!dgv.SelectedRows[0].IsNewRow)
                {
                    existing = true;
                }
            }
            if (dgv != null)
            {
                int n = 2;
                if (dgv.Tag.ToString() == Tables_name[3])
                {
                    n = 1;
                }
                Form frm_Module = new Form //Form
                {
                    Text = $"Creazione nuovo record",
                    Size = new Size(450, 100 + 30 * (dgvr.Cells.Count-n)),
                    StartPosition = FormStartPosition.CenterParent
                };
                int y = 10;
                List<string> fkColumns = LQ.GetForeignKeyColumns(dgv.Tag.ToString(), cnn);

                for (int i = n; i < dgvr.Cells.Count; i++)
                {
                    Label lbl = new Label //Label
                    {
                        Text = dgv.Columns[i].HeaderText,
                        Location = new Point(10, y + 3),
                        AutoSize = true
                    };
                    frm_Module.Controls.Add(lbl);
                    frm_Module.Controls.Add(CreateInputControl(dgvr.Cells[i].ValueType, fkColumns, dgv.Columns[i].HeaderText, dgv.Tag.ToString(), dgv.Rows[0].Cells[i].Value, y, existing,dt));
                    y = y + 30;
                }
                Button btn_ConfirmM = new Button //pulsante conferma
                {
                    Text = "Confirm",
                    Location = new Point(10, frm_Module.Height - 75),
                    Width = 410,
                };
                btn_ConfirmM.Click += (s, ev) => btn_ConfirmM_onClick(s, ev, frm_Module, dgv, dt);
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
                if (dgv.Tag.ToString() == Tables_name[2])
                {
                    a--;
                }
            }
        }

        private void btn_ConfirmM_onClick(object sender, EventArgs e, Form frm, DataGridView dgv, DataTable dt1) //conferma inserimento querie associata
        {
            DataTable dt = dgv.DataSource as DataTable;
            DataRow rowt = dt.NewRow();
            bool correct = true;
            int j = 2;
            if (dgv.Tag.ToString() == Tables_name[3])
            {
                j = 1;
            }
            List<string> s = LQ.GetRequiredColumns(dgv.Tag.ToString(), cnn);
            for (int i = 0; i < frm.Controls.Count && correct; i++)
            {
                if (frm.Controls[i] is TextBox txt)
                {
                    var result = LS.TextBoxScript(txt, dgv, s, j);
                    rowt[j] = result.Item1;
                    correct = result.Item2;
                    j++;
                }
                else if (frm.Controls[i] is ComboBox cbx)
                {
                    var result = LS.ComboBoxScript(cbx, LQ, cnn, dgv, s, j, Tables_name, dt1);
                    rowt[j] = result.Item1;
                    correct = result.Item2;
                    j++;
                }
            }
            if (correct)
            {
                if (dgv.SelectedRows.Count>0)
                {
                    rowt[0] = dgv.SelectedRows[0].Cells[0].Value;
                    dt.Rows.RemoveAt(dgv.SelectedRows[0].Index);
                }
                else if (dgv.Tag.ToString() == Tables_name[2])
                {
                    rowt[0] = Convert.ToInt32(LQ.ExecuteQ($"SELECT ISNULL(MAX(Id_Queries_Parameter), 0) + {a} FROM [dbo].[{dgv.Tag.ToString()}]", cnn).Tables[0].Rows[0][0]);
                    a++;
                }
                dt.Rows.Add(rowt);
                dgv.DataSource = dt;
                frm.Close();
            }
            else
            {
                MessageBox.Show(string.Join(", ", s) + " are required");
            }
        }
        private Control CreateInputControl(Type valueType, List<string> fkColumns, string Header, string tag,  object value, int y, bool existing, DataTable dt=null)
        {

            if (fkColumns.Contains(Header) && !string.IsNullOrEmpty(tag)) //Controllo campi associati a tabelle esterne
            {
                string refInfo = LQ.GetReferencedTable(tag, Header, cnn);
                ComboBox cb = new ComboBox //Combobox campi esterni
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(170, y),
                    Width = 240,
                    Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
                    Tag = refInfo

                };
                if (Tables_name.Contains(refInfo) && dt !=null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        cb.Items.Add(dr[2]);
                    }
                }
                else
                {
                    string colum = LQ.GetAllColumnNames(refInfo, cnn)[1];
                    DataSet ds = LQ.ExecuteQ($"SELECT {colum} FROM {refInfo}", cnn);
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        cb.Items.Add(dr[0].ToString());
                    }
                    if (existing)
                    {
                        var ds1 = LQ.ExecuteQ($"SELECT {colum} FROM {refInfo} WHERE {Header} = '{value.ToString()}'", cnn);
                        if (ds1.Tables[0].Rows.Count > 0)
                        {
                            cb.SelectedItem = ds1.Tables[0].Rows[0][0].ToString();
                        }
                    }
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
                        Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
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
                        Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top,
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
        private void SelectionDetail(object sender, EventArgs e, DataGridView dgv)
        {
            DataGridView dgv2 = (DataGridView)sender;
            DataTable dt = dgv.DataSource as DataTable;
            if (dgv2.SelectedRows.Count>0 && dgv2.SelectedRows[0].Index < dgv2.Rows.Count-1)
            {
                dt.DefaultView.RowFilter = $"{dgv.Columns[1].HeaderText} = '{dgv2.SelectedRows[0].Cells[0].Value}'";
            }
            else
            {
                dt.DefaultView.RowFilter = "";
            }
        }
        private void FormLoad(object sender, EventArgs e)
        {
            Form frm = sender as Form;
            foreach (Control dgv in frm.Controls)
            {
                if (dgv is DataGridView dataGridView)
                {
                    if (dataGridView.Tag.ToString() == Tables_name[3])
                    {
                        dataGridView.Columns[0].Visible = false;
                    }
                    else
                    {
                        dataGridView.Columns[0].Visible = false;
                        dataGridView.Columns[1].Visible = false;
                    }
                }
            }
        }
        private string SanitizeFileName(string input)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}

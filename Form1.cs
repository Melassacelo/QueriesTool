using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PoorMansTSqlFormatterRedux;

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
                Size = new Size(1150, 50 + 30 * row.Cells.Count),
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
            Label lbl1 = new Label
            {
                Text = "Edit Existing Row: Select a row in the DataGridView and double-click on it to open the form.\nAdd New Row: Double-click on the header of the DataGridView to open the form",
                Location = new Point(430, 5),
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Microsoft Sans Serif", 6.5F, FontStyle.Regular, GraphicsUnit.Point)
            };
            frm_Querie.Controls.Add(lbl1);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            DataGridView dgv1 = new DataGridView //Creazione datagridview per tabella Query_CrossModules
            {
                Tag = "Queries_CrossModules",
                Location = new Point(430, 30),
                Width = 340,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 60,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left,
                MultiSelect = false,
                AllowUserToAddRows = false,
            };
            string q1 = BuildSelectWithJoins(dgv1.Tag.ToString(), cnn, LQ, 1, $"{Tables_name[0]}.ID_Queries = (SELECT ISNULL(MAX(Id_Queries), 0) + 1 FROM[dbo].[{Tables_name[0]}])");
            if (existing)
            {
                q1 = BuildSelectWithJoins(dgv1.Tag.ToString(), cnn, LQ, 1, $"{Tables_name[0]}.ID_Queries = {row.Cells[0].Value}");
            }
            DataTable dtdgv1 = LQ.ExecuteQ(q1, cnn).Tables[0];
            dgv1.DataSource = dtdgv1;
            dgv1.DoubleClick += (s, ev) => ModuleCreation(s, ev);
            dgv1.KeyDown += (s, ev) => RowElimination(s, ev, dgv1); //Creazione datagridview per tabella Query_parameters
            frm_Querie.Controls.Add(dgv1);
            //////////////////////////////////////////////////////////////////////////////////////
            DataGridView dgv2 = new DataGridView
            {
                Tag = "Queries_Parameter",
                Location = new Point(430, frm_Querie.Height / 2 - 20),
                Width = 340,
                ReadOnly = true,
                Height = frm_Querie.Height / 2 - 60,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                MultiSelect = false,
                AllowUserToAddRows = false,
            };
            string q2 = BuildSelectWithJoins(dgv2.Tag.ToString(), cnn, LQ, 1, $"{Tables_name[0]}.ID_Queries = (SELECT ISNULL(MAX(Id_Queries),0) +1 FROM [dbo].[{Tables_name[0]}])");
            if (existing)
            {
                q2 = BuildSelectWithJoins(dgv2.Tag.ToString(), cnn, LQ, 1, $"{Tables_name[0]}.ID_Queries = {row.Cells[0].Value}");
            }
            DataTable dtdgv2 = LQ.ExecuteQ(q2, cnn).Tables[0];
            dgv2.DataSource = dtdgv2;
            dgv2.KeyDown += (s, ev) => RowElimination(s, ev, dgv2);
            frm_Querie.Controls.Add(dgv2);
            ////////////////////////////////////////////////////////////////////////////////////////////
            DataGridView dgv3 = new DataGridView
            {
                Tag = "Queries_Parameter_Detail",
                Location = new Point(780, 30),
                Width = 340,
                ReadOnly = true,
                Height = frm_Querie.Height - 110,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                MultiSelect = false,
                AllowUserToAddRows = false,
            };
            string q3 = BuildSelectWithJoins(dgv3.Tag.ToString(), cnn, LQ, 2, $"{Tables_name[2]}.ID_Queries_Parameter = (SELECT ISNULL(MAX(ID_Queries_Parameter),0) +1 FROM [dbo].[{Tables_name[2]}])");
            DataTable dt = new DataTable();
            if (existing)
            {
                DataTable dt2 = dgv2.DataSource as DataTable;
                foreach (DataRow drv in dt2.Rows)
                {
                    q3 = BuildSelectWithJoins(dgv3.Tag.ToString(), cnn, LQ, 2, $"{Tables_name[2]}.ID_Queries_Parameter = {drv[0]}");
                    DataTable dt1 = LQ.ExecuteQ(q3, cnn).Tables[0];
                    dt.Merge(dt1);
                }
            }
            else
            {
                dt = LQ.ExecuteQ(q3, cnn).Tables[0];
            }
            dgv3.DataSource = dt;
            dgv3.DoubleClick += (s, ev) => ModuleCreation(s, ev, dgv2.DataSource as DataTable);
            dgv3.KeyDown += (s, ev) => RowElimination(s, ev, dgv3);
            frm_Querie.Controls.Add(dgv3);
            dgv2.SelectionChanged += (s, ev) => SelectionDetail(s, ev, dgv3);
            dgv2.DoubleClick += (s, ev) => ModuleCreation(s, ev, null, dgv3);
            Button btn_Confirm = new Button //pulsante per la creazione del file.sql
            {
                Text = "Generate .sql script",
                Location = new Point(10, frm_Querie.Height - 75),
                Width = 1110,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left,
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
                    string columns2 = string.Join(",\n\t\t\t", strings);
                    if (dgv.Tag.ToString() != Tables_name[3])
                    {
                        for (int y = 0; y < dgv.Rows.Count; y++)
                        {
                            queriesM.Add(LS.DataGridViewScript(dgv, Tables_name, strings, columns2, y, LQ, cnn));
                            if (dgv.Tag.ToString() == Tables_name[2])
                            {
                                DataGridView dgv1 = frm_Querie.Controls[i + 1] as DataGridView;
                                DataTable dt1 = dgv1.DataSource as DataTable;
                                dt1.DefaultView.RowFilter = $"{dgv1.Columns[1].HeaderText} = '{dgv.Rows[y].Cells[2].Value}'";
                                for (int x = 0; x < dgv1.Rows.Count; x++)
                                {
                                    queriesM.Add(LS.DataGridViewScript(dgv1, Tables_name, LQ.GetAllColumnNames(dgv1.Tag.ToString(), cnn), string.Join(", ", LQ.GetAllColumnNames(dgv1.Tag.ToString(), cnn)), x, LQ, cnn));
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
                string formatted = SqlFormattingManager.DefaultFormat(query);
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
                                sw.WriteLine(formatted);
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

        private void ModuleCreation(object sender, EventArgs e, DataTable dt = null, DataGridView dv = null) //modulo per inserimento Queries_CrossModules e Queries_Parameters
        {
            DataGridView dgv = sender as DataGridView;
            DataGridViewRow dgvr = null;
            bool existing = false;

            // Controllo se la DataGridView ha almeno una riga
            if (dgv.Rows.Count > 0)
            {
                dgvr = dgv.Rows[0];
                if (dgv.SelectedRows.Count > 0)
                {
                    dgvr = dgv.SelectedRows[0];
                    if (!dgv.SelectedRows[0].IsNewRow)
                    {
                        existing = true;
                    }
                }
            }
            else
            {
                // Se la DataGridView è vuota, crea una DataGridViewRow fittizia per ottenere il numero di colonne
                dgvr = dgv.Columns.Count > 0 ? dgv.RowTemplate : null;
                existing = false;
            }

            if (dgvr != null)
            {
                int n = 2;
                if (dgv.Tag.ToString() == Tables_name[3])
                {
                    n = 1;
                }
                int numControls = dgvr.Cells.Count > 0 ? dgvr.Cells.Count : dgv.Columns.Count;
                Form frm_Module = new Form //Form
                {
                    Text = $"Creazione nuovo record",
                    Size = new Size(450, 100 + 30 * (numControls - n)),
                    StartPosition = FormStartPosition.CenterParent
                };
                int y = 10;
                List<string> fkColumns = LQ.GetForeignKeyColumns(dgv.Tag.ToString(), cnn);

                for (int i = n; i < numControls; i++)
                {
                    string headerText = dgv.Columns[i].HeaderText;
                    object cellValue = existing ? dgvr.Cells[i].Value : null;
                    Type valueType = dgv.Columns[i].ValueType ?? typeof(string);

                    Label lbl = new Label //Label
                    {
                        Text = headerText,
                        Location = new Point(10, y + 3),
                        AutoSize = true
                    };
                    frm_Module.Controls.Add(lbl);
                    frm_Module.Controls.Add(CreateInputControl(valueType, fkColumns, headerText, dgv.Tag.ToString(), cellValue, y, existing, dt));
                    y = y + 30;
                }
                Button btn_ConfirmM = new Button //pulsante conferma
                {
                    Text = "Confirm",
                    Location = new Point(10, frm_Module.Height - 75),
                    Width = 410,
                };
                btn_ConfirmM.Click += (s, ev) => btn_ConfirmM_onClick(s, ev, frm_Module, dgv, dt, dv);
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

        private void btn_ConfirmM_onClick(object sender, EventArgs e, Form frm, DataGridView dgv, DataTable dt1, DataGridView dv = null) //conferma inserimento querie associata
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
                if (dgv.SelectedRows.Count > 0 && !dgv.SelectedRows[0].IsNewRow)
                {
                    rowt[0] = dgv.SelectedRows[0].Cells[0].Value;
                    if (dgv.Tag.ToString() == Tables_name[2])
                    {
                        DataTable dataTable = dv.DataSource as DataTable;
                        if (dataTable != null)
                        {
                            foreach (DataGridViewRow row in dv.Rows)
                            {
                                if (!row.IsNewRow)
                                {
                                    if (row.Cells[1].Value.ToString() == dgv.SelectedRows[0].Cells[2].Value.ToString())
                                    {
                                        // Trova la riga corrispondente nella DataTable e aggiorna il valore
                                        int rowIndex = row.Index;
                                        dataTable.Rows[rowIndex][1] = rowt[2];
                                        // Aggiorna anche la cella della DataGridView per coerenza visiva
                                        row.Cells[1].Value = rowt[2];
                                    }
                                }
                            }
                        }
                    }
                    dt.Rows.RemoveAt(dgv.SelectedRows[0].Index);
                }
                else if (dgv.Tag.ToString() == Tables_name[2])
                {
                    rowt[0] = Convert.ToInt32(LQ.ExecuteQ($"SELECT ISNULL(MAX(Id_Queries_Parameter), 0) + {a} FROM [dbo].[{dgv.Tag.ToString()}]", cnn).Tables[0].Rows[0][0]);
                    a++;
                }
                dt.Rows.Add(rowt);
                frm.Close();
            }
            else
            {
                MessageBox.Show(string.Join(", ", s) + " are required");
            }
        }
        private Control CreateInputControl(Type valueType, List<string> fkColumns, string Header, string tag, object value, int y, bool existing, DataTable dt = null)
        {

            if (fkColumns.Contains(Header) && !string.IsNullOrEmpty(tag)) //Controllo campi associati a tabelle esterne
            {
                string refInfo = LQ.GetReferencedTable(tag, Header, cnn);
                ComboBox cb = new ComboBox //Combobox campi esterni
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(170, y),
                    Width = 240,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Tag = refInfo

                };
                if (Tables_name.Contains(refInfo) && dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        cb.Items.Add(dr[2]);
                    }
                    if (existing)
                    {
                        cb.SelectedItem = value;
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
                        var ds1 = LQ.ExecuteQ($"SELECT {colum} FROM {refInfo} WHERE {Header} = '{cb.Items.IndexOf(value.ToString()) + 1}'", cnn);
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
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
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
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
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
            if (dgv2.SelectedRows.Count > 0 && !dgv2.SelectedRows[0].IsNewRow)
            {
                dt.DefaultView.RowFilter = $"{dgv.Columns[1].HeaderText} = '{dgv2.SelectedRows[0].Cells[2].Value}'";
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

        private string BuildSelectWithJoins(string tableName, SqlConnection cnn, LibraryQuery lq, int o, string condizione = null)
        {
            // Recupera tutte le colonne della tabella  
            List<string> allColumns = lq.GetAllColumnNames(tableName, cnn);
            // Recupera le colonne che sono chiavi esterne  
            List<string> fkColumns = lq.GetForeignKeyColumns(tableName, cnn);

            // Lista per le colonne da selezionare  
            List<string> selectColumns = new List<string>();
            // Lista per le join  
            List<string> joins = new List<string>();

            foreach (var col in allColumns)
            {
                if (fkColumns.Contains(col))
                {
                    // Trova la tabella referenziata  
                    string refTable = lq.GetReferencedTable(tableName, col, cnn);
                    // Prende la seconda colonna della tabella referenziata  
                    List<string> refColumns = lq.GetAllColumnNames(refTable, cnn);
                    if (refColumns.Count < 2)
                        continue; // Salta se la tabella referenziata ha meno di 2 colonne  

                    string refCol = refColumns[o];

                    // Aggiunge la colonna della tabella referenziata al SELECT  
                    selectColumns.Add($"{refTable}.{refCol} AS [{col}]");

                    // Costruisce la JOIN  
                    joins.Add($"INNER JOIN {refTable} ON {tableName}.{col} = {refTable}.{refColumns[0]}");
                }
                else
                {
                    // Colonna normale della tabella principale  
                    selectColumns.Add($"{tableName}.{col}");
                }
            }

            // Costruisce la query finale  
            string selectClause = string.Join(", ", selectColumns);
            string joinClause = string.Join(" ", joins);

            string q = $"SELECT {selectClause} FROM {tableName} {joinClause}";
            if (!string.IsNullOrWhiteSpace(condizione))
            {
                q += $" WHERE {condizione}";
            }
            return q;
        }

    }
}

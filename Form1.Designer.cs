namespace WA_Progetto
{
    partial class Form1
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgv_Tabella = new System.Windows.Forms.DataGridView();
            this.txb_searchBar = new System.Windows.Forms.TextBox();
            this.txb_SearchId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_Add = new System.Windows.Forms.Button();
            this.btn_Duplicate = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Tabella)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_Tabella
            // 
            this.dgv_Tabella.AllowUserToAddRows = false;
            this.dgv_Tabella.AllowUserToDeleteRows = false;
            this.dgv_Tabella.AllowUserToResizeColumns = false;
            this.dgv_Tabella.AllowUserToResizeRows = false;
            this.dgv_Tabella.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_Tabella.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Tabella.Location = new System.Drawing.Point(14, 91);
            this.dgv_Tabella.Name = "dgv_Tabella";
            this.dgv_Tabella.ReadOnly = true;
            this.dgv_Tabella.RowHeadersWidth = 62;
            this.dgv_Tabella.RowTemplate.Height = 28;
            this.dgv_Tabella.Size = new System.Drawing.Size(1158, 560);
            this.dgv_Tabella.TabIndex = 0;
            this.dgv_Tabella.Tag = "Queries";
            this.dgv_Tabella.SelectionChanged += new System.EventHandler(this.dgv_Tabella_SelectionChanged);
            // 
            // txb_searchBar
            // 
            this.txb_searchBar.Location = new System.Drawing.Point(134, 36);
            this.txb_searchBar.Name = "txb_searchBar";
            this.txb_searchBar.Size = new System.Drawing.Size(117, 26);
            this.txb_searchBar.TabIndex = 1;
            // 
            // txb_SearchId
            // 
            this.txb_SearchId.Location = new System.Drawing.Point(14, 36);
            this.txb_SearchId.Name = "txb_SearchId";
            this.txb_SearchId.Size = new System.Drawing.Size(98, 26);
            this.txb_SearchId.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "search by ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(130, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "search by name";
            // 
            // btn_Add
            // 
            this.btn_Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Add.Location = new System.Drawing.Point(1071, 13);
            this.btn_Add.Name = "btn_Add";
            this.btn_Add.Size = new System.Drawing.Size(101, 59);
            this.btn_Add.TabIndex = 5;
            this.btn_Add.Text = "Create New";
            this.btn_Add.UseVisualStyleBackColor = true;
            this.btn_Add.Click += new System.EventHandler(this.btn_Create_Form);
            // 
            // btn_Duplicate
            // 
            this.btn_Duplicate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Duplicate.Enabled = false;
            this.btn_Duplicate.Location = new System.Drawing.Point(964, 13);
            this.btn_Duplicate.Name = "btn_Duplicate";
            this.btn_Duplicate.Size = new System.Drawing.Size(101, 59);
            this.btn_Duplicate.TabIndex = 6;
            this.btn_Duplicate.Text = "Duplicate";
            this.btn_Duplicate.UseVisualStyleBackColor = true;
            this.btn_Duplicate.Click += new System.EventHandler(this.btn_Create_Form);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(272, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 59);
            this.button1.TabIndex = 7;
            this.button1.Text = "Search";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 663);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_Duplicate);
            this.Controls.Add(this.btn_Add);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txb_SearchId);
            this.Controls.Add(this.txb_searchBar);
            this.Controls.Add(this.dgv_Tabella);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Tabella)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_Tabella;
        private System.Windows.Forms.TextBox txb_searchBar;
        private System.Windows.Forms.TextBox txb_SearchId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_Add;
        private System.Windows.Forms.Button btn_Duplicate;
        private System.Windows.Forms.Button button1;
    }
}


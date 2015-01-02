namespace Aventura_RPG
{
    partial class AventuraRPG
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblVida = new System.Windows.Forms.Label();
            this.lblOuro = new System.Windows.Forms.Label();
            this.lblExperiencia = new System.Windows.Forms.Label();
            this.lblLevel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxArmas = new System.Windows.Forms.ComboBox();
            this.comboBoxPoçoes = new System.Windows.Forms.ComboBox();
            this.buttonUsarArma = new System.Windows.Forms.Button();
            this.buttonUsarPoçao = new System.Windows.Forms.Button();
            this.buttonNorte = new System.Windows.Forms.Button();
            this.buttonLeste = new System.Windows.Forms.Button();
            this.buttonSul = new System.Windows.Forms.Button();
            this.buttonOeste = new System.Windows.Forms.Button();
            this.richTextBoxLocal = new System.Windows.Forms.RichTextBox();
            this.richTextBoxMensagens = new System.Windows.Forms.RichTextBox();
            this.dataGridViewInventario = new System.Windows.Forms.DataGridView();
            this.dataGridViewQuests = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewInventario)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewQuests)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Vida:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Ouro:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Experiencia:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Level:";
            // 
            // lblVida
            // 
            this.lblVida.AutoSize = true;
            this.lblVida.Location = new System.Drawing.Point(110, 19);
            this.lblVida.Name = "lblVida";
            this.lblVida.Size = new System.Drawing.Size(0, 13);
            this.lblVida.TabIndex = 8;
            // 
            // lblOuro
            // 
            this.lblOuro.AutoSize = true;
            this.lblOuro.Location = new System.Drawing.Point(110, 45);
            this.lblOuro.Name = "lblOuro";
            this.lblOuro.Size = new System.Drawing.Size(0, 13);
            this.lblOuro.TabIndex = 10;
            // 
            // lblExperiencia
            // 
            this.lblExperiencia.AutoSize = true;
            this.lblExperiencia.Location = new System.Drawing.Point(110, 73);
            this.lblExperiencia.Name = "lblExperiencia";
            this.lblExperiencia.Size = new System.Drawing.Size(0, 13);
            this.lblExperiencia.TabIndex = 12;
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Location = new System.Drawing.Point(110, 99);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(0, 13);
            this.lblLevel.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(617, 531);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Escolha ação";
            // 
            // comboBoxArmas
            // 
            this.comboBoxArmas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxArmas.FormattingEnabled = true;
            this.comboBoxArmas.Location = new System.Drawing.Point(369, 559);
            this.comboBoxArmas.Name = "comboBoxArmas";
            this.comboBoxArmas.Size = new System.Drawing.Size(121, 21);
            this.comboBoxArmas.TabIndex = 16;
            // 
            // comboBoxPoçoes
            // 
            this.comboBoxPoçoes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPoçoes.FormattingEnabled = true;
            this.comboBoxPoçoes.Location = new System.Drawing.Point(369, 593);
            this.comboBoxPoçoes.Name = "comboBoxPoçoes";
            this.comboBoxPoçoes.Size = new System.Drawing.Size(121, 21);
            this.comboBoxPoçoes.TabIndex = 17;
            // 
            // buttonUsarArma
            // 
            this.buttonUsarArma.Location = new System.Drawing.Point(620, 559);
            this.buttonUsarArma.Name = "buttonUsarArma";
            this.buttonUsarArma.Size = new System.Drawing.Size(75, 23);
            this.buttonUsarArma.TabIndex = 18;
            this.buttonUsarArma.Text = "Usar Arma";
            this.buttonUsarArma.UseVisualStyleBackColor = true;
            this.buttonUsarArma.Click += new System.EventHandler(this.buttonUsarArma_Click);
            // 
            // buttonUsarPoçao
            // 
            this.buttonUsarPoçao.Location = new System.Drawing.Point(620, 593);
            this.buttonUsarPoçao.Name = "buttonUsarPoçao";
            this.buttonUsarPoçao.Size = new System.Drawing.Size(75, 23);
            this.buttonUsarPoçao.TabIndex = 19;
            this.buttonUsarPoçao.Text = "Usar Poção";
            this.buttonUsarPoçao.UseVisualStyleBackColor = true;
            this.buttonUsarPoçao.Click += new System.EventHandler(this.buttonUsarPoçao_Click);
            // 
            // buttonNorte
            // 
            this.buttonNorte.Location = new System.Drawing.Point(493, 433);
            this.buttonNorte.Name = "buttonNorte";
            this.buttonNorte.Size = new System.Drawing.Size(75, 23);
            this.buttonNorte.TabIndex = 20;
            this.buttonNorte.Text = "Norte";
            this.buttonNorte.UseVisualStyleBackColor = true;
            this.buttonNorte.Click += new System.EventHandler(this.buttonNorte_Click);
            // 
            // buttonLeste
            // 
            this.buttonLeste.Location = new System.Drawing.Point(573, 457);
            this.buttonLeste.Name = "buttonLeste";
            this.buttonLeste.Size = new System.Drawing.Size(75, 23);
            this.buttonLeste.TabIndex = 21;
            this.buttonLeste.Text = "Leste";
            this.buttonLeste.UseVisualStyleBackColor = true;
            this.buttonLeste.Click += new System.EventHandler(this.buttonLeste_Click);
            // 
            // buttonSul
            // 
            this.buttonSul.Location = new System.Drawing.Point(493, 487);
            this.buttonSul.Name = "buttonSul";
            this.buttonSul.Size = new System.Drawing.Size(75, 23);
            this.buttonSul.TabIndex = 22;
            this.buttonSul.Text = "Sul";
            this.buttonSul.UseVisualStyleBackColor = true;
            this.buttonSul.Click += new System.EventHandler(this.buttonSul_Click);
            // 
            // buttonOeste
            // 
            this.buttonOeste.Location = new System.Drawing.Point(412, 457);
            this.buttonOeste.Name = "buttonOeste";
            this.buttonOeste.Size = new System.Drawing.Size(75, 23);
            this.buttonOeste.TabIndex = 23;
            this.buttonOeste.Text = "Oeste";
            this.buttonOeste.UseVisualStyleBackColor = true;
            this.buttonOeste.Click += new System.EventHandler(this.buttonOeste_Click);
            // 
            // richTextBoxLocal
            // 
            this.richTextBoxLocal.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxLocal.Location = new System.Drawing.Point(347, 19);
            this.richTextBoxLocal.Name = "richTextBoxLocal";
            this.richTextBoxLocal.ReadOnly = true;
            this.richTextBoxLocal.Size = new System.Drawing.Size(360, 105);
            this.richTextBoxLocal.TabIndex = 24;
            this.richTextBoxLocal.Text = "";
            // 
            // richTextBoxMensagens
            // 
            this.richTextBoxMensagens.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxMensagens.Location = new System.Drawing.Point(347, 130);
            this.richTextBoxMensagens.Name = "richTextBoxMensagens";
            this.richTextBoxMensagens.ReadOnly = true;
            this.richTextBoxMensagens.Size = new System.Drawing.Size(360, 286);
            this.richTextBoxMensagens.TabIndex = 25;
            this.richTextBoxMensagens.Text = "";
            // 
            // dataGridViewInventario
            // 
            this.dataGridViewInventario.AllowUserToAddRows = false;
            this.dataGridViewInventario.AllowUserToDeleteRows = false;
            this.dataGridViewInventario.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewInventario.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewInventario.Location = new System.Drawing.Point(16, 130);
            this.dataGridViewInventario.MultiSelect = false;
            this.dataGridViewInventario.Name = "dataGridViewInventario";
            this.dataGridViewInventario.ReadOnly = true;
            this.dataGridViewInventario.Size = new System.Drawing.Size(312, 309);
            this.dataGridViewInventario.TabIndex = 26;
            // 
            // dataGridViewQuests
            // 
            this.dataGridViewQuests.AllowUserToAddRows = false;
            this.dataGridViewQuests.AllowUserToDeleteRows = false;
            this.dataGridViewQuests.AllowUserToResizeRows = false;
            this.dataGridViewQuests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewQuests.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewQuests.Location = new System.Drawing.Point(16, 446);
            this.dataGridViewQuests.MultiSelect = false;
            this.dataGridViewQuests.Name = "dataGridViewQuests";
            this.dataGridViewQuests.ReadOnly = true;
            this.dataGridViewQuests.Size = new System.Drawing.Size(312, 189);
            this.dataGridViewQuests.TabIndex = 27;
            // 
            // AventuraRPG
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 651);
            this.Controls.Add(this.dataGridViewQuests);
            this.Controls.Add(this.dataGridViewInventario);
            this.Controls.Add(this.richTextBoxMensagens);
            this.Controls.Add(this.richTextBoxLocal);
            this.Controls.Add(this.buttonOeste);
            this.Controls.Add(this.buttonSul);
            this.Controls.Add(this.buttonLeste);
            this.Controls.Add(this.buttonNorte);
            this.Controls.Add(this.buttonUsarPoçao);
            this.Controls.Add(this.buttonUsarArma);
            this.Controls.Add(this.comboBoxPoçoes);
            this.Controls.Add(this.comboBoxArmas);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblLevel);
            this.Controls.Add(this.lblExperiencia);
            this.Controls.Add(this.lblOuro);
            this.Controls.Add(this.lblVida);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "AventuraRPG";
            this.Text = "Aventura RPG V0001";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewInventario)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewQuests)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblVida;
        private System.Windows.Forms.Label lblOuro;
        private System.Windows.Forms.Label lblExperiencia;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxArmas;
        private System.Windows.Forms.ComboBox comboBoxPoçoes;
        private System.Windows.Forms.Button buttonUsarArma;
        private System.Windows.Forms.Button buttonUsarPoçao;
        private System.Windows.Forms.Button buttonNorte;
        private System.Windows.Forms.Button buttonLeste;
        private System.Windows.Forms.Button buttonSul;
        private System.Windows.Forms.Button buttonOeste;
        private System.Windows.Forms.RichTextBox richTextBoxLocal;
        private System.Windows.Forms.RichTextBox richTextBoxMensagens;
        private System.Windows.Forms.DataGridView dataGridViewInventario;
        private System.Windows.Forms.DataGridView dataGridViewQuests;
    }
}


namespace s3s_viewer
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSetup = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.wv2View = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.edtID = new s3s_viewer.WatermarkTextbox();
            this.edtPassword = new s3s_viewer.WatermarkTextbox();
            this.edtFreq = new s3s_viewer.WatermarkTextbox();
            ((System.ComponentModel.ISupportInitialize)(this.wv2View)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSetup
            // 
            this.btnSetup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSetup.Location = new System.Drawing.Point(1012, 704);
            this.btnSetup.Name = "btnSetup";
            this.btnSetup.Size = new System.Drawing.Size(75, 23);
            this.btnSetup.TabIndex = 5;
            this.btnSetup.Text = "セットアップ";
            this.btnSetup.UseVisualStyleBackColor = true;
            this.btnSetup.Click += new System.EventHandler(this.BtnSetup_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Location = new System.Drawing.Point(1093, 704);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "接続";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(1174, 704);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 7;
            this.btnDisconnect.Text = "切断";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1255, 704);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "閉じる";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // wv2View
            // 
            this.wv2View.AllowExternalDrop = true;
            this.wv2View.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wv2View.CreationProperties = null;
            this.wv2View.DefaultBackgroundColor = System.Drawing.Color.White;
            this.wv2View.Location = new System.Drawing.Point(12, 36);
            this.wv2View.Name = "wv2View";
            this.wv2View.Size = new System.Drawing.Size(1318, 659);
            this.wv2View.Source = new System.Uri("https://stat.ink/", System.UriKind.Absolute);
            this.wv2View.TabIndex = 6;
            this.wv2View.ZoomFactor = 1D;
            // 
            // edtID
            // 
            this.edtID.Location = new System.Drawing.Point(12, 7);
            this.edtID.Name = "edtID";
            this.edtID.Size = new System.Drawing.Size(189, 23);
            this.edtID.TabIndex = 0;
            this.edtID.WatermarkText = "ユーザー名";
            // 
            // edtPassword
            // 
            this.edtPassword.Location = new System.Drawing.Point(207, 7);
            this.edtPassword.Name = "edtPassword";
            this.edtPassword.PasswordChar = '*';
            this.edtPassword.Size = new System.Drawing.Size(189, 23);
            this.edtPassword.TabIndex = 1;
            this.edtPassword.WatermarkText = "パスワード";
            // 
            // edtFreq
            // 
            this.edtFreq.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.edtFreq.Location = new System.Drawing.Point(402, 7);
            this.edtFreq.Name = "edtFreq";
            this.edtFreq.ShortcutsEnabled = false;
            this.edtFreq.Size = new System.Drawing.Size(189, 23);
            this.edtFreq.TabIndex = 2;
            this.edtFreq.Text = "60";
            this.edtFreq.WatermarkText = "更新間隔";
            this.edtFreq.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EdtFreq_KeyPress);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1342, 736);
            this.Controls.Add(this.edtFreq);
            this.Controls.Add(this.edtPassword);
            this.Controls.Add(this.edtID);
            this.Controls.Add(this.wv2View);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnSetup);
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "stat.ink";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wv2View)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button btnSetup;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnClose;
        private Microsoft.Web.WebView2.WinForms.WebView2 wv2View;
        private WatermarkTextbox edtID;
        private WatermarkTextbox edtPassword;
        private WatermarkTextbox edtFreq;
    }
}
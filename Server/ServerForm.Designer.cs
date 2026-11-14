namespace Chat_room_test_TCP
{
    partial class ServerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Start_Server_Button = new System.Windows.Forms.Button();
            this.Close_Server_Button = new System.Windows.Forms.Button();
            this.Exit_button = new System.Windows.Forms.Button();
            this.Server_ChatView = new System.Windows.Forms.RichTextBox();
            this.Status_Label = new System.Windows.Forms.Label();
            this.ServiceUrl_Label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Start_Server_Button
            // 
            this.Start_Server_Button.Location = new System.Drawing.Point(24, 18);
            this.Start_Server_Button.Name = "Start_Server_Button";
            this.Start_Server_Button.Size = new System.Drawing.Size(130, 42);
            this.Start_Server_Button.TabIndex = 0;
            this.Start_Server_Button.Text = "Bật WebService";
            this.Start_Server_Button.UseVisualStyleBackColor = true;
            this.Start_Server_Button.Click += new System.EventHandler(this.Start_Server_Button_Click);
            // 
            // Close_Server_Button
            // 
            this.Close_Server_Button.Location = new System.Drawing.Point(170, 18);
            this.Close_Server_Button.Name = "Close_Server_Button";
            this.Close_Server_Button.Size = new System.Drawing.Size(130, 42);
            this.Close_Server_Button.TabIndex = 1;
            this.Close_Server_Button.Text = "Tắt WebService";
            this.Close_Server_Button.UseVisualStyleBackColor = true;
            this.Close_Server_Button.Click += new System.EventHandler(this.Close_Server_Button_Click);
            // 
            // Exit_button
            // 
            this.Exit_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Exit_button.Location = new System.Drawing.Point(676, 18);
            this.Exit_button.Name = "Exit_button";
            this.Exit_button.Size = new System.Drawing.Size(100, 42);
            this.Exit_button.TabIndex = 2;
            this.Exit_button.Text = "Thoát";
            this.Exit_button.UseVisualStyleBackColor = true;
            this.Exit_button.Click += new System.EventHandler(this.Exit_button_Click);
            // 
            // Server_ChatView
            // 
            this.Server_ChatView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Server_ChatView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Server_ChatView.Location = new System.Drawing.Point(24, 103);
            this.Server_ChatView.Name = "Server_ChatView";
            this.Server_ChatView.ReadOnly = true;
            this.Server_ChatView.Size = new System.Drawing.Size(752, 317);
            this.Server_ChatView.TabIndex = 5;
            this.Server_ChatView.Text = "";
            this.Server_ChatView.WordWrap = false;
            this.Server_ChatView.TextChanged += new System.EventHandler(this.Server_ChatView_TextChanged);
            // 
            // Status_Label
            // 
            this.Status_Label.AutoSize = true;
            this.Status_Label.Location = new System.Drawing.Point(24, 73);
            this.Status_Label.Name = "Status_Label";
            this.Status_Label.Size = new System.Drawing.Size(107, 16);
            this.Status_Label.TabIndex = 3;
            this.Status_Label.Text = "Trạng thái: Đã tắt";
            // 
            // ServiceUrl_Label
            // 
            this.ServiceUrl_Label.AutoSize = true;
            this.ServiceUrl_Label.Location = new System.Drawing.Point(210, 73);
            this.ServiceUrl_Label.Name = "ServiceUrl_Label";
            this.ServiceUrl_Label.Size = new System.Drawing.Size(170, 16);
            this.ServiceUrl_Label.TabIndex = 4;
            this.ServiceUrl_Label.Text = "Địa chỉ: http://localhost:5000";
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Server_ChatView);
            this.Controls.Add(this.ServiceUrl_Label);
            this.Controls.Add(this.Status_Label);
            this.Controls.Add(this.Exit_button);
            this.Controls.Add(this.Close_Server_Button);
            this.Controls.Add(this.Start_Server_Button);
            this.MinimumSize = new System.Drawing.Size(650, 400);
            this.Name = "ServerForm";
            this.Text = "Zalu WebService Dashboard";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button Start_Server_Button;
        private System.Windows.Forms.Button Close_Server_Button;
        private System.Windows.Forms.Button Exit_button;
        private System.Windows.Forms.RichTextBox Server_ChatView;
        private System.Windows.Forms.Label Status_Label;
        private System.Windows.Forms.Label ServiceUrl_Label;
    }
}

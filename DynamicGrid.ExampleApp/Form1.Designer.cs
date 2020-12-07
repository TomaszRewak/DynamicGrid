
namespace DynamicGrid.ExampleApp
{
	partial class Form1
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
			this.gridHeader = new DynamicGrid.ExampleApp.MyGridHeader();
			this.grid = new DynamicGrid.ExampleApp.MyGrid();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.gridHeader.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.gridHeader.Location = new System.Drawing.Point(12, 12);
			this.gridHeader.Name = "gridHeader";
			this.gridHeader.Size = new System.Drawing.Size(776, 30);
			this.gridHeader.TabIndex = 0;
			// 
			// grid
			// 
			this.grid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.grid.Location = new System.Drawing.Point(12, 42);
			this.grid.Name = "grid";
			this.grid.Size = new System.Drawing.Size(776, 396);
			this.grid.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.gridHeader);
			this.Controls.Add(this.grid);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private DynamicGrid.ExampleApp.MyGridHeader gridHeader;
		private DynamicGrid.ExampleApp.MyGrid grid;
	}
}


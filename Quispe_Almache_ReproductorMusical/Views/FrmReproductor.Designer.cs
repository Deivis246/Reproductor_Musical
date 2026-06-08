namespace Quispe_Almache_ReproductorMusical.Views
{
    partial class FrmReproductor
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelVisualizacion = new System.Windows.Forms.Panel();
            this.btnCargar = new System.Windows.Forms.Button();
            this.btnReproducir = new System.Windows.Forms.Button();
            this.btnPausar = new System.Windows.Forms.Button();
            this.btnDetener = new System.Windows.Forms.Button();
            this.barraVolumen = new System.Windows.Forms.TrackBar();
            this.lblVolumen = new System.Windows.Forms.Label();
            this.lblTiempo = new System.Windows.Forms.Label();
            this.cmbModoVisualizacion = new System.Windows.Forms.ComboBox();
            this.lblModoVisualizacion = new System.Windows.Forms.Label();
            this.lblNombreArchivo = new System.Windows.Forms.Label();
            this.lblEstado = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.barraVolumen)).BeginInit();
            this.SuspendLayout();
            // 
            // panelVisualizacion
            // 
            this.panelVisualizacion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelVisualizacion.BackColor = System.Drawing.Color.Black;
            this.panelVisualizacion.Location = new System.Drawing.Point(12, 12);
            this.panelVisualizacion.Name = "panelVisualizacion";
            this.panelVisualizacion.Size = new System.Drawing.Size(760, 350);
            this.panelVisualizacion.TabIndex = 0;
            // 
            // btnCargar
            // 
            this.btnCargar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCargar.Location = new System.Drawing.Point(12, 368);
            this.btnCargar.Name = "btnCargar";
            this.btnCargar.Size = new System.Drawing.Size(75, 30);
            this.btnCargar.TabIndex = 1;
            this.btnCargar.Text = "Cargar";
            this.btnCargar.UseVisualStyleBackColor = true;
            this.btnCargar.Click += new System.EventHandler(this.btnCargar_Click);
            // 
            // btnReproducir
            // 
            this.btnReproducir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReproducir.Location = new System.Drawing.Point(93, 368);
            this.btnReproducir.Name = "btnReproducir";
            this.btnReproducir.Size = new System.Drawing.Size(75, 30);
            this.btnReproducir.TabIndex = 2;
            this.btnReproducir.Text = "Reproducir";
            this.btnReproducir.UseVisualStyleBackColor = true;
            this.btnReproducir.Click += new System.EventHandler(this.btnReproducir_Click);
            // 
            // btnPausar
            // 
            this.btnPausar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPausar.Location = new System.Drawing.Point(174, 368);
            this.btnPausar.Name = "btnPausar";
            this.btnPausar.Size = new System.Drawing.Size(75, 30);
            this.btnPausar.TabIndex = 3;
            this.btnPausar.Text = "Pausar";
            this.btnPausar.UseVisualStyleBackColor = true;
            this.btnPausar.Click += new System.EventHandler(this.btnPausar_Click);
            // 
            // btnDetener
            // 
            this.btnDetener.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDetener.Location = new System.Drawing.Point(255, 368);
            this.btnDetener.Name = "btnDetener";
            this.btnDetener.Size = new System.Drawing.Size(75, 30);
            this.btnDetener.TabIndex = 4;
            this.btnDetener.Text = "Detener";
            this.btnDetener.UseVisualStyleBackColor = true;
            this.btnDetener.Click += new System.EventHandler(this.btnDetener_Click);
            // 
            // barraVolumen
            // 
            this.barraVolumen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.barraVolumen.Location = new System.Drawing.Point(336, 375);
            this.barraVolumen.Maximum = 100;
            this.barraVolumen.Name = "barraVolumen";
            this.barraVolumen.Size = new System.Drawing.Size(150, 45);
            this.barraVolumen.TabIndex = 5;
            this.barraVolumen.Value = 100;
            this.barraVolumen.Scroll += new System.EventHandler(this.barraVolumen_Scroll);
            // 
            // lblVolumen
            // 
            this.lblVolumen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblVolumen.AutoSize = true;
            this.lblVolumen.Location = new System.Drawing.Point(336, 355);
            this.lblVolumen.Name = "lblVolumen";
            this.lblVolumen.Size = new System.Drawing.Size(47, 13);
            this.lblVolumen.TabIndex = 6;
            this.lblVolumen.Text = "Volumen";
            // 
            // lblTiempo
            // 
            this.lblTiempo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTiempo.AutoSize = true;
            this.lblTiempo.Location = new System.Drawing.Point(700, 355);
            this.lblTiempo.Name = "lblTiempo";
            this.lblTiempo.Size = new System.Drawing.Size(72, 13);
            this.lblTiempo.TabIndex = 7;
            this.lblTiempo.Text = "00:00 / 00:00";
            // 
            // cmbModoVisualizacion
            // 
            this.cmbModoVisualizacion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbModoVisualizacion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModoVisualizacion.FormattingEnabled = true;
            this.cmbModoVisualizacion.Items.AddRange(new object[] {
            "Barras de Espectro",
            "Onda de Audio",
            "Partículas",
            "Formas Geométricas",
            "Espectro Circular"});
            this.cmbModoVisualizacion.Location = new System.Drawing.Point(502, 370);
            this.cmbModoVisualizacion.Name = "cmbModoVisualizacion";
            this.cmbModoVisualizacion.Size = new System.Drawing.Size(150, 21);
            this.cmbModoVisualizacion.TabIndex = 8;
            this.cmbModoVisualizacion.SelectedIndexChanged += new System.EventHandler(this.cmbModoVisualizacion_SelectedIndexChanged);
            // 
            // lblModoVisualizacion
            // 
            this.lblModoVisualizacion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblModoVisualizacion.AutoSize = true;
            this.lblModoVisualizacion.Location = new System.Drawing.Point(502, 355);
            this.lblModoVisualizacion.Name = "lblModoVisualizacion";
            this.lblModoVisualizacion.Size = new System.Drawing.Size(119, 13);
            this.lblModoVisualizacion.TabIndex = 9;
            this.lblModoVisualizacion.Text = "Modo de Visualización";
            // 
            // lblNombreArchivo
            // 
            this.lblNombreArchivo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNombreArchivo.AutoSize = true;
            this.lblNombreArchivo.Location = new System.Drawing.Point(500, 410);
            this.lblNombreArchivo.Name = "lblNombreArchivo";
            this.lblNombreArchivo.Size = new System.Drawing.Size(272, 13);
            this.lblNombreArchivo.TabIndex = 10;
            this.lblNombreArchivo.Text = "Archivo: Ninguno cargado";
            // 
            // lblEstado
            // 
            this.lblEstado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstado.AutoSize = true;
            this.lblEstado.Location = new System.Drawing.Point(12, 410);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(45, 13);
            this.lblEstado.TabIndex = 11;
            this.lblEstado.Text = "Estado: -";
            // 
            // FrmReproductor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 435);
            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.lblNombreArchivo);
            this.Controls.Add(this.lblModoVisualizacion);
            this.Controls.Add(this.cmbModoVisualizacion);
            this.Controls.Add(this.lblTiempo);
            this.Controls.Add(this.lblVolumen);
            this.Controls.Add(this.barraVolumen);
            this.Controls.Add(this.btnDetener);
            this.Controls.Add(this.btnPausar);
            this.Controls.Add(this.btnReproducir);
            this.Controls.Add(this.btnCargar);
            this.Controls.Add(this.panelVisualizacion);
            this.MinimumSize = new System.Drawing.Size(800, 470);
            this.Name = "FrmReproductor";
            this.Text = "Reproductor Musical con Animaciones Sincronizadas";
            this.Load += new System.EventHandler(this.FrmReproductor_Load);
            this.Resize += new System.EventHandler(this.FrmReproductor_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.barraVolumen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelVisualizacion;
        private System.Windows.Forms.Button btnCargar;
        private System.Windows.Forms.Button btnReproducir;
        private System.Windows.Forms.Button btnPausar;
        private System.Windows.Forms.Button btnDetener;
        private System.Windows.Forms.TrackBar barraVolumen;
        private System.Windows.Forms.Label lblVolumen;
        private System.Windows.Forms.Label lblTiempo;
        private System.Windows.Forms.ComboBox cmbModoVisualizacion;
        private System.Windows.Forms.Label lblModoVisualizacion;
        private System.Windows.Forms.Label lblNombreArchivo;
        private System.Windows.Forms.Label lblEstado;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quispe_Almache_ReproductorMusical.Controllers;
using Quispe_Almache_ReproductorMusical.Models;
using Quispe_Almache_ReproductorMusical.Views;

namespace Quispe_Almache_ReproductorMusical
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            ReproductorModel modelo = new ReproductorModel();
            FrmReproductor vista = new FrmReproductor();
            ReproductorController controlador = new ReproductorController(modelo, vista);
            
            controlador.Iniciar();
        }
    }
}

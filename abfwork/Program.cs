using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace abfwork
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form mainForm = new  mainForm();
            DialogResult result = mainForm.ShowDialog();
            //DialogResult result = loginForm.ShowDialog();
            //if (result == DialogResult.OK)
            //{
            //    Application.Run(new mainForm());
            //}
        }
    }
}

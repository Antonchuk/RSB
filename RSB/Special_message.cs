using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSB
{
    public partial class Special_message : Form
    {
        private static int counter = 0;
        private System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"\\HOLY-BOX\Public\Обменник 2\Лукьянчук\hypno_2.wav");
        /// <summary>
        /// форма предупреждения!!!!
        /// </summary>
        public Special_message()
        {
            InitializeComponent();
        }

        private void Special_message_Load(object sender, EventArgs e)
        {
            lbl_text.Text = "Use the DATABASE plugin in KVANTM3D\nIT IS AN ORDER!!";
            this.WindowState = FormWindowState.Maximized;
            //запускаем музыку            
            player.Play();

        }

        private async void Special_message_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (counter == 0)
            {
                e.Cancel = true;
                await Task.Delay(5000);
                player.Stop();
                counter += 1;
            }
            else
            {
                e.Cancel = false;
                counter = 0;
            }                        
        }
    }
}

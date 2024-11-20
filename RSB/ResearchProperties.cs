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
    public partial class ResearchProperties : Form
    {
        public ResearchProperties()
        {
            InitializeComponent();
        }
        private bool IsCheked(System.Windows.Forms.RadioButton yesbtn, System.Windows.Forms.RadioButton nobtn)
        {
            if (yesbtn.Checked || nobtn.Checked)
            {
                return true;
            }
            return false;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (IsCheked(radioButtonBad, radioButtonGood) &&
                IsCheked(radioButtonCrystNo, radioButtonCrystYes) &&
                IsCheked(radioButtonPahsesNo, radioButtonPhasesYes) &&
                IsCheked(radioButtonRaggedNo, radioButtonRaggedYes) &&
                IsCheked(radioButtonTEMControl, radioButtonTEMControlNo))
            {
                Properties.Settings.Default.ResPropGood = radioButtonGood.Checked; //true - сложно
                Properties.Settings.Default.ResPropCryst = radioButtonCrystYes.Checked; //true - много попыток
                Properties.Settings.Default.ResPropPhase = radioButtonPhasesYes.Checked; //
                Properties.Settings.Default.ResPropRagged = radioButtonRaggedYes.Checked;
                Properties.Settings.Default.ResPropTEM = radioButtonTEMControl.Checked; //
                Properties.Settings.Default.Save();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Choose wisely!");
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}

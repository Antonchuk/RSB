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
    public partial class SpecimenProperties : Form
    {
        private Flags CurrentFlgas = new Flags();
        
        public SpecimenProperties()
        {
            InitializeComponent();
            
            radioButtonAttemptsHigh.Checked = false;
            radioButtonAttemptsLow.Checked = false;
            radioButtonEasy.Checked = false;
            radioButtonHard.Checked = false;
            radioButtonFragileNo.Checked = false;
            radioButtonFragileYes.Checked = false;
            radioButtonLongThinNo.Checked = false;
            radioButtonLongThinYes.Checked = false;
            radioButtonSelectiveNo.Checked = false;
            radioButtonSelectiveYes.Checked = false;
        }
        private bool IsCheked(System.Windows.Forms.RadioButton yesbtn, System.Windows.Forms.RadioButton nobtn)
        {
            if (yesbtn.Checked || nobtn.Checked) 
            { 
                return true; 
            }
            return false;
        }
        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (IsCheked(radioButtonAttemptsHigh, radioButtonAttemptsLow) &&
                IsCheked(radioButtonEasy, radioButtonHard) &&
                IsCheked(radioButtonFragileNo, radioButtonFragileYes) &&
                IsCheked(radioButtonLongThinNo, radioButtonLongThinYes) &&
                IsCheked(radioButtonSelectiveNo, radioButtonSelectiveYes))
            {
                Properties.Settings.Default.SpecPropGeneral = radioButtonHard.Checked; //true - сложно
                Properties.Settings.Default.SpecPropAttempts = radioButtonAttemptsHigh.Checked; //true - много попыток
                Properties.Settings.Default.SpecPropFragile = radioButtonFragileYes.Checked; //
                Properties.Settings.Default.SpecPropEtching = radioButtonSelectiveYes.Checked;
                Properties.Settings.Default.SpecPropLongSection = radioButtonLongThinNo.Checked; //
                Properties.Settings.Default.Save();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Choose wisely!");
            }
        }
    }
    public class Flags
    {
        public bool General;
        public bool Attempts;
        public bool Fragile;
        public bool Etching;
        public bool LongSection;
    }
}

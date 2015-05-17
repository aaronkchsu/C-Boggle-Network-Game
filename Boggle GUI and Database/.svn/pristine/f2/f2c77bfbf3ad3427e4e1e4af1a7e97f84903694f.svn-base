using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;

namespace BoggleClient
{
    public partial class BoggleGUI : Form
    {
        private BoggleClientModel model;

        public BoggleGUI()
        {
            InitializeComponent();
            model = new BoggleClientModel();
            model.gui = this;
        }

        private void usrBox_TextChanged(object sender, EventArgs e)
        {
            model.UserName = ((TextBox)sender).Text;
        }

        private void ipBox_TextChanged(object sender, EventArgs e)
        {
            model.ipAddress = ((TextBox)sender).Text;
        }

        private void wordBox_TextChanged(object sender, EventArgs e)
        {
            model.SendWord(((TextBox) sender).Text);
        }

        private void playBTN_Click(object sender, EventArgs e)
        {
            if (model.UserName != null && model.ipAddress != null)
            {
                System.Net.IPAddress temp = null;
                if (!System.Net.IPAddress.TryParse(model.ipAddress, out temp))
                    temp = Dns.GetHostAddresses(model.ipAddress)[0];

                //Handle the exception. Pop up a window if there is a connection problem.
                model.Connect(temp.ToString(), 2000, model.UserName);

            }
        }
        
    }
}

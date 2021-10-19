using System;
using System.Windows.Forms;
using System.Configuration;
using System.Xml;

namespace Diascan.Agent.ClientApp
{
    public partial class AddressesForm : Form
    {
        
        public AddressesForm()
        {
            InitializeComponent();

            try
            {
                tbAddress.Text = SharingEvents.SharingEvents.OnGetAddressConnection().Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var newAddress = tbAddress.Text;
            try
            {
                SharingEvents.SharingEvents.OnChangeAddressConnection(newAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Close();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var newAddress = tbAddress.Text;
            try
            {
                var status = SharingEvents.SharingEvents.OnTestConnection(newAddress);
                switch (status.Result)
                {
                    case true:
                        MessageBox.Show("Соединение установлено.");
                        break;
                    case false:
                        MessageBox.Show("Ошибка соединения.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

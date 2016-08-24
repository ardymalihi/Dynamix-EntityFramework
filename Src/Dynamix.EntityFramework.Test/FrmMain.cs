using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Dynamix.EntityFramework.Db;
using Dynamix.EntityFramework.Model;

namespace Dynamix.Test
{
    public partial class FrmMain : Form
    {
        DbAdapter dbAdapter = null;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Text = "Connecting";
                btnConnect.Enabled = false;

                //Main Code
                dbAdapter = new DbAdapter(txtConnectionString.Text);
                dbAdapter.Config.Asynchronized = true;
                dbAdapter.Config.BrowsePrimaryKeyColumns = true;
                dbAdapter.Config.BrowseForeignKeyColumns = true;
                dbAdapter.OnFinishedLoading += new FinishedLoading(db_OnFinishedLoading);
                dbAdapter.Load();

                


            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        void db_OnFinishedLoading()
        {
            if (dbAdapter.IsActive)
            {
                btnConnect.Text = "Connected";
                lstTables.DataSource = dbAdapter.dbSchemaBuilder.Tables.ToList();
                grb.Visible = true;
            }
            else
            {
                btnConnect.Text = "Connect";
                btnConnect.Enabled = true;
            }
        }

        private void lstTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Binding Columns
            lstColumns.DataSource = ((Table)lstTables.SelectedItem).Columns;

            //Bindign Grid
            try
            {
                var List = dbAdapter.Get(lstTables.SelectedItem.ToString());
                //DbExtensions.Load(List);
                dataGridView1.DataSource = (List as IEnumerable<dynamic>).ToList();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void lstColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Binding Column Info
            pgColumnInfo.SelectedObject = lstColumns.SelectedItem;
        }

        
    }
}

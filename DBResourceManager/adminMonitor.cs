using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DBResourceManager
{   
    /// <summary>
    /// admin monitor feature
    /// </summary>
    public partial class adminMonitor : Form
    {
        /// <summary>
        /// update history window
        /// </summary>
        public adminMonitor()
        {
            InitializeComponent();
            updateUserDBWindow();
        }

        /// <summary>
        /// add extra time for specific users after button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            int outputValue,userTime;
            string userName, sql;

            if (!int.TryParse(textTime.Text, out outputValue)) {
                MessageBox.Show("Please enter an integer in the Add Time box");
                return;
            }

            userTime = Convert.ToInt32(textTime.Text);
            if (userTime < 0)
            {
                MessageBox.Show("Please enter a positive integer in the Add Time box");
                return;
            }

            userName = textUser.Text;
            sql = string.Format("UPDATE CLIENT SET TimeLeft += {0} WHERE Username = '{1}'", userTime * 1000, userName);

            addExtraTimeToUser(sql);
            updateUserDBWindow();
        }


        /// <summary>
        /// helper method, check if user exist or other error occured 
        /// </summary>
        /// <param name="sql"></param>
        private void addExtraTimeToUser(String sql) {
            using (SqlConnection connection = new SqlConnection(Globals.userDB))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.UpdateCommand = new SqlCommand(sql, connection);
                    if (adapter.UpdateCommand.ExecuteNonQuery() == 0)
                    {
                        MessageBox.Show("Invalid username");
                    }
                    MessageBox.Show("Update Success!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred!");
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// update textbox
        /// </summary>
        private void updateUserDBWindow()
        {
            using (SqlConnection connection = new SqlConnection(Globals.userDB))
            {
                string sql = "select * from client";
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader rdr = command.ExecuteReader();
                while (rdr.Read())
                {
                    richTextBoxUserDB.Text += String.Format("UserName: {0} Dailylimit:{1}ms CurrentTimeLeft: {2}ms\n", rdr[0], rdr[2], rdr[3]);
                }
            }
        }

        /// <summary>
        /// history textbox set method
        /// </summary>
        /// <param name="userName"></param>
        public void setHistory(string userName)
        {
            Utility.historyDisplay(userName, adminHistoryTextBox);
        }

        /// <summary>
        /// activity text box set method
        /// </summary>
        /// <param name="update"></param>
        public void setActicity(string update)
        {
            richTextBoxActivity.Text = update;
        }
    }
}

using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DBResourceManager
{
    /// <summary>
    /// This is user login component
    /// </summary>
    public partial class userLogin : Form
    {
        string userName, passWord,sql;
        SqlCommand command;
        adminMonitor adminform;
        
        /// <summary>
        /// Create admin window 
        /// </summary>
        public userLogin() 
        {
            InitializeComponent();
            adminform = new adminMonitor();
            adminform.Show();
        }

        /// <summary>
        /// Check user credentials to login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) 
        {
            userName = txtUsername.Text;
            passWord = txtPassword.Text;
            
            sql = string.Format("SELECT * FROM CLIENT WHERE Username = '{0}' and Password = '{1}'  ", userName,passWord);

            using (SqlConnection connection = new SqlConnection(Globals.userDB))
            {
                try
                {
                    //sql query testing
                    int time = 0;
                    connection.Open();
                    command = new SqlCommand(sql, connection);

                    SqlDataReader rdr = command.ExecuteReader();
                    Console.WriteLine(sql);

                    rdr.Read();
                    if (rdr[0] != null) { time = (int)rdr[3]; }
                    
                    if (time <= 0)
                    {
                        MessageBox.Show("Sorry, your current avilable time is 0, please come back tomorrow");
                        return;
                    }

                    userMain form = new userMain(userName, time);
                    adminform.setHistory(userName);
                    adminform.setActicity(userName + "login at "+ Utility.getDateTime()+" time remain: " + time);

                    this.Hide();
                    form.FormClosed += new FormClosedEventHandler(adminMain_formClosed);
                    form.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid username or password "+ ex.Message);
                }
            }
        }

        private void adminMain_formClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}

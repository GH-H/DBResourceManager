using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DBResourceManager
{
    /// <summary>
    /// This class contains helper methods for multiple uses.
    /// </summary>
    static class Utility
    {
        /// <summary>
        /// Method to display history text box
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="historyTextBox"></param>
        public static void historyDisplay(string userName, RichTextBox historyTextBox)
        {
            using (SqlConnection connection = new SqlConnection(Globals.userDB))
            {
                string sql;
                int counter;
                sql = String.Format("select * from {0}History", userName);

                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader rdr = command.ExecuteReader();
                counter = 1;
                historyTextBox.Text = string.Empty;
                while (rdr.Read())
                {
                    historyTextBox.Text += String.Format("{2}. query date time{0}, time used {1} ms,result stored in table{3}\n", rdr[0], rdr[1], counter,rdr[2]);
                    counter++;
                }
            }
        }

        /// <summary>
        /// Get current system time in SQL server datetime format
        /// </summary>
        /// <returns></returns>
        public static string getDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Update history textbox
        /// </summary>
        /// <param name="newLine"></param>
        /// <param name="historyTextBox"></param>
        public static void updateHistoryDisplay(string newLine, RichTextBox historyTextBox)
        {
            historyTextBox.Text += newLine;
        }
    }
}

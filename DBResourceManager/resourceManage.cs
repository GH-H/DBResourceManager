using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace DBResourceManager
{
    /// <summary>
    /// Database resource management main unit which contains query feature and materialized design.
    /// </summary>
    public partial class userMain : Form
    {
        private int time;
        private String userName;
        private string tableName = "";
        private DataSet DATASET = new DataSet();
        
        /// <summary>
        /// Initialize components upon user login, update textboxs with user history and welcome message.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="time"></param>
        public userMain(String userName, int time)
        {
            InitializeComponent();
            this.userName = userName;
            this.time = time;           
            mainTextBox.Text = "Welcome "+userName+" you still have" + time / 1000 + "s remain\n";
            Utility.historyDisplay(userName, historyTextBox);
        }

        /// <summary>
        /// Execute query on main database after button click and update user database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string sql = txtQuery.Text;
            string dateTime = Utility.getDateTime();

            if (time == 0)
            {
                MessageBox.Show("You have ran out of time. Please come back tomorrow!");
                return;
            }
                
            if (!isReadOnly(txtQuery.Text)) {
                MessageBox.Show("please enter a read only query");
                return;
            }
            
            Thread t1 = new Thread(() => DataRead(sql, dateTime));
            t1.Start();
            AppendTextBox("query processing\n");

            if (!t1.Join(new TimeSpan(0, 0, time / 1000)))
            {
                t1.Abort();
                updateUserInfo(time, dateTime, null);
                Console.WriteLine("Run out of time!");
                return;
            }   

            if (DATASET.Tables[0].Rows.Count != 0)
            {
                dataGridView1.DataSource = DATASET.Tables[0];
            }
                
            Utility.historyDisplay(userName, historyTextBox);
        }

        /// <summary>
        /// Execute query on user database after button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e)
        {
            string sql = txtQuery2.Text;
            SqlDataAdapter adapter;
            DataSet dataSet;

            using (SqlConnection connection = new SqlConnection(Globals.userDB))
            {
                try
                {
                    adapter = new SqlDataAdapter(sql, connection);
                    dataSet = new DataSet();
                    adapter.Fill(dataSet);

                    if (dataSet.Tables[0].Rows.Count != 0)
                    {
                        dataGridView1.DataSource = dataSet.Tables[0];
                    }
                        
                } 
                catch (SqlException ex)
                {
                    MessageBox.Show("Invalid query! "+ ex.Message);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Other error!"+ ex.Message);
                }
            }
        }

        /// <summary>
        /// Load data into table and store into user database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dateTime"></param>
        private void DataRead(string sql, string dateTime) {
            using (SqlConnection connection = new SqlConnection(Globals.mainDB))
            {
                try
                {
                    int elapsed = 0;
                    var sw = new Stopwatch();
                    
                    var columns = new List<string>();
                    var types = new List<string>();
                    DataTable dt = new DataTable();

                    Dictionary<string, string> columnType = columnTypeInit();

                    connection.Open();
                    sw.Start();

                    SqlCommand command = new SqlCommand(sql, connection);
                    var rdr = command.ExecuteReader();

                    Console.WriteLine("loading data into datatable");
                    dt.Load(rdr);
                    Console.WriteLine("data loaded into datatable");

                    DATASET = new DataSet();
                    DATASET.Tables.Add(dt);

                    
                    listInit(columns,types,dt);                   

                    materialize(dateTime,columns,columnType,types,command,dt);

                    elapsed = (int)sw.Elapsed.TotalMilliseconds;
                    Console.WriteLine("Query excution time: " + elapsed + "ms");

                    updateUserInfo(elapsed, dateTime,tableName);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Invalid query!");
                }
                catch (ThreadAbortException ex)
                {
                    MessageBox.Show("You have ran out of time!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Other error!" + ex.Message);
                }
            }
        }

        /// <summary>
        /// Materialized compoent
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="columnType"></param>
        /// <param name="types"></param>
        /// <param name="command"></param>
        /// <param name="dt"></param>
        private void materialize( string dateTime, List<string> columns, Dictionary<string, string> columnType, List<string> types, SqlCommand command, DataTable dt) {
            using (SqlConnection connection2 = new SqlConnection(Globals.userDB))
            {
                connection2.Open();
                string modifiedDateTime = dateTime.Replace("-", "_").Replace(":", "_").Replace(" ", "_");
                tableName = userName + "_" + modifiedDateTime;

                string sql2 = string.Format("CREATE TABLE {0}(", tableName);
                for (int i = 0; i < columns.Count; i++)
                {
                    sql2 += columns[i] + " " + columnType[types[i]] + ", ";
                }
                sql2 += ")";
                Console.WriteLine(sql2);
                command = new SqlCommand(sql2, connection2);
                command.ExecuteNonQuery();
                Console.WriteLine("Table created in userDB");

                SqlBulkCopy bulkcopy = new SqlBulkCopy(connection2);

                bulkcopy.DestinationTableName = tableName;
                try
                {
                    Console.WriteLine("storing query result into userDB");
                    bulkcopy.WriteToServer(dt);
                    Console.WriteLine("storing success");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                Console.WriteLine("instruction complete");
            }
        }

        /// <summary>
        /// Initlize columns and types for user database table creation
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="types"></param>
        /// <param name="dt"></param>
        private void listInit(List<string> columns, List<string>types, DataTable dt) {
            foreach (DataColumn column in dt.Columns)
            {
                columns.Add(column.ColumnName);
                types.Add(column.DataType.Name.ToString());
            }
        }

        /// <summary>
        /// Update user status in user database
        /// </summary>
        /// <param name="elapsed"></param>
        /// <param name="dateTime"></param>
        /// <param name="tableName"></param>
        private void updateUserInfo(int elapsed, string dateTime,string tableName)
        {
            string sql;
            using (SqlConnection connection = new SqlConnection(Globals.userDB))
            {
                connection.Open();

                SqlDataAdapter adapter = new SqlDataAdapter();
                time -= elapsed;
                sql = string.Format("UPDATE CLIENT SET TimeLeft = {0} WHERE Username = '{1}'", time, userName);

                adapter.UpdateCommand = new SqlCommand(sql, connection);
                adapter.UpdateCommand.ExecuteNonQuery();

                //update history
                sql = string.Format("INSERT INTO {0}History (Date, Time,TableName) VALUES ('{1}','{2}','{3}') ", userName, dateTime, elapsed,tableName);
                Console.WriteLine(sql);
                SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
                Console.WriteLine("user History insert correct");
                AppendTextBox("query complete\n you still have" + time / 1000 + "s remain\n");
            }
        }
    
        /// <summary>
        /// Make sure query on main database is read only
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Boolean isReadOnly(string input)
        {
            return !input.ToLower().Contains("update") && !input.ToLower().Contains("delete") &&
                !input.ToLower().Contains("insert") && !input.ToLower().Contains("create") &&
                !input.ToLower().Contains("alter") && !input.ToLower().Contains("drop");
        }

        /// <summary>
        /// Append text in textbox, thread safe
        /// </summary>
        /// <param name="value"></param>
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            mainTextBox.Text += value;
        }

        /// <summary>
        /// Create dictionary to match sqlserver datatype and datatable datatype 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> columnTypeInit() {
            Dictionary<string, string> columnType = new Dictionary<string, string>();
            columnType.Add("Int16", "Int");
            columnType.Add("Int32", "Int");
            columnType.Add("Int64", "Int");
            columnType.Add("UInt16", "Int");
            columnType.Add("UInt32", "Int");
            columnType.Add("UInt64", "Int");
            columnType.Add("String", "varchar(max)");
            columnType.Add("Guid", "uniqueidentifier");
            columnType.Add("DateTime", "DateTime");
            columnType.Add("Date", "Date");
            columnType.Add("Decimal", "Decimal");
            return columnType;
        }
    }
}

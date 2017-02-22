using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Collections;
using InnogrityLinePackingClient;

namespace InnogrityLinePackingClient
{
    class ReportQuery
    {
        NetworkThread nw;
        public int PingMyDb()
        {
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("select * from TblInOut", connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                var table = new DataTable();
                int count = adapter.Fill(table);
                return count;
            }

        }
        public void INS_Func(DateTime Insertdate) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"INSERT INTO TblInOut (DTIn) VALUES (@para1)";
                    command.Parameters.AddWithValue("@para1", Insertdate);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                    }
                }
            }
        }
        public int ReqLastID() //Station2 data insert
        {
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"SELECT MAX(Id) FROM TblInOut";
                    try
                    {
                        connection.Open();
                        int a = (int)command.ExecuteScalar();
                        connection.Close();
                        return a;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return -1;
                    }
                }
            }
        }
        public void UpdST4RJ(string OEEId, int Status,string Reason) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"UPDATE TblInOut SET DTOut = @para1, STnNo = 4, Stat = 4 , Reason = @para2 Where Id = @para3";
                    command.Parameters.AddWithValue("@para1", DateTime.Now);
                    command.Parameters.AddWithValue("@para2", Reason);
                    command.Parameters.AddWithValue("@para3", OEEId);
              
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                    }
                }
            }
        }
        public void UpdST6RJ(string OEEId, int Status, string Reason) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"UPDATE TblInOut SET DTOut = @para1, STnNo = 6, Stat = @para4 , Reason = @para2 Where Id = @para3";
                    command.Parameters.AddWithValue("@para1", DateTime.Now);
                    command.Parameters.AddWithValue("@para2", Reason);
                    command.Parameters.AddWithValue("@para3", OEEId);
                    command.Parameters.AddWithValue("@para4", Status.ToString());
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                    }
                }
            }
        }
        public void UpdST8RJ(string OEEId, int Status, string Reason) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"UPDATE TblInOut SET DTOut = @para1, STnNo = 8, Stat = @para2 , Reason = @para3 Where Id = @para4";
                    command.Parameters.AddWithValue("@para1", DateTime.Now);
                    command.Parameters.AddWithValue("@para2", Status);
                    command.Parameters.AddWithValue("@para3", Reason);
                    command.Parameters.AddWithValue("@para4", OEEId);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                    }
                }
            }
        }
        public string[] UpdJamstatus(int StationNo, int Status, int reason) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = File.ReadAllText(outputFolder + @"\AllStJamUpdate.sql");
                    command.Parameters.AddWithValue("@para3", StationNo);
                    command.Parameters.AddWithValue("@para4", Status);
                    command.Parameters.AddWithValue("@para5", reason);
                    DataTable dt = new DataTable();
                    try
                    {
                        SqlDataAdapter myAdapter = new SqlDataAdapter();
                        connection.Open();
                        // string[,,] Array = new string[,,]  ;
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt);
                        string[] arrray = dt.Rows.OfType<DataRow>().Select(k => k[0].ToString()).ToArray();
                        return arrray;
                    }
                    catch (SqlException ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return null;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return null;
                    }
                }
            }
        }

        public void UpdRJReasonbyID(int Reason, int ID) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"UPDATE TblInOut SET  Reason = @para1, STnNo = 2, Stat = 4 Where Id = @para2";
                    command.Parameters.AddWithValue("@para1", Reason);
                    command.Parameters.AddWithValue("@para2", ID);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                    }
                }
            }
        }
        public void UpdStNobyID(int StationNum, int ID) //Update Station Number by ID
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"UPDATE TblInOut SET  STnNo = @para1 Where Id = @para2";
                    command.Parameters.AddWithValue("@para1", StationNum);
                    command.Parameters.AddWithValue("@para2", ID);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                    }
                }
            }
        }
        public void UpdFLbyID(String FL, int ID) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"UPDATE TblInOut SET  FL = @para1, STnNo = 2 Where Id = @para2";
                    command.Parameters.AddWithValue("@para1", FL);
                    command.Parameters.AddWithValue("@para2", ID);
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                    }
                }
            }
        }
        public int ReqIDbyFL(String FL) //Station2 data insert
        {

            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @" SELECT MAX(Id) from TblInOut where FL = @para1";
                    command.Parameters.AddWithValue("@para1", FL);
                    try
                    {
                        connection.Open();
                        int a = (int)command.ExecuteScalar();
                        connection.Close();
                        return a;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return -1;
                    }
                }
            }
        }
        public string ReqFLByID()
        {
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("select FL from TblInOut Where ID = 5", connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                connection.Open();
                string a = (string)command.ExecuteScalar();
                connection.Close();
                return a;
            }

        }
        #region OldCode
        //public List<string> RetQuerhyItem(string STDate, string EndDate, string STTime, string EndTime,bool IsOverNightShift)
        //{
        //    string[] ShiftTimeCtrl = new string[4];
        //    if (IsOverNightShift)
        //    {
        //        ShiftTimeCtrl[0] = STTime; //Para3
        //        ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
        //        ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
        //        ShiftTimeCtrl[3] = EndTime; //Para6
        //    }
        //    else
        //    {
        //        ShiftTimeCtrl[0] = STTime; //Para3
        //        ShiftTimeCtrl[1] = EndTime; //Para4
        //        ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
        //        ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6
        //    }
        //    string mdfFilename = @"\ReportDatabase.mdf";
        //    string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    outputFolder = outputFolder + @"\resources";
        //    string attachDbFilename = outputFolder + mdfFilename;
        //    string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand())
        //        {
        //            command.Connection = connection;
        //            command.CommandType = CommandType.Text;
        //            command.CommandText = File.ReadAllText(outputFolder + @"\IGTChartQuery.sql");
        //            command.Parameters.AddWithValue("@para1", STDate );
        //            command.Parameters.AddWithValue("@para2", EndDate);
        //            command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[0]);
        //            command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[1]);
        //            command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[2]);
        //            command.Parameters.AddWithValue("@para6", ShiftTimeCtrl[3]);
        //            try
        //            {
        //                List<string> list = new List<string>();
        //                connection.Open();
        //              // string[,,] Array = new string[,,]  ;
        //                using (SqlDataReader result = command.ExecuteReader())
        //                {
        //                    while (result.Read())
        //                    {
        //                         list.Add(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", (string)result[0].ToString(), (string)result[1].ToString(), (string)result[2].ToString(), (string)result[3].ToString(), (string)result[4].ToString(), (string)result[5].ToString(), (string)result[6].ToString(), (string)result[7].ToString(), (string)result[8].ToString()));

        //                    }
        //                }
        //                return list;
        //            }
        //            catch (SqlException ex)
        //            {
        //                ex.ToString();
        //                throw;
        //            }
        //            catch (Exception ex)
        //            {
        //                ex.ToString();
        //                throw;
        //            }

        //        }
        //    }
        //}
        #endregion
        #region OldCode
        //public DataTable RetQuerhyItem(DateTime STDate, DateTime EndDate, string STTime, string EndTime, bool IsOverNightShift)
        //{
        //    string[] ShiftTimeCtrl = new string[4];
        //    if (IsOverNightShift)
        //    {
        //        ShiftTimeCtrl[0] = STTime; //Para3
        //        ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
        //        ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
        //        ShiftTimeCtrl[3] = EndTime; //Para6
        //    }
        //    else
        //    {
        //        ShiftTimeCtrl[0] = STTime; //Para3
        //        ShiftTimeCtrl[1] = EndTime; //Para4
        //        ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
        //        ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6
        //    }
        //    string mdfFilename = @"\ReportDatabase.mdf";
        //    string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    outputFolder = outputFolder + @"\resources";
        //    string attachDbFilename = outputFolder + mdfFilename;
        //    string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand())
        //        {
        //            command.Connection = connection;
        //            command.CommandType = CommandType.Text;
        //            command.CommandText = File.ReadAllText(outputFolder + @"\IGTChartQuery.sql");
        //            command.Parameters.AddWithValue("@para1", STDate.ToString("yyyy/MM/dd"));
        //            command.Parameters.AddWithValue("@para2", EndDate.ToString("yyyy/MM/dd"));
        //            command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[0]);
        //            command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[1]);
        //            command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[2]);
        //            command.Parameters.AddWithValue("@para6", ShiftTimeCtrl[3]);
        //            DataTable dt = new DataTable();
        //            try
        //            {
        //                SqlDataAdapter myAdapter = new SqlDataAdapter();

        //                connection.Open();
        //                // string[,,] Array = new string[,,]  ;
        //                myAdapter.SelectCommand = command;
        //                myAdapter.Fill(dt);

        //                return dt;
        //            }
        //            catch (SqlException ex)
        //            {
        //                nw.IGTOEELog.Info(ex.ToString());
        //                return dt;
        //            }
        //            catch (Exception ex)
        //            {
        //                nw.IGTOEELog.Info(ex.ToString());
        //                return dt;
        //            }

        //        }
        //    }
        //}
        #endregion
        #region NewCOde
        public DataTable RetQuerhyItem(DateTime STDate, DateTime EndDate, string STTime, string EndTime, bool IsOverNightShift)
        {
            string[] ShiftTimeCtrl = new string[4];
            if (IsOverNightShift)
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = EndTime; //Para6
            }
            else
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = EndTime; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6
            }
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = File.ReadAllText(outputFolder + @"\IGTChartQuery.sql");
                    command.Parameters.AddWithValue("@para1", STDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para2", EndDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[0]);
                    command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[1]);
                    command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[2]);
                    command.Parameters.AddWithValue("@para6", ShiftTimeCtrl[3]);
                    DataTable dt = new DataTable();
                    try
                    {
                        SqlDataAdapter myAdapter = new SqlDataAdapter();

                        connection.Open();
                        // string[,,] Array = new string[,,]  ;
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt);

                        return dt;
                    }
                    catch (SqlException ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }

                }
            }
        }
        public DataTable RetQuerhyItem1(DateTime STDate, DateTime EndDate, string STTime, string EndTime, bool IsOverNightShift)
        {
            string[] ShiftTimeCtrl = new string[4];
            if (IsOverNightShift)
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = EndTime; //Para6
            }
            else
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = EndTime; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6
            }
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = File.ReadAllText(outputFolder + @"\IGTChartQuery1.sql");
                    command.Parameters.AddWithValue("@para1", STDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para2", EndDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[0]);
                    command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[1]);
                    command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[2]);
                    command.Parameters.AddWithValue("@para6", ShiftTimeCtrl[3]);
                    DataTable dt = new DataTable();
                    try
                    {
                        SqlDataAdapter myAdapter = new SqlDataAdapter();

                        connection.Open();
                        // string[,,] Array = new string[,,]  ;
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt);

                        return dt;
                    }
                    catch (SqlException ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }

                }
            }
        }
        public DataTable RetQuerhyItem2(DateTime STDate, DateTime EndDate, string STTime, string EndTime, bool IsOverNightShift)
        {
            string[] ShiftTimeCtrl = new string[4];
            if (IsOverNightShift)
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = EndTime; //Para6
            }
            else
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = EndTime; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6
            }
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = File.ReadAllText(outputFolder + @"\IGTChartQuery2.sql");
                    command.Parameters.AddWithValue("@para1", STDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para2", EndDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[0]);
                    command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[1]);
                    command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[2]);
                    command.Parameters.AddWithValue("@para6", ShiftTimeCtrl[3]);
                    DataTable dt = new DataTable();
                    try
                    {
                        SqlDataAdapter myAdapter = new SqlDataAdapter();

                        connection.Open();
                        // string[,,] Array = new string[,,]  ;
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt);

                        return dt;
                    }
                    catch (SqlException ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }

                }
            }
        }
        public DataTable RetQuerhyItem3(DateTime STDate, DateTime EndDate, string STTime, string EndTime, bool IsOverNightShift)
        {
            string[] ShiftTimeCtrl = new string[4];
            if (IsOverNightShift)
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = EndTime; //Para6
            }
            else
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = EndTime; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6
            }
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = File.ReadAllText(outputFolder + @"\IGTChartQuery3.sql");
                    command.Parameters.AddWithValue("@para1", STDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para2", EndDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[0]);
                    command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[1]);
                    command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[2]);
                    command.Parameters.AddWithValue("@para6", ShiftTimeCtrl[3]);
                    DataTable dt = new DataTable();
                    try
                    {
                        SqlDataAdapter myAdapter = new SqlDataAdapter();

                        connection.Open();
                        // string[,,] Array = new string[,,]  ;
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt);

                        return dt;
                    }
                    catch (SqlException ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }

                }
            }
        }
        public DataTable RetRJItem(DateTime STDate, string STTime, string EndTime, bool IsOverNightShift)
        {
            string[] ShiftTimeCtrl = new string[4];
            if (IsOverNightShift)
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = EndTime; //Para6
            }
            else
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = EndTime; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6
            }
            string mdfFilename = @"\ReportDatabase.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = File.ReadAllText(outputFolder + @"\IGTChartQuery4.sql");
                    command.Parameters.AddWithValue("@para1", STDate.ToString("yyyy/MM/dd"));
                    command.Parameters.AddWithValue("@para2", ShiftTimeCtrl[0]);
                    command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[1]);
                    command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[2]);
                    command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[3]);
                    DataTable dt = new DataTable();
                    try
                    {
                        SqlDataAdapter myAdapter = new SqlDataAdapter();

                        connection.Open();
                        // string[,,] Array = new string[,,]  ;
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt);

                        return dt;
                    }
                    catch (SqlException ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return dt;
                    }

                }
            }
        }
   
        #endregion
        public DataSet CurrentYieldReq(DateTime STDate, string STTime, string EndTime, bool IsOverNightShift)
        {
            string[] ShiftTimeCtrl = new string[20];
            if (IsOverNightShift)
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = EndTime; //Para6

                ShiftTimeCtrl[4] = STTime; //Para3
                ShiftTimeCtrl[5] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[6] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[7] = EndTime; //Para6

                ShiftTimeCtrl[8] = STTime; //Para3
                ShiftTimeCtrl[9] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[10] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[11] = EndTime; //Para6

                ShiftTimeCtrl[12] = STTime; //Para3
                ShiftTimeCtrl[13] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[14] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[15] = EndTime; //Para6

                ShiftTimeCtrl[16] = STTime; //Para3
                ShiftTimeCtrl[17] = "11:59:59 PM"; //Para4
                ShiftTimeCtrl[18] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[19] = EndTime; //Para6
            }
            else
            {
                ShiftTimeCtrl[0] = STTime; //Para3
                ShiftTimeCtrl[1] = EndTime; //Para4
                ShiftTimeCtrl[2] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[3] = "00:00:00 AM"; //Para6

                ShiftTimeCtrl[4] = STTime; //Para3
                ShiftTimeCtrl[5] = EndTime; //Para4
                ShiftTimeCtrl[6] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[7] = "00:00:00 AM"; //Para6

                ShiftTimeCtrl[8] = STTime; //Para3
                ShiftTimeCtrl[9] = EndTime; //Para4
                ShiftTimeCtrl[10] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[11] = "00:00:00 AM"; //Para6

                ShiftTimeCtrl[12] = STTime; //Para3
                ShiftTimeCtrl[13] = EndTime; //Para4
                ShiftTimeCtrl[14] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[15] = "00:00:00 AM"; //Para6

                ShiftTimeCtrl[16] = STTime; //Para3
                ShiftTimeCtrl[17] = EndTime; //Para4
                ShiftTimeCtrl[18] = "00:00:00 AM"; //Para5
                ShiftTimeCtrl[19] = "00:00:00 AM"; //Para6

            }
            string mdfFilename = @"\ReportDatabase.mdf";
           
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputFolder = outputFolder + @"\resources";
            string attachDbFilename = outputFolder + mdfFilename;
            string connectionString = string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + attachDbFilename + "; Integrated Security=True");
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    SqlDataAdapter myAdapter = new SqlDataAdapter();
                    DataSet ds = new DataSet();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;

                    try
                    {
                        string LoadSqlScript = File.ReadAllText(outputFolder + @"\SQLQuery1.sql");
                        string[] SqlScript = LoadSqlScript.Split('#');
                        connection.Open();
                        DataTable dt4 = new DataTable("ST4RJ");
                        command.CommandText = SqlScript[0];
                        command.Parameters.AddWithValue("@para1", STDate.ToString("yyyy/MM/dd"));
                        command.Parameters.AddWithValue("@para2", ShiftTimeCtrl[0]);
                        command.Parameters.AddWithValue("@para3", ShiftTimeCtrl[1]);
                        command.Parameters.AddWithValue("@para4", ShiftTimeCtrl[2]);
                        command.Parameters.AddWithValue("@para5", ShiftTimeCtrl[3]);
                        command.Parameters.AddWithValue("@para6", "4");
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt4);
                        ds.Tables.Add(dt4);
                        
                        DataTable dt6 = new DataTable("ST6RJ");
                        command.CommandText = SqlScript[1];
                        command.Parameters.AddWithValue("@para7", STDate.ToString("yyyy/MM/dd"));
                        command.Parameters.AddWithValue("@para8", ShiftTimeCtrl[4]);
                        command.Parameters.AddWithValue("@para9", ShiftTimeCtrl[5]);
                        command.Parameters.AddWithValue("@para10", ShiftTimeCtrl[6]);
                        command.Parameters.AddWithValue("@para11", ShiftTimeCtrl[7]);
                        command.Parameters.AddWithValue("@para12", "6");
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt6);
                        ds.Tables.Add(dt6);
                        string e = STDate.ToString("MM/dd/yyyy");
                        DataTable dt8 = new DataTable("ST8RJ");
                        command.CommandText = SqlScript[2];
                        command.Parameters.AddWithValue("@para13", STDate.ToString("yyyy/MM/dd"));
                        command.Parameters.AddWithValue("@para14", ShiftTimeCtrl[8]);
                        command.Parameters.AddWithValue("@para15", ShiftTimeCtrl[9]);
                        command.Parameters.AddWithValue("@para16", ShiftTimeCtrl[10]);
                        command.Parameters.AddWithValue("@para17", ShiftTimeCtrl[11]);
                        command.Parameters.AddWithValue("@para18", "8");
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt8);
                        ds.Tables.Add(dt8);

                        DataTable dt0 = new DataTable("STJAM");
                        command.CommandText = SqlScript[3];
                        command.Parameters.AddWithValue("@para19", STDate.ToString("yyyy/MM/dd"));
                        command.Parameters.AddWithValue("@para20", ShiftTimeCtrl[12]);
                        command.Parameters.AddWithValue("@para21", ShiftTimeCtrl[13]);
                        command.Parameters.AddWithValue("@para22", ShiftTimeCtrl[14]);
                        command.Parameters.AddWithValue("@para23", ShiftTimeCtrl[15]);
                        command.Parameters.AddWithValue("@para24", "0");
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt0);
                        ds.Tables.Add(dt0);

                        DataTable dt2 = new DataTable("STAQL");
                        command.CommandText = SqlScript[4];
                        command.Parameters.AddWithValue("@para25", STDate.ToString("yyyy/MM/dd"));
                        command.Parameters.AddWithValue("@para26", ShiftTimeCtrl[16]);
                        command.Parameters.AddWithValue("@para27", ShiftTimeCtrl[17]);
                        command.Parameters.AddWithValue("@para28", ShiftTimeCtrl[18]);
                        command.Parameters.AddWithValue("@para29", ShiftTimeCtrl[19]);
                        command.Parameters.AddWithValue("@para30", "2");
                        myAdapter.SelectCommand = command;
                        myAdapter.Fill(dt2);
                        ds.Tables.Add(dt2);
                        ShiftTimeCtrl = null;


                        return ds;

                    }
                    catch (SqlException ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return ds;
                    }
                    catch (Exception ex)
                    {
                        nw.IGTOEELog.Info(ex.ToString());
                        return ds;
                }

                }
            }
        }
    }
}

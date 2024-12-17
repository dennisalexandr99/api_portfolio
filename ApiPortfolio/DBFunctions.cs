using Microsoft.AspNetCore.Hosting;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System.Data;
using System.Globalization;

namespace ApiPortfolio
{
    public class DBFunctions
    {
        private MySqlConnection db = new MySqlConnection();

        public DBFunctions()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var connString = new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("ConnectionStrings")["MySqlConnection"];

            db = new MySqlConnection(connString);
            //db.Open();
        }

        public MySqlConnection GetConnection()
        {
            return db;
        }

        public DataTable Login(string userUniqueId, string password, MySqlConnection SqlConn)
        {
            DataTable tmpDt = new DataTable();
            MySqlCommand mcom = new MySqlCommand("", SqlConn);
            MySqlDataAdapter sDap = new MySqlDataAdapter();

            mcom.Parameters.AddWithValue("@user_unique_id", userUniqueId);
            mcom.Parameters.AddWithValue("@password", password);

            mcom.CommandText = "select u.id_user, r.role_level from user u ";
            mcom.CommandText += "inner join role r on r.id_role = u.id_role ";
            mcom.CommandText += "where u.user_unique_id = @user_unique_id  AND u.user_password = @password;";

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
            if (preparedStatement)
                mcom.Prepare();

            sDap = new MySqlDataAdapter(mcom);
            sDap.Fill(tmpDt);
            return tmpDt;
        }

        public DataTable GetUserInfo(string userUniqueId, string lastId, string roleLevel, MySqlConnection SqlConn)
        {
            DataTable tmpDt = new DataTable();
            MySqlCommand mcom = new MySqlCommand("", SqlConn);
            MySqlDataAdapter sDap = new MySqlDataAdapter();

            mcom.Parameters.AddWithValue("@user_unique_id", userUniqueId);
            mcom.Parameters.AddWithValue("@lastid", lastId);
            mcom.Parameters.AddWithValue("@role_level", roleLevel);

            mcom.CommandText = "select u.id_user, u.user_unique_id ,u.user_fullname ,u.user_fullname ,u.user_email ";
            mcom.CommandText += ",r.id_role ,r.role_name ,r.role_level ";
            mcom.CommandText += "from user u ";
            mcom.CommandText += "inner join role r on u.id_role = r.id_role  ";
            mcom.CommandText += "where 1=1 ";
            if (lastId != "")
                mcom.CommandText += "AND u.id_user = @lastid ";
            if (userUniqueId != "")
                mcom.CommandText += "AND u.user_unique_id = @user_unique_id ";
            if (roleLevel != "")
                mcom.CommandText += "AND r.role_level >= @role_level ";

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
            if (preparedStatement)
                mcom.Prepare();

            sDap = new MySqlDataAdapter(mcom);
            sDap.Fill(tmpDt);
            return tmpDt;
        }

        public DataTable GetRoleList(string idRole, MySqlConnection SqlConn)
        {
            DataTable tmpDt = new DataTable();
            MySqlCommand mcom = new MySqlCommand("", SqlConn);
            MySqlDataAdapter sDap = new MySqlDataAdapter();

            mcom.Parameters.AddWithValue("@id_role", idRole);

            mcom.CommandText = "select id_role, role_name, role_level ";
            mcom.CommandText += "from role ";
            mcom.CommandText += "where 1=1 ";
            if (idRole != "")
                mcom.CommandText += "AND id_role = @id_role ";

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
            if (preparedStatement)
                mcom.Prepare();

            sDap = new MySqlDataAdapter(mcom);
            sDap.Fill(tmpDt);
            return tmpDt;
        }



        public string CreateNewUser(string userUniqueId, string userFullName, string userEmail, string userPassword, int idRole, MySqlTransaction myTrans, MySqlConnection sqlConn)
        {
            DataTable tmpDt = new DataTable();
            MySqlCommand mcom = new MySqlCommand("", sqlConn);
            MySqlDataAdapter sDap = new MySqlDataAdapter();
            bool tmpResult = false;
            string lastId = "";

            try
            {
                mcom.Transaction = myTrans;

                mcom.Parameters.AddWithValue("@user_unique_id", userUniqueId);
                mcom.Parameters.AddWithValue("@user_fullname", userFullName);
                mcom.Parameters.AddWithValue("@user_email", userEmail);
                mcom.Parameters.AddWithValue("@user_password", userPassword);
                mcom.Parameters.AddWithValue("@id_role", idRole);

                mcom.CommandText = "INSERT INTO `user` (user_unique_id,user_fullname,user_email,user_password,id_role) ";
                mcom.CommandText += "VALUES (@user_unique_id,@user_fullname,@user_email,@user_password,@id_role)";

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
                if (preparedStatement)
                    mcom.Prepare();

                if (mcom.ExecuteNonQuery() == 1)
                    tmpResult = true;
                else
                    tmpResult = false;

                if (tmpResult)
                {
                    mcom.CommandText = "SELECT LAST_INSERT_ID();";
                    if (preparedStatement)
                        mcom.Prepare();

                    lastId = mcom.ExecuteScalar().ToString();
                    return lastId;
                }
                else
                {
                    return "";
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GetCurrentPassword(string uniqueId, MySqlConnection SqlConn)
        {
            DataTable tmpDt = new DataTable();
            string result = "";
            MySqlDataAdapter sDap = new MySqlDataAdapter();
            MySqlCommand mcom = new MySqlCommand("", SqlConn);

            mcom.Parameters.AddWithValue("@unique_id", uniqueId);

            mcom.CommandText = "SELECT user_password FROM `user` WHERE user_unique_id = @unique_id; ";

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
            if (preparedStatement)
                mcom.Prepare();

            result = mcom.ExecuteScalar().ToString();
            return result;
        }

        public bool EditUser(string targetUniqueId, string newUserFullname, string newUserEmail, string newUserPassword, int newUserRole, MySqlTransaction myTrans, MySqlConnection sqlConn)
        {
            MySqlCommand mcom = new MySqlCommand("", sqlConn);
            mcom.Transaction = myTrans;

            try
            {
                mcom.Parameters.AddWithValue("@target_unique_id", targetUniqueId);
                mcom.Parameters.AddWithValue("@new_user_fullname", newUserFullname);
                mcom.Parameters.AddWithValue("@new_user_email", newUserEmail);
                mcom.Parameters.AddWithValue("@new_user_password", newUserPassword);
                mcom.Parameters.AddWithValue("@new_user_role", newUserRole);

                mcom.CommandText = "UPDATE `user` ";
                mcom.CommandText += "SET user_fullname = @new_user_fullname, ";
                mcom.CommandText += "user_email = @new_user_email, ";
                mcom.CommandText += "user_password = @new_user_password, ";
                mcom.CommandText += "id_role = @new_user_role ";
                mcom.CommandText += "WHERE user_unique_id = @target_unique_id";

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
                if (preparedStatement)
                    mcom.Prepare();

                mcom.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool DeleteUser(string targetUniqueId, MySqlTransaction myTrans, MySqlConnection sqlConn)
        {
            MySqlCommand mcom = new MySqlCommand("", sqlConn);
            mcom.Transaction = myTrans;

            try
            {
                mcom.Parameters.AddWithValue("@target_unique_id", targetUniqueId);

                mcom.CommandText = "DELETE FROM `user` ";
                mcom.CommandText += "WHERE user_unique_id = @target_unique_id";

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
                if (preparedStatement)
                    mcom.Prepare();

                mcom.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string CreateTicket(string title, string description, string pictureFileName, string userUniqueId, MySqlTransaction myTrans, MySqlConnection sqlConn)
        {
            DataTable tmpDt = new DataTable();
            MySqlCommand mcom = new MySqlCommand("", sqlConn);
            MySqlDataAdapter sDap = new MySqlDataAdapter();
            bool tmpResult = false;
            string lastId = "";

            try
            {
                mcom.Transaction = myTrans;

                mcom.Parameters.AddWithValue("@title", title);
                mcom.Parameters.AddWithValue("@description", description);
                mcom.Parameters.AddWithValue("@picture_file_name", pictureFileName);
                mcom.Parameters.AddWithValue("@user_unique_id", userUniqueId);

                mcom.CommandText = "INSERT INTO `ticket` (title,description,picture,created_by,create_time) ";
                mcom.CommandText += "VALUES (@title,@description,@picture_file_name,@user_unique_id,NOW())";

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
                if (preparedStatement)
                    mcom.Prepare();

                if (mcom.ExecuteNonQuery() == 1)
                    tmpResult = true;
                else
                    tmpResult = false;

                if (tmpResult)
                {
                    mcom.CommandText = "SELECT LAST_INSERT_ID();";
                    if (preparedStatement)
                        mcom.Prepare();

                    lastId = mcom.ExecuteScalar().ToString();
                    return lastId;
                }
                else
                {
                    return "";
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public DataTable GetTicket(string userRoleLevel, string idTicket, string orderBy, string orderType, string limit, string lastIndex, MySqlConnection SqlConn)
        {
            DataTable tmpDt = new DataTable();
            MySqlCommand mcom = new MySqlCommand("", SqlConn);
            MySqlDataAdapter sDap = new MySqlDataAdapter();

            mcom.Parameters.AddWithValue("@user_role_level", userRoleLevel);
            mcom.Parameters.AddWithValue("@id_ticket", idTicket);
            mcom.Parameters.AddWithValue("@order_by", orderBy);
            mcom.Parameters.AddWithValue("@order_type", orderType);

            mcom.CommandText = "select t.id_ticket, t.title, t.description, t.picture, t.created_by, t.create_time, t.update_time ";
            mcom.CommandText += ",r.id_role ,r.role_name ,r.role_level ";
            mcom.CommandText += ",u.id_user ,u.user_unique_id ,u.user_fullname ,u.user_email ";
            mcom.CommandText += "from ticket t ";
            mcom.CommandText += "inner join user u on u.user_unique_id = t.created_by  ";
            mcom.CommandText += "inner join role r on u.id_role = r.id_role  ";
            mcom.CommandText += "where 1=1 ";
            mcom.CommandText += "AND r.role_level >= @user_role_level ";
            mcom.CommandText += "AND t.delete_time IS NULL ";

            if (idTicket != "")
                mcom.CommandText += "AND t.id_ticket = @id_ticket ";
            if (userRoleLevel != "")
                mcom.CommandText += "AND r.role_level >= @user_role_level ";

            if (orderType != "" && orderBy !="")
            {
                if (limit == "")
                    mcom.CommandText += "ORDER BY t." + orderBy + " " + orderType + " ";
                else
                    mcom.CommandText += "ORDER BY t." + orderBy + " " + orderType + " LIMIT " + lastIndex + "," + limit + " ";
            }          

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
            if (preparedStatement)
                mcom.Prepare();

            sDap = new MySqlDataAdapter(mcom);
            sDap.Fill(tmpDt);
            return tmpDt;
        }

        public bool EditTicket(string idTicket, string newTitle, string newDescription, string newPictureFileName, MySqlTransaction myTrans, MySqlConnection sqlConn)
        {
            MySqlCommand mcom = new MySqlCommand("", sqlConn);
            mcom.Transaction = myTrans;

            try
            {
                mcom.Parameters.AddWithValue("@id_ticket", idTicket);
                mcom.Parameters.AddWithValue("@new_title", newTitle);
                mcom.Parameters.AddWithValue("@new_description", newDescription);
                mcom.Parameters.AddWithValue("@new_picture_file_name", newPictureFileName);

                mcom.CommandText = "UPDATE `ticket` ";
                mcom.CommandText += "SET title = @new_title, ";
                mcom.CommandText += "description = @new_description, ";
                mcom.CommandText += "picture = @new_picture_file_name, ";
                mcom.CommandText += "update_time = NOW() ";
                mcom.CommandText += "WHERE id_ticket = @id_ticket";

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
                if (preparedStatement)
                    mcom.Prepare();

                mcom.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool DeleteTicket(string idTicket, MySqlTransaction myTrans, MySqlConnection sqlConn)
        {
            MySqlCommand mcom = new MySqlCommand("", sqlConn);
            mcom.Transaction = myTrans;

            try
            {
                mcom.Parameters.AddWithValue("@id_ticket", idTicket);

                mcom.CommandText = "UPDATE `ticket` ";
                mcom.CommandText += "SET delete_time = NOW() ";
                mcom.CommandText += "WHERE id_ticket = @id_ticket";

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool preparedStatement = bool.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["UsePreparedStatement"]);
                if (preparedStatement)
                    mcom.Prepare();

                mcom.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

using MySql.Data.MySqlClient;
using System.Data;

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
    }
}

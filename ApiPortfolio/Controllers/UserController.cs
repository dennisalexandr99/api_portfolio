using ApiPortfolio.Models.Response;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace ApiPortfolio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly ClassUtility _utility;
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public UserController(ClassUtility classUtility)
        {
            _utility = classUtility;
        }

        [Route("[action]", Name = "Login")]
        [HttpGet]
        public IActionResult Login(string userUniqueId, string password)
        {
            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtLogin = new DataTable();

            ClassResponseLogin response = new ClassResponseLogin();

            try
            {
                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                string key = new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["Sha256Key"];
                password = _utility.ComputeHmacSha256Base64(password, key);

                dtLogin = db.Login(userUniqueId, password, sqlConn);
                if (dtLogin.Rows.Count > 0)
                {
                    Response.StatusCode = 200;
                    response.status = "success";

                    string tmpRoleLevel = _utility.NullToString(dtLogin.Rows[0]["role_level"]);

                    response.data = new Models.ClassLogin();
                    response.data.jwtToken = TokenManager.GenerateJwtToken(
                        new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["JWTKey"]
                        , "issuer"
                        , "audience"
                        , int.Parse(new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Configurations")["JWTExpiresInMinutes"])
                        , userUniqueId
                        , tmpRoleLevel);

                }
                else
                {
                    Response.StatusCode = 401;
                    response.status = "fail";
                    response.message = "wrong username or password";
                    _utility.FileTraceLog("Fail login attempt : " + "Login, userUniqueId : " + userUniqueId);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "Login " + e.Message + "userUniqueId : " + userUniqueId);
            }
            finally
            {
                sqlConn.Close();
            }

            return Json(response);
        }

        [Route("[action]", Name = "GetUserInfo")]
        [HttpGet]
        public IActionResult GetUserInfo()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtUser = new DataTable();
            string[] jwtInformations = null;

            ClassResponseUser response = new ClassResponseUser();

            try
            {
                TokenManager token = new TokenManager();
                //validate jwt
                if (authorizationHeader != null)
                    jwtInformations = token.ValidateJwtToken(authorizationHeader, new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["JWTKey"]);
                if (jwtInformations == null)
                {
                    Response.StatusCode = 401;
                    response.status = "unauthorized";
                    response.message = "unauthorized";
                    return Json(response);
                }

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                dtUser = db.GetUserInfo(jwtInformations[0], "", "", sqlConn);
                if (dtUser.Rows.Count > 0)
                {
                    Response.StatusCode = 200;
                    response.status = "success";
                    response.message = "data fetched successfully";

                    response.data = new Models.ClassUser();
                    response.data.id_user = _utility.NullToInt(dtUser.Rows[0]["id_user"]);
                    response.data.user_unique_id = _utility.NullToString(dtUser.Rows[0]["user_unique_id"]);
                    response.data.user_email = _utility.NullToString(dtUser.Rows[0]["user_email"]);
                    response.data.user_full_name = _utility.NullToString(dtUser.Rows[0]["user_fullname"]);

                    response.data.role = new Models.ClassRole();
                    response.data.role.id_role = _utility.NullToInt(dtUser.Rows[0]["id_role"]);
                    response.data.role.role_name = _utility.NullToString(dtUser.Rows[0]["role_name"]);
                    response.data.role.role_level = _utility.NullToInt(dtUser.Rows[0]["role_level"]);
                }
                else
                {
                    Response.StatusCode = 404;
                    response.status = "fail";
                    response.message = "data not found";
                    _utility.FileTraceLog("Datanotfound : " + "GetUserInfo, userUniqueId : " + jwtInformations[0]);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "GetUserInfo " + e.Message + "userUniqueId : " + jwtInformations[0]);
            }
            finally
            {
                sqlConn.Close();
            }

            return Json(response);
        }

        [Route("[action]", Name = "GetUserListByPrivilege")]
        [HttpGet]
        public IActionResult GetUserListByPrivilege()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtUser = new DataTable();
            string[] jwtInformations = null;

            ClassResponseUserLists response = new ClassResponseUserLists();

            try
            {
                TokenManager token = new TokenManager();
                //validate jwt
                if (authorizationHeader != null)
                    jwtInformations = token.ValidateJwtToken(authorizationHeader, new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["JWTKey"]);
                if (jwtInformations == null)
                {
                    Response.StatusCode = 401;
                    response.status = "unauthorized";
                    response.message = "unauthorized";
                    return Json(response);
                }

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                dtUser = db.GetUserInfo("", "", jwtInformations[1], sqlConn);
                if (dtUser.Rows.Count > 0)
                {
                    Response.StatusCode = 200;
                    response.status = "success";
                    response.message = "data fetched successfully";

                    response.data = new Models.ClassUser[dtUser.Rows.Count];
                    for (int i = 0; i < dtUser.Rows.Count; i++)
                    {
                        response.data[i] = new Models.ClassUser();
                        response.data[i].id_user = _utility.NullToInt(dtUser.Rows[i]["id_user"]);
                        response.data[i].user_unique_id = _utility.NullToString(dtUser.Rows[i]["user_unique_id"]);
                        response.data[i].user_email = _utility.NullToString(dtUser.Rows[i]["user_email"]);
                        response.data[i].user_full_name = _utility.NullToString(dtUser.Rows[i]["user_fullname"]);

                        response.data[i].role = new Models.ClassRole();
                        response.data[i].role.id_role = _utility.NullToInt(dtUser.Rows[i]["id_role"]);
                        response.data[i].role.role_name = _utility.NullToString(dtUser.Rows[i]["role_name"]);
                        response.data[i].role.role_level = _utility.NullToInt(dtUser.Rows[i]["role_level"]);
                    }
                }
                else
                {
                    Response.StatusCode = 404;
                    response.status = "fail";
                    response.message = "data not found";
                    _utility.FileTraceLog("Datanotfound : " + "GetUserInfo, userUniqueId : " + jwtInformations[0]);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "GetUserLists " + e.Message + "userUniqueId : " + jwtInformations[0]);
            }
            finally
            {
                sqlConn.Close();
            }

            return Json(response);
        }

        [Route("[action]", Name = "CreateNewUser")]
        [HttpPost]
        public IActionResult CreateNewUser(string userUniqueId, string userFullName, string userEmail, string userPassword, int idRole)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string[] jwtInformations = null;

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtUser = new DataTable();
            DataTable tmpDtLevel = new DataTable();

            ClassResponseUser response = new ClassResponseUser();

            if (sqlConn.State == ConnectionState.Closed)
                sqlConn.Open();
            MySqlTransaction myTrans = sqlConn.BeginTransaction();

            try
            {
                TokenManager token = new TokenManager();
                //validate jwt
                if (authorizationHeader != null)
                    jwtInformations = token.ValidateJwtToken(authorizationHeader, new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["JWTKey"]);
                if (jwtInformations == null)
                {
                    Response.StatusCode = 401;
                    response.status = "unauthorized";
                    response.message = "unauthorized";
                    return Json(response);
                }

                if (!_utility.IsValidUniqueId(userUniqueId))
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "unique id must be all number and 8 digits long";
                    return Json(response);
                }

                if (!_utility.IsValidEmail(userEmail))
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "email address must be in a correct format";
                    return Json(response);
                }

                //user only able to create role same or lower than their level
                tmpDtLevel = db.GetUserInfo(jwtInformations[0], "", "", sqlConn);
                int userLevel = 0;
                if (tmpDtLevel.Rows.Count > 0)
                    userLevel = _utility.NullToInt(tmpDtLevel.Rows[0]["role_level"]);


                if (idRole < int.Parse(jwtInformations[1]))
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "you only able to create an user level below you or same as you (" + jwtInformations[1] + ")";
                    return Json(response);
                }

                string key = new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["Sha256Key"];
                userPassword = _utility.ComputeHmacSha256Base64(userPassword, key);

                string lastId = db.CreateNewUser(userUniqueId, userFullName, userEmail, userPassword, idRole, myTrans, sqlConn);
                if (lastId == "" || lastId == null)
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "user not created, please check contact administrator";
                    _utility.FileTraceLog("User not created : " + "CreateUser, userUniqueId : " + userUniqueId);
                    return Json(response);
                }

                dtUser = db.GetUserInfo("", lastId, "", sqlConn);
                if (dtUser.Rows.Count > 0)
                {
                    Response.StatusCode = 200;
                    response.status = "success";
                    response.message = "user created successfully";

                    response.data = new Models.ClassUser();
                    response.data.id_user = _utility.NullToInt(dtUser.Rows[0]["id_user"]);
                    response.data.user_unique_id = _utility.NullToString(dtUser.Rows[0]["user_unique_id"]);
                    response.data.user_email = _utility.NullToString(dtUser.Rows[0]["user_email"]);
                    response.data.user_full_name = _utility.NullToString(dtUser.Rows[0]["user_fullname"]);

                    response.data.role = new Models.ClassRole();
                    response.data.role.id_role = _utility.NullToInt(dtUser.Rows[0]["id_role"]);
                    response.data.role.role_name = _utility.NullToString(dtUser.Rows[0]["role_name"]);
                    response.data.role.role_level = _utility.NullToInt(dtUser.Rows[0]["role_level"]);
                }
                else
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "user not created, please check contact administrator";
                    _utility.FileTraceLog("User not created : " + "CreateUser, userUniqueId : " + userUniqueId);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "GetUserInfo " + e.Message + "userUniqueId : " + userUniqueId);
            }
            finally
            {
                if (response.status == "success")
                {
                    myTrans.Commit();
                }
                else
                {
                    try
                    {
                        myTrans.Rollback();
                    }
                    catch (MySqlException exT)
                    {

                    }
                }
                sqlConn.Close();
            }

            return Json(response);
        }

        [Route("[action]", Name = "EditUser")]
        [HttpPost]
        public IActionResult EditUser(string targetUserUniqueId, string newUserFullName, string newUserEmail, string oldPassword, string newUserPassword, int newIdRole)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string[] jwtInformations = null;

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtUser = new DataTable();
            DataTable tmpDtLevel = new DataTable();

            ClassResponseUser response = new ClassResponseUser();

            if (sqlConn.State == ConnectionState.Closed)
                sqlConn.Open();
            MySqlTransaction myTrans = sqlConn.BeginTransaction();

            try
            {
                TokenManager token = new TokenManager();
                //validate jwt
                if (authorizationHeader != null)
                    jwtInformations = token.ValidateJwtToken(authorizationHeader, new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["JWTKey"]);
                if (jwtInformations==null)
                {
                    Response.StatusCode = 401;
                    response.status = "unauthorized";
                    response.message = "unauthorized";
                    return Json(response);
                }

                if (!_utility.IsValidEmail(newUserEmail))
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "email address must be in a correct format";
                    return Json(response);
                }

                //user only able to edit role same or lower than their level
                //tmpDtLevel = db.GetUserInfo(jwtInformations[0], "", "", sqlConn);
                //int userLevel = 0;
                //if (tmpDtLevel.Rows.Count > 0)
                //    userLevel = _utility.NullToInt(tmpDtLevel.Rows[0]["role_level"]);

                if (newIdRole < int.Parse(jwtInformations[1]))
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "you only able to edit a user level below you or same as you (" + jwtInformations[1] + ")";
                    return Json(response);
                }

                string key = new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["Sha256Key"];
                oldPassword = _utility.ComputeHmacSha256Base64(oldPassword, key);

                //get old password first for confirmation
                string currentPassword = db.GetCurrentPassword(targetUserUniqueId, sqlConn);
                if (oldPassword != currentPassword)
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "wrong old password";
                    return Json(response);
                }

                //create new password
                newUserPassword = _utility.ComputeHmacSha256Base64(newUserPassword, key);


                bool editUser = db.EditUser(targetUserUniqueId, newUserFullName, newUserEmail, newUserPassword, newIdRole, myTrans, sqlConn);
                if (editUser == false)
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "user not edited, please check contact administrator";
                    _utility.FileTraceLog("User not edited : " + "CreateUser, userUniqueId : " + targetUserUniqueId);
                    return Json(response);
                }
                else
                {
                    dtUser = db.GetUserInfo(targetUserUniqueId, "", "", sqlConn);
                    if (dtUser.Rows.Count > 0)
                    {
                        Response.StatusCode = 200;
                        response.status = "success";
                        response.message = "user edited successfully";

                        response.data = new Models.ClassUser();
                        response.data.id_user = _utility.NullToInt(dtUser.Rows[0]["id_user"]);
                        response.data.user_unique_id = _utility.NullToString(dtUser.Rows[0]["user_unique_id"]);
                        response.data.user_email = _utility.NullToString(dtUser.Rows[0]["user_email"]);
                        response.data.user_full_name = _utility.NullToString(dtUser.Rows[0]["user_fullname"]);

                        response.data.role = new Models.ClassRole();
                        response.data.role.id_role = _utility.NullToInt(dtUser.Rows[0]["id_role"]);
                        response.data.role.role_name = _utility.NullToString(dtUser.Rows[0]["role_name"]);
                        response.data.role.role_level = _utility.NullToInt(dtUser.Rows[0]["role_level"]);
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        response.status = "fail";
                        response.message = "user not found";
                        _utility.FileTraceLog("User not created : " + "EditUser, userUniqueId : " + targetUserUniqueId);

                    }
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "EditUser " + e.Message + "userUniqueId : " + targetUserUniqueId);
            }
            finally
            {
                if (response.status == "success")
                {
                    myTrans.Commit();
                }
                else
                {
                    try
                    {
                        myTrans.Rollback();
                    }
                    catch (MySqlException exT)
                    {

                    }
                }
                sqlConn.Close();
            }

            return Json(response);
        }

        [Route("[action]", Name = "DeleteUser")]
        [HttpPost]
        public IActionResult DeleteUser(string targetUserUniqueId)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string[] jwtInformations = null;

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtUser = new DataTable();
            DataTable tmpDtLevel = new DataTable();

            ClassResponseUser response = new ClassResponseUser();

            if (sqlConn.State == ConnectionState.Closed)
                sqlConn.Open();
            MySqlTransaction myTrans = sqlConn.BeginTransaction();

            try
            {
                TokenManager token = new TokenManager();
                //validate jwt
                if (authorizationHeader != null)
                    jwtInformations = token.ValidateJwtToken(authorizationHeader, new ConfigurationBuilder().AddJsonFile("appsettings." + environment + ".json").Build().GetSection("Secret")["JWTKey"]);
                if (jwtInformations == null)
                {
                    Response.StatusCode = 401;
                    response.status = "unauthorized";
                    response.message = "unauthorized";
                    return Json(response);
                }

                //user only able to edit role same or lower than their level
                tmpDtLevel = db.GetUserInfo(targetUserUniqueId, "", "", sqlConn);
                int userLevel = 0;
                if (tmpDtLevel.Rows.Count > 0)
                    userLevel = _utility.NullToInt(tmpDtLevel.Rows[0]["role_level"]);
                else
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "user not found";
                    return Json(response);
                }

                if (userLevel < int.Parse(jwtInformations[1]))
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "you only able to delete an user level below you or same as you (" + jwtInformations[1] + ")";
                    return Json(response);
                }

                bool deleteUser = db.DeleteUser(targetUserUniqueId, myTrans, sqlConn);
                if (deleteUser == false)
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "user not deleted, please check contact administrator";
                    _utility.FileTraceLog("User not deleted : " + "DeleteUser, userUniqueId : " + targetUserUniqueId);
                    return Json(response);
                }
                else
                {
                    Response.StatusCode = 200;
                    response.status = "success";
                    response.message = "user deleted successfully";
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "DeleteUser " + e.Message + "userUniqueId : " + targetUserUniqueId);
            }
            finally
            {
                if (response.status == "success")
                {
                    myTrans.Commit();
                }
                else
                {
                    try
                    {
                        myTrans.Rollback();
                    }
                    catch (MySqlException exT)
                    {

                    }
                }
                sqlConn.Close();
            }

            return Json(response);
        }
    }
}

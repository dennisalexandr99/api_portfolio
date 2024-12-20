using ApiPortfolio.Models.Response;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;

namespace ApiPortfolio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : Controller
    {
        private readonly ClassUtility _utility;
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public TicketController(ClassUtility classUtility)
        {
            _utility = classUtility;
        }

        [Route("[action]", Name = "CreateTicket")]
        [HttpPost]
        public IActionResult CreateTicket(string title, string description, string pictureFileName)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string[] jwtInformations = null;

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtTicket = new DataTable();
            DataTable tmpDtLevel = new DataTable();

            string url = HttpContext.Request.GetDisplayUrl();
            url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}" + "/";

            ClassResponseTicket response = new ClassResponseTicket();

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

                string lastId = db.CreateTicket(title, description, pictureFileName, jwtInformations[0], myTrans, sqlConn);
                if (lastId == "" || lastId == null)
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "ticket not created, please check contact administrator";
                    _utility.FileTraceLog("Ticket not created : " + "CreateTicket, userUniqueId : " + jwtInformations[0]);
                    return Json(response);
                }

                //GetTicket(string userRoleLevel, string idTicket, string orderBy, string orderType, string limit, string lastIndex, MySqlConnection SqlConn)
                dtTicket = db.GetTicket(jwtInformations[1], lastId, "", "", "1", "", sqlConn);
                if (dtTicket.Rows.Count > 0)
                {
                    Response.StatusCode = 200;
                    response.status = "success";
                    response.message = "ticket created successfully";

                    response.data = new Models.ClassTicketHeader();
                    response.data.limit = 1;
                    response.data.last_index = 0;
                    response.data.order_by = "";
                    response.data.order_type = "";

                    response.data.tickets = new Models.ClassTicket[1];
                    response.data.tickets[0] = new Models.ClassTicket();
                    response.data.tickets[0].id_ticket = int.Parse(_utility.NullToString(dtTicket.Rows[0]["id_ticket"]));
                    response.data.tickets[0].title = _utility.NullToString(dtTicket.Rows[0]["title"]);
                    response.data.tickets[0].description = _utility.NullToString(dtTicket.Rows[0]["description"]);
                    response.data.tickets[0].picture_url = url + "picture_uploads/medium/" + _utility.NullToString(dtTicket.Rows[0]["picture"]);
                    response.data.tickets[0].created_by = _utility.NullToString(dtTicket.Rows[0]["created_by"]);
                    if (_utility.NullToString(dtTicket.Rows[0]["create_time"]) != "")
                        response.data.tickets[0].create_time = DateTime.Parse(_utility.NullToString(dtTicket.Rows[0]["create_time"]));
                    if (_utility.NullToString(dtTicket.Rows[0]["update_time"]) != "")
                        response.data.tickets[0].update_time = DateTime.Parse(_utility.NullToString(dtTicket.Rows[0]["update_time"]));

                    response.data.tickets[0].user = new Models.ClassUser();
                    response.data.tickets[0].user.id_user = _utility.NullToInt(dtTicket.Rows[0]["id_user"]);
                    response.data.tickets[0].user.user_unique_id = _utility.NullToString(dtTicket.Rows[0]["user_unique_id"]);
                    response.data.tickets[0].user.user_full_name = _utility.NullToString(dtTicket.Rows[0]["user_fullname"]);
                    response.data.tickets[0].user.user_email = _utility.NullToString(dtTicket.Rows[0]["user_email"]);

                    response.data.tickets[0].user.role = new Models.ClassRole();
                    response.data.tickets[0].user.role.id_role = _utility.NullToInt(dtTicket.Rows[0]["id_role"]);
                    response.data.tickets[0].user.role.role_name = _utility.NullToString(dtTicket.Rows[0]["role_name"]);
                    response.data.tickets[0].user.role.role_level = _utility.NullToInt(dtTicket.Rows[0]["role_level"]);
                }
                else
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "ticket not created, please check contact administrator";
                    _utility.FileTraceLog("ticket not created : " + "CreateTicket, userUniqueId : " + jwtInformations[0]);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "CreateTicket " + e.Message + "userUniqueId : " + jwtInformations[0]);
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

        [Route("[action]", Name = "EditTicket")]
        [HttpPost]
        public IActionResult EditTicket(string idTicket, string newTitle, string newDescription, string newPictureFileName)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string[] jwtInformations = null;

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtTicket = new DataTable();
            DataTable tmpDtLevel = new DataTable();

            string url = HttpContext.Request.GetDisplayUrl();
            url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}" + "/";

            ClassResponseTicket response = new ClassResponseTicket();

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

                bool editTicket = db.EditTicket(idTicket, newTitle, newDescription, newPictureFileName, myTrans, sqlConn);
                if (editTicket == false)
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "ticket not created, please check contact administrator";
                    _utility.FileTraceLog("Ticket not created : " + "CreateTicket, userUniqueId : " + jwtInformations[0]);
                    return Json(response);
                }
                else
                {
                    //GetTicket(string userRoleLevel, string idTicket, string orderBy, string orderType, string limit, string lastIndex, MySqlConnection SqlConn)
                    dtTicket = db.GetTicket("", idTicket, "", "", "1", "", sqlConn);
                    if (dtTicket.Rows.Count > 0)
                    {
                        Response.StatusCode = 200;
                        response.status = "success";
                        response.message = "ticket created successfully";

                        response.data = new Models.ClassTicketHeader();
                        response.data.limit = 1;
                        response.data.last_index = 0;
                        response.data.order_by = "";
                        response.data.order_type = "";

                        response.data.tickets = new Models.ClassTicket[1];
                        response.data.tickets[0] = new Models.ClassTicket();
                        response.data.tickets[0].id_ticket = int.Parse(_utility.NullToString(dtTicket.Rows[0]["id_ticket"]));
                        response.data.tickets[0].title = _utility.NullToString(dtTicket.Rows[0]["title"]);
                        response.data.tickets[0].description = _utility.NullToString(dtTicket.Rows[0]["description"]);
                        response.data.tickets[0].picture_url = url + "picture_uploads/medium/" + _utility.NullToString(dtTicket.Rows[0]["picture"]);
                        response.data.tickets[0].created_by = _utility.NullToString(dtTicket.Rows[0]["created_by"]);
                        if (_utility.NullToString(dtTicket.Rows[0]["create_time"]) != "")
                            response.data.tickets[0].create_time = DateTime.Parse(_utility.NullToString(dtTicket.Rows[0]["create_time"]));
                        if (_utility.NullToString(dtTicket.Rows[0]["update_time"]) != "")
                            response.data.tickets[0].update_time = DateTime.Parse(_utility.NullToString(dtTicket.Rows[0]["update_time"]));

                        response.data.tickets[0].user = new Models.ClassUser();
                        response.data.tickets[0].user.id_user = _utility.NullToInt(dtTicket.Rows[0]["id_user"]);
                        response.data.tickets[0].user.user_unique_id = _utility.NullToString(dtTicket.Rows[0]["user_unique_id"]);
                        response.data.tickets[0].user.user_full_name = _utility.NullToString(dtTicket.Rows[0]["user_fullname"]);
                        response.data.tickets[0].user.user_email = _utility.NullToString(dtTicket.Rows[0]["user_email"]);

                        response.data.tickets[0].user.role = new Models.ClassRole();
                        response.data.tickets[0].user.role.id_role = _utility.NullToInt(dtTicket.Rows[0]["id_role"]);
                        response.data.tickets[0].user.role.role_name = _utility.NullToString(dtTicket.Rows[0]["role_name"]);
                        response.data.tickets[0].user.role.role_level = _utility.NullToInt(dtTicket.Rows[0]["role_level"]);
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        response.status = "fail";
                        response.message = "ticket not edited, please check contact administrator";
                        _utility.FileTraceLog("ticket not edited : " + "EditTicket, userUniqueId : " + jwtInformations[0]);
                    }
                }

            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "EditTicket " + e.Message + "userUniqueId : " + jwtInformations[0]);
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

        [Route("[action]", Name = "DeleteTicket")]
        [HttpPost]
        public IActionResult DeteleTicket(string idTicket)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string[] jwtInformations = null;

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtTicket = new DataTable();
            DataTable tmpDtLevel = new DataTable();

            ClassResponseTicket response = new ClassResponseTicket();

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

                bool editTicket = db.DeleteTicket(idTicket, myTrans, sqlConn);
                if (editTicket == false)
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "ticket not deleted, please check contact administrator";
                    _utility.FileTraceLog("Ticket not created : " + "CreateTicket, userUniqueId : " + jwtInformations[0]);
                    return Json(response);
                }
                else
                {
                    Response.StatusCode = 200;
                    response.status = "success";
                    response.message = "ticket deleted successfully";
                }

            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "DeleteTicket " + e.Message + "userUniqueId : " + jwtInformations[0]);
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


        [Route("[action]", Name = "UploadPicture")]
        [HttpPost]
        public IActionResult UploadPicture(IFormFile pictureFile)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string[] jwtInformations = null;

            DBFunctions db = new DBFunctions();
            DataTable dtTicket = new DataTable();
            DataTable tmpDtLevel = new DataTable();
            string pictureName = "Pic" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";

            string url = HttpContext.Request.GetDisplayUrl();
            url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}" + "/";

            ClassResponsePicture response = new ClassResponsePicture();

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

                if (_utility.UploadFile(pictureFile, pictureName))
                {
                    response.status = "200";
                    response.data = new Models.ClassPicture();
                    response.data.picture_file_name = url + "picture_uploads/medium/" + pictureName;
                    response.message = "success";
                }
                else
                {
                    Response.StatusCode = 404;
                    response.status = "fail";
                    response.message = "failed to upload a picture";
                    _utility.FileTraceLog("UploadPicture : " + "UploadPicture, userUniqueId : " + jwtInformations[0]);
                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "UploadPicture " + e.Message + "userUniqueId : " + jwtInformations[0]);
            }

            return Json(response);
        }



        [Route("[action]", Name = "GetTicketListByPrivilege")]
        [HttpGet]
        public IActionResult GetTicketListByPrivilege(string orderBy, string orderType, string limit, string lastIndex)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            DBFunctions db = new DBFunctions();
            MySqlConnection sqlConn = db.GetConnection();
            DataTable dtTicket = new DataTable();
            string[] jwtInformations = null;

            string url = HttpContext.Request.GetDisplayUrl();
            url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}" + "/";

            ClassResponseTicket response = new ClassResponseTicket();

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

                //GetTicket(string userRoleLevel, string idTicket, string orderBy, string orderType, string limit, string lastIndex, MySqlConnection SqlConn)
                dtTicket = db.GetTicket(jwtInformations[1], "", orderBy, orderType, limit, lastIndex, sqlConn);
                if (dtTicket.Rows.Count > 0)
                {
                    Response.StatusCode = 200;
                    response.status = "success";

                    response.data = new Models.ClassTicketHeader();
                    response.data.limit = 1;
                    response.data.last_index = 0;
                    response.data.order_by = "";
                    response.data.order_type = "";

                    response.data.tickets = new Models.ClassTicket[dtTicket.Rows.Count];
                    for (int i = 0; i < dtTicket.Rows.Count; i++)
                    {
                        response.data.tickets[i] = new Models.ClassTicket();
                        response.data.tickets[i].id_ticket = int.Parse(_utility.NullToString(dtTicket.Rows[i]["id_ticket"]));
                        response.data.tickets[i].title = _utility.NullToString(dtTicket.Rows[i]["title"]);
                        response.data.tickets[i].description = _utility.NullToString(dtTicket.Rows[i]["description"]);
                        response.data.tickets[i].picture_url = url + "picture_uploads/medium/" + _utility.NullToString(dtTicket.Rows[i]["picture"]);
                        response.data.tickets[i].created_by = _utility.NullToString(dtTicket.Rows[i]["created_by"]);
                        if (_utility.NullToString(dtTicket.Rows[i]["create_time"]) != "")
                            response.data.tickets[i].create_time = DateTime.Parse(_utility.NullToString(dtTicket.Rows[i]["create_time"]));
                        if (_utility.NullToString(dtTicket.Rows[i]["update_time"]) != "")
                            response.data.tickets[i].update_time = DateTime.Parse(_utility.NullToString(dtTicket.Rows[i]["update_time"]));

                        response.data.tickets[i].user = new Models.ClassUser();
                        response.data.tickets[i].user.id_user = _utility.NullToInt(dtTicket.Rows[i]["id_user"]);
                        response.data.tickets[i].user.user_unique_id = _utility.NullToString(dtTicket.Rows[i]["user_unique_id"]);
                        response.data.tickets[i].user.user_full_name = _utility.NullToString(dtTicket.Rows[i]["user_fullname"]);
                        response.data.tickets[i].user.user_email = _utility.NullToString(dtTicket.Rows[i]["user_email"]);

                        response.data.tickets[i].user.role = new Models.ClassRole();
                        response.data.tickets[i].user.role.id_role = _utility.NullToInt(dtTicket.Rows[i]["id_role"]);
                        response.data.tickets[i].user.role.role_name = _utility.NullToString(dtTicket.Rows[i]["role_name"]);
                        response.data.tickets[i].user.role.role_level = _utility.NullToInt(dtTicket.Rows[i]["role_level"]);
                    }
                }
                else
                {
                    Response.StatusCode = 400;
                    response.status = "fail";
                    response.message = "ticket not found, please check contact administrator";
                    _utility.FileTraceLog("ticket not found : " + "GetTicket, userUniqueId : " + jwtInformations[0]);

                }
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                response.status = "fail";
                response.message = e.Message;
                _utility.FileTraceLog("Exception : " + "GetTicket " + e.Message + "userUniqueId : " + jwtInformations[0]);
            }
            finally
            {
                sqlConn.Close();
            }

            return Json(response);
        }
    }
}

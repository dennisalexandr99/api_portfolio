using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiPortfolio
{
    public class ClassUtility
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constructor to inject IWebHostEnvironment
        public ClassUtility(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string NullToString(object field)
        {
            string result = "";
            if (field == DBNull.Value || field == null)
            {
                result = "";
            }
            else
            {
                result = field.ToString();
            }

            return result;
        }

        public int NullToInt(object field)
        {
            int result = 0;
            if (field == DBNull.Value || field == null)
                result = 0;
            else
                result = int.Parse(field.ToString());

            return result;
        }

        public string FileTraceLog(string msg)
        {
            string result = "";
            msg = msg.Replace("''.", "'.").Trim();

            if (msg.EndsWith(@"\"))
                msg = msg.Substring(0, msg.Length - 1);
            if (msg.Length > 4000)
                msg = msg.Substring(0, 4000);

            while (msg.EndsWith("'") & !msg.EndsWith("''"))
                msg = msg.Substring(0, msg.Length - 1);

            try
            {
                string dir = Path.Combine(_webHostEnvironment.ContentRootPath, "Tracelog");
                bool exists = Directory.Exists(dir);
                if (!exists)
                    Directory.CreateDirectory(dir);

                var sFile = Path.Combine(dir, "Tracelog" + DateTime.Now.ToString("yyMMddHH") + ".TXT");
                StreamWriter sw = new StreamWriter(String.Format(sFile), true);
                sw.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + ": " + msg);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                throw;
            }

            return result;
        }

        public string ComputeHmacSha256Base64(string input, string key)
        {
            // Convert the input string and key to byte arrays
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Create an HMACSHA256 object with the key
            using (var hmac = new HMACSHA256(keyBytes))
            {
                // Compute the HMAC hash of the input bytes
                byte[] hashBytes = hmac.ComputeHash(inputBytes);

                // Convert the hash to a Base64 encoded string
                string base64Result = Convert.ToBase64String(hashBytes);

                return base64Result;
            }
        }

        public bool IsValidUniqueId(string input)
        {
            string pattern = @"^\d{8}$";
            return Regex.IsMatch(input, pattern);
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

    }
}

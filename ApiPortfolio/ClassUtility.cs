using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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

        public bool UploadFile(IFormFile photo, string photo_name)
        {
            string pathReal = "";
            string pathMedium = "";

            try
            {
                pathReal = Path.Combine(Directory.GetCurrentDirectory(), "picture_uploads/real");
                pathMedium = Path.Combine(Directory.GetCurrentDirectory(), "picture_uploads/medium");

                // Generate file name and save the file
                //var fileName = Path.GetFileName(photo.FileName);
                var filePath = Path.Combine(pathReal, photo_name);
                var filePathMedium = Path.Combine(pathMedium, photo_name);

                if (!Directory.Exists(pathReal))
                    Directory.CreateDirectory(pathReal);

                if (!Directory.Exists(pathMedium))
                    Directory.CreateDirectory(pathMedium);

                //save photo real hanya sebagai temporary
                if (photo != null)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        photo.CopyTo(stream);
                    }
                }

                string resize_img_response = "";
                if (photo_name.Contains(".jpg"))
                {
                    resize_img_response = ResizeImages(pathReal + "/" + photo_name, pathMedium + "/" + photo_name, 30);
                    if (resize_img_response != "success")
                    {
                        using (var stream = new FileStream(filePathMedium, FileMode.Create))
                        {
                            photo.CopyTo(stream);
                        }
                    }
                }

                //delete photo real
                if (File.Exists(pathReal + "/" + photo_name))
                    File.Delete(pathReal + "/" + photo_name);

            }
            catch (Exception e)
            {
                throw;
            }

            return true;
        }

        public string ResizeImages(string photo_url_origin, string photo_url_commpress, int percent)
        {
            string response = "success";
            try
            {
                const int OrientationKey = 0x0112;
                const int NotSpecified = 0;
                const int NormalOrientation = 1;
                const int MirrorHorizontal = 2;
                const int UpsideDown = 3;
                const int MirrorVertical = 4;
                const int MirrorHorizontalAndRotateRight = 5;
                const int RotateLeft = 6;
                const int MirorHorizontalAndRotateLeft = 7;
                const int RotateRight = 8;

                int oldW, oldH;
                System.Drawing.Image curImg = System.Drawing.Image.FromFile(photo_url_origin);
                string imgFileName = Path.GetFileName(photo_url_origin);
                System.Drawing.Imaging.ImageCodecInfo[] Info = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
                System.Drawing.Imaging.EncoderParameters Params = new System.Drawing.Imaging.EncoderParameters(1);

                Params.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, percent);
                oldW = curImg.Width;
                oldH = curImg.Height;

                Bitmap imgCrpd = new Bitmap(oldW, oldH);
                Graphics myGraphic = Graphics.FromImage(imgCrpd);
                myGraphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                myGraphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                myGraphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                myGraphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                myGraphic.DrawImage(curImg, 0, 0, oldW, oldH);

                // Fix orientation if needed.
                if (curImg.PropertyIdList.Contains(OrientationKey))
                {
                    var orientation = (int)curImg.GetPropertyItem(OrientationKey).Value[0];
                    switch (orientation)
                    {
                        case NotSpecified: // Assume it is good.
                        case NormalOrientation:
                            // No rotation required.
                            break;
                        case MirrorHorizontal:
                            imgCrpd.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case UpsideDown:
                            imgCrpd.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case MirrorVertical:
                            imgCrpd.RotateFlip(RotateFlipType.Rotate180FlipX);
                            break;
                        case MirrorHorizontalAndRotateRight:
                            imgCrpd.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case RotateLeft:
                            imgCrpd.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case MirorHorizontalAndRotateLeft:
                            imgCrpd.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case RotateRight:
                            imgCrpd.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        default:
                            throw new NotImplementedException("An orientation of " + orientation + " isn't implemented.");
                    }
                }

                curImg.Dispose();

                imgCrpd.Save(photo_url_commpress, Info[1], Params);
                imgCrpd.Dispose();
            }
            catch (Exception ex)
            {
                FileTraceLog("exception error: " + ex.Message + " " + ex.StackTrace + " " + "RESIZE IMAGE FUNCTION");
                response = ex.ToString();
                throw;
            }
            return response;
        }

    }
}

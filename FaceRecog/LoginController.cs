using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;
using ShopifyMultipassTokenGenerator.Models;
using System.Configuration;
using Newtonsoft.Json;
using ShopifyMultipassTokenGenerator;
using FaceRecog.Controller;

namespace FaceRecog
{
    public class LoginController : BaseController
    {
        public static string RemoveEXIFRotation(string URI)
        {
            // Decode the URI information
            string base64Data = Regex.Match(URI, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            byte[] binData = Convert.FromBase64String(base64Data);

            using (var stream = new MemoryStream(binData))
            {
                Bitmap bmp = new Bitmap(stream);

                if (Array.IndexOf(bmp.PropertyIdList, 274) > -1)
                {
                    var orientation = (int)bmp.GetPropertyItem(274).Value[0];
                    switch (orientation)
                    {
                        case 1:
                            // No rotation required.
                            return URI;
                        case 2:
                            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case 3:
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 4:
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                            break;
                        case 5:
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipX);
                            break;
                        case 6:
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 7:
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
                            break;
                        case 8:
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                    // This EXIF data is now invalid and should be removed.
                    bmp.RemovePropertyItem(274);
                }

                ImageCodecInfo jgpEncoder = ImageCodecInfo.GetImageDecoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);

                // Create an Encoder object based on the GUID
                // for the Quality parameter category.
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                // Create an EncoderParameters object.
                // An EncoderParameters object has an array of EncoderParameter
                // objects. In this case, there is only one
                // EncoderParameter object in the array.
                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;

                using (MemoryStream output = new MemoryStream(binData.Length))
                {
                    bmp.Save(output, jgpEncoder, myEncoderParameters);
                    output.Position = 0;
                    int len = (int)output.Length;
                    byte[] results = new byte[len];
                    output.Read(results, 0, len);

                    return string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(results));
                }
            }
        }

        [HttpPost]
        public ResponseMessage Post()
        {
            float thSimilarity = 0.8f;

            if (!GlobalVar.bInitialized)
            {
                string curPath = HttpContext.Current.Server.MapPath("..") + "\\fr.dat";
                if (FaceEngine.InitialEngine((float)15.0, (float)100000.0, curPath))
                    GlobalVar.bInitialized = true;
            }

            //user verification(name & face verification)

            //name verification
            String userName = HttpContext.Current.Request.Params["UserName"];
            string strReturnUrl = HttpContext.Current.Request.Params["ReturnUrl"];
            string strFrom = HttpContext.Current.Request.Params["From"];

            string sqlQuery = "SELECT COUNT(*) FROM UserData WHERE UserName = '" + userName + "'";
            SqlConnection dbConn = new SqlConnection(GlobalVar.sqlDatabaseAccessString);
            SqlCommand sqlCmd = new SqlCommand(sqlQuery, dbConn);
            
            try
            {
                dbConn.Open();

                Int32 count = Convert.ToInt32(sqlCmd.ExecuteScalar());
                if(count == 0)
                {
                    return makeRedirectResponse(new ResponseMessage(false, "There's no user having this name!"), strReturnUrl, strFrom);
                }
            }
            catch (Exception)
            {
                return makeRedirectResponse(new ResponseMessage(false, "DB error occured while verification is being held!"), strReturnUrl, strFrom);
            }
            finally
            {
                dbConn.Close();
            }

            //face verfication
            String userPhoto = HttpContext.Current.Request.Params["UserPhoto"];

            userPhoto = RemoveEXIFRotation(userPhoto);
            userPhoto = userPhoto.Replace("data:image/jpg;base64,", "");
            byte[] img = Convert.FromBase64String(userPhoto);

            MemoryStream memStream = new MemoryStream();
            memStream.Write(img, 0, img.Length);
            memStream.Flush();
            memStream.Seek(0, SeekOrigin.Begin);
            BitmapEx bmp = new BitmapEx(memStream);

            if (bmp.GetWidth() == 0)
            {
                return makeRedirectResponse(new ResponseMessage(false, "Invalid Photo Error!"), strReturnUrl, strFrom);
            }

            // detect face
            Int32 nFaceCount = FaceEngine.GetFaceCount(bmp.GetBuffer(), bmp.GetWidth(), bmp.GetHeight());
            if (nFaceCount <= 0)
            {
                FaceEngine.RemoveAllTemplate();
                return makeRedirectResponse(new ResponseMessage(false, "Face Detection Error! Could not find any face."), strReturnUrl, strFrom);
            }
            else if (nFaceCount > 1)
            {
                FaceEngine.RemoveAllTemplate();
                return makeRedirectResponse(new ResponseMessage(false, "Too many faces! Please make sure that there's only 1 face."), strReturnUrl, strFrom);
            }

            //get face region
            FaceRectInfo faceRect = new FaceRectInfo();
            FaceEngine.GetDetectFaces(0, ref faceRect);

            //detect landmarks
            SFaceLandmark landmarkPoints = new SFaceLandmark();
            FaceEngine.GetDetectPoints(0, ref landmarkPoints);

            FaceEngine.RemoveAllTemplate();

            //extract facial feature
            float[] pFeature = new float[2048];
            FaceEngine.GetFeatures(bmp.GetBuffer(), bmp.GetWidth(), bmp.GetHeight(), 3, ref landmarkPoints, pFeature);

            //comparing with enrolled faces
            sqlCmd.Dispose();
            dbConn.Dispose();

            bool bFaceVerified = false;
            string strUserEmail = "";

            sqlQuery = "SELECT UserEmail, FeatureData FROM UserData WHERE UserName = '" + userName + "'";
            dbConn = new SqlConnection(GlobalVar.sqlDatabaseAccessString);
            sqlCmd = new SqlCommand(sqlQuery, dbConn);
            try
            {
                dbConn.Open();
                SqlDataReader dbRead = sqlCmd.ExecuteReader();

                while (dbRead.Read())
                {
                    strUserEmail = dbRead.GetString(0);
                    string strJointEnrolledFeatureData = dbRead.GetString(1);

                    string[] strEnrolledFeatureDatas = strJointEnrolledFeatureData.Split('|');

                    float maxSimilarity = 0.0f;
                    for (int i = 0; i < strEnrolledFeatureDatas.Length; i++)
                    {
                        float[] pEnrolledSubFeature = Array.ConvertAll(strEnrolledFeatureDatas[i].Split(','), float.Parse);
                        float similarity = FaceEngine.GetSimilarity(pFeature, pEnrolledSubFeature);
                        if (maxSimilarity < similarity)
                            maxSimilarity = similarity;
                    }

                    if (maxSimilarity > thSimilarity)
                    {
                        bFaceVerified = true;
                        break;
                    }
                }
                dbRead.Close();
            }
            catch (Exception)
            {
                return makeRedirectResponse(new ResponseMessage(false, "Error occured while verification is being held!"), strReturnUrl, strFrom);
            }
            finally
            {
                dbConn.Close();
            }

            if (!bFaceVerified)
            {
                return makeRedirectResponse(new ResponseMessage(false, "Sorry, your face doesn't match with enrolled one!"), strReturnUrl, strFrom);
            }

            // Succeeded
            return makeShopifyLoginResponse(strUserEmail, strReturnUrl, strFrom);
        }
    }
}
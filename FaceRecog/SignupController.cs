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

namespace FaceRecog
{
    public class SignupController : ApiController
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

            //photo processing
            String userMainPhoto = HttpContext.Current.Request.Params["MainPhoto"];
            userMainPhoto = RemoveEXIFRotation(userMainPhoto);
            userMainPhoto = userMainPhoto.Replace("data:image/jpg;base64,", "");

            byte[] bufferMainPhoto = Convert.FromBase64String(userMainPhoto);
            MemoryStream memStream = new MemoryStream();
            memStream.Write(bufferMainPhoto, 0, bufferMainPhoto.Length);
            memStream.Flush();
            memStream.Seek(0, SeekOrigin.Begin);
            
            BitmapEx bmpMainPhoto = new BitmapEx(memStream);
            if (bmpMainPhoto.GetWidth() == 0)
            {
                return new ResponseMessage(false, "Invalid Photo Error!");
            }

            // detect face
            Int32 nFaceCount = FaceEngine.GetFaceCount(bmpMainPhoto.GetBuffer(), bmpMainPhoto.GetWidth(), bmpMainPhoto.GetHeight());
            if (nFaceCount <= 0)
            {
                FaceEngine.RemoveAllTemplate();
                return new ResponseMessage(false, "Face Detection Error! Could not find any face.");
            }
            else if (nFaceCount > 1) 
            {
                FaceEngine.RemoveAllTemplate();
                return new ResponseMessage(false, "Too many faces! Please make sure that there's only 1 face.");
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
            FaceEngine.GetFeatures(bmpMainPhoto.GetBuffer(), bmpMainPhoto.GetWidth(), bmpMainPhoto.GetHeight(), 3, ref landmarkPoints, pFeature);

            //comparing with enrolled faces
            bool bFaceExisting = false;

            string sql = "SELECT FeatureData FROM UserData";
            SqlConnection dbConn = new SqlConnection(GlobalVar.sqlDatabaseAccessString);
            SqlCommand dbComm = new SqlCommand(sql, dbConn);        
            try
            {
                dbConn.Open();
                SqlDataReader dbRead = dbComm.ExecuteReader();
                
                while(dbRead.Read())
                {
                    string strJointEnrolledFeatureData = dbRead.GetString(0);
                    string[] strEnrolledFeatureDatas = strJointEnrolledFeatureData.Split('|');

                    //compare with main feature first
                    float[] pEnrolledMainFeature = Array.ConvertAll(strEnrolledFeatureDatas[0].Split(','), float.Parse);
                    float similarity = FaceEngine.GetSimilarity(pFeature, pEnrolledMainFeature);
                    if (similarity < thSimilarity)
                    {
                        //compare with all features
                        float overallSimilarity = similarity;
                        for (int i = 0; i < strEnrolledFeatureDatas.Length - 1; i++)
                        {
                            float[] pEnrolledSubFeature = Array.ConvertAll(strEnrolledFeatureDatas[i + 1].Split(','), float.Parse);
                            overallSimilarity += FaceEngine.GetSimilarity(pFeature, pEnrolledSubFeature);
                        }
                        overallSimilarity /= strEnrolledFeatureDatas.Length;

                        if (overallSimilarity > thSimilarity)
                        {
                            bFaceExisting = true;
                            break;
                        }
                    }
                    else
                    {
                        bFaceExisting = true;
                        break;
                    }
                }
                dbRead.Close();
            }
            catch (Exception)
            {
                return new ResponseMessage(false, "DB error occured while comparing with enrolled faces!");
            }
            finally
            {
                dbConn.Close();
            }

            if(bFaceExisting)
            {
                return new ResponseMessage(false, "There's already the same face existing!");
            }

            //signing up
            String userName = HttpContext.Current.Request.Params["UserName"];
            String userAddress = HttpContext.Current.Request.Params["UserAddress"];
            String strFeatureData = string.Join(",", pFeature);

            byte[] bufferUserBlob = null;
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                for(int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                {
                    long length = HttpContext.Current.Request.Files[i].InputStream.Length;
                    bufferUserBlob = new byte[length];
                    HttpContext.Current.Request.Files[i].InputStream.Read(bufferUserBlob, 0, (int)length);

                    if (HttpContext.Current.Request.Files[i].ContentType == "image/jpeg")
                    {
                        //calc features from subPhotos & concatenate them
                        BitmapEx bmpSubPhoto = new BitmapEx(HttpContext.Current.Request.Files[i].InputStream);

                        if (bmpSubPhoto.GetWidth() == 0)
                        {
                            continue;
                        }

                        nFaceCount = FaceEngine.GetFaceCount(bmpSubPhoto.GetBuffer(), bmpSubPhoto.GetWidth(), bmpSubPhoto.GetHeight());
                        if (nFaceCount == 1)
                        {
                            FaceEngine.GetDetectFaces(0, ref faceRect);
                            FaceEngine.GetDetectPoints(0, ref landmarkPoints);
                            FaceEngine.GetFeatures(bmpSubPhoto.GetBuffer(), bmpSubPhoto.GetWidth(), bmpSubPhoto.GetHeight(), 3, ref landmarkPoints, pFeature);
                            String strSubFeatureData = string.Join(",", pFeature);
                            strFeatureData = strFeatureData + "|" + strSubFeatureData;
                        }
                        FaceEngine.RemoveAllTemplate();
                    }
                }
            }

            string strQuery = "INSERT INTO UserData (UserName, UserAddress, UserPhoto, UserVideo, FeatureData) ";
            strQuery += "VALUES (@UserName, @UserAddress, @UserPhoto, @UserVideo, @FeatureData)";
            SqlCommand cmd = new SqlCommand(strQuery);
            cmd.Parameters.Add("@UserName", SqlDbType.NChar).Value = userName;
            cmd.Parameters.Add("@UserAddress", SqlDbType.NChar).Value = userAddress;
            cmd.Parameters.Add("@UserPhoto", SqlDbType.Image).Value = bmpMainPhoto.GetBuffer();
            cmd.Parameters.Add("@UserVideo", SqlDbType.Image).Value = bufferUserBlob;
            cmd.Parameters.Add("@FeatureData", SqlDbType.Text).Value = strFeatureData;

            SqlConnection sqlDbConnection = new SqlConnection(GlobalVar.sqlDatabaseAccessString);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlDbConnection;
            try
            {
                sqlDbConnection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return new ResponseMessage(false, "DB error occured while creating an instance!");
            }
            finally
            {
                sqlDbConnection.Close();
                sqlDbConnection.Dispose();
            }

            return new ResponseMessage(true, "OK");
        }
    }
}
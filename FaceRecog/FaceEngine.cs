using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace FaceRecog
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FaceRectInfo
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public float roll;
        public float pitch;
        public float yaw;
        public float score; 
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LandmarkPoint
    {
        public int X;
        public int Y;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SFaceLandmark
    {
        public LandmarkPoint RightEye;
        public LandmarkPoint LeftEye;
        public LandmarkPoint Nose;
        public LandmarkPoint LeftMouth;
        public LandmarkPoint RightMouth;
    }

    class FaceEngine
    {
        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitialEngine(float fmin, float fmax, [MarshalAs(UnmanagedType.LPStr)]string modelPath);


        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 GetFaceCount(byte[] imgbuf, Int32 dwWidth, Int32 dwHeight);


        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetDetectFaces(Int32 nFaceIndex, ref FaceRectInfo pFaceData);


        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetDetectPoints(Int32 nFaceIndex, ref SFaceLandmark pLandmarkPoints);

                
        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetFeatures(byte[] imgbuf, Int32 imgWidth, Int32 imgHeight, Int32 nChannels, ref SFaceLandmark pLandmarkPoints, float[] pFeature);


        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetSimilarity(float[] pFeature1, float[] pFeature2);


        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RemoveAllTemplate();


        [DllImport(@"faceengine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool FinalEngine();
    }
}
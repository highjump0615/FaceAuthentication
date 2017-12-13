using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace FaceRecog
{
    public class BitmapEx
    {
        int width = 0;
        int height = 0;
        int stride = 0;
        byte[] buffer;
        Bitmap bitmap;

        public Bitmap GetBitmap()
        {
            return bitmap;
        }
        public BitmapEx(Bitmap bmp)
        {
            LoadBitmap(bmp);
        }
        public BitmapEx(Stream stream)
        {
            LoadBitmap(stream);
        }

        public BitmapEx(String filename)
        {
            LoadBitmap(filename);
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public int GetStride()
        {
            return stride;
        }

        public byte[] GetBuffer()
        {
            return buffer;
        }

        bool LoadBitmap(Bitmap bmp)
        {
            try
            {
                bitmap = bmp;
                width = bitmap.Width;
                height = bitmap.Height;
                if (width == 0 || height == 0)
                    return false;

                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                stride = bitmapData.Stride;
                buffer = new byte[stride * height];
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, stride * height);
                bitmap.UnlockBits(bitmapData);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        bool LoadBitmap(String filename)
        {
            try
            {
                bitmap = new Bitmap(filename);
                width = bitmap.Width;
                height = bitmap.Height;
                if (width == 0 || height == 0)
                    return false;

                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                stride = bitmapData.Stride;
                buffer = new byte[stride * height];
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, stride * height);
                bitmap.UnlockBits(bitmapData);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        bool LoadBitmap(Stream stream)
        {
            if (stream.Length == 0)
                return false;

            try
            {
                bitmap = new Bitmap(stream);
                width = bitmap.Width;
                height = bitmap.Height;
                if (width == 0 || height == 0)
                    return false;
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                stride = bitmapData.Stride;
                buffer = new byte[stride * height];
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, stride * height);
                bitmap.UnlockBits(bitmapData);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public void SaveImage(String filename)
        {
            bitmap.Save(filename);
        }
    }
}
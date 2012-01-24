using System;
using System.Drawing;
using System.IO;

namespace MainFrame.Devices.Video
{


	public class CvBitmap : ICaptureImage
	{
		
		public static int BytesPerPixel = 4;
		
		public Bitmap Bitmap {get; private set;}
		public int Width {get; private set;}
		public int Height {get; private set;}
		
		public unsafe CvBitmap (byte *dataPtr, int width, int height) 
		{
			Width = width;
			Height = height;
			
			if ((int)dataPtr != 0) {
				//might not be the correct way: (channels etc)
			
				Bitmap = new Bitmap(
					
				    width,
				                       height,
				                       GetLineWidth(),
				                       System.Drawing.Imaging.PixelFormat.Format24bppRgb,
				                       new IntPtr(dataPtr));
				
					
				
				//int bmpSize = _ext_getScreenHeight() * _ext_getScreenWidth() * 3;
				//byte [] rawBmp = new byte[bmpSize];
				
				//Marshal.Copy(new IntPtr(dataPtr), rawBmp, 0, bmpSize);
				//using (MemoryStream stream = new MemoryStream(rawBmp)) 
				//{
				//}
			}
			else
				throw new NullReferenceException("Pointer was null");

		}
		
		private int GetLineWidth() 
		{
			return BytesPerPixel * Width;
		}
		
		public void Save(string fileName)  {
			Bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			Bitmap = null;
		}
		
		#endregion
	}
}

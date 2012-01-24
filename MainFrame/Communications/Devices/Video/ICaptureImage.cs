using System.Drawing;
using System;

namespace MainFrame.Devices.Video
{


	public interface ICaptureImage : IDisposable
	{
		Bitmap Bitmap {get;}
		int Width {get; }
		int Height {get;}
		void Save(string fileName);
		
	}
}

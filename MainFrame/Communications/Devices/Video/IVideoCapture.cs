using MainFrame.Devices;
using System;

namespace MainFrame.Devices.Video
{

	public interface IVideoCapture : IDevice
	{
		bool Initialized {get;}
		bool Capture ();
		CvRect Area {get;}
		bool HasCapture {get;}
		ICaptureImage Image{get ;}
		CvSize ScreenSize{get;}
	}
}

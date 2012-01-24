using MainFrame.Processes;
using MainFrame.Communication;
using System;

namespace MainFrame.Devices.Video
{


	public interface ICamera : IProcess, IRequestable
	{
		IVideoCapture CreateHaarCapture(string haarFileName);
		bool TryCaptureFor(int position);
		CvRect GetCaptureAreaFor(int position);
		ICaptureImage GetCaptureImage(int position);
		CvSize ScreenSize {get;}
		ICaptureImage CurrentFrame {get;}
		
	}
}

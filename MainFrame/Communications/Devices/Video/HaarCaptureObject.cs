using MainFrame.Core.Logger;
using System;

namespace MainFrame.Devices.Video
{


	public class HaarCaptureObject : IVideoCapture
	{
		
		protected CameraProcess camera;
		protected string haarFileName;
		protected int position;

		public bool Initialized {get; protected set;}
		
		public HaarCaptureObject (CameraProcess camera, string haarFileName, int position)
		{
			this.camera = camera;
			this.haarFileName = haarFileName;
			this.position = position;
			
		}
		
		public CvRect Area {get; private set;}
		public CvSize ScreenSize
		{
			get { return camera.ScreenSize; }
		}
		public bool HasCapture {get; private set;}
		
		public bool Capture () 
		{
			if (camera.TryCaptureFor(position))
			{
				Area = camera.GetCaptureAreaFor(position);
				HasCapture = true;
				return true;
			}
			return false;
		}
		
		public ICaptureImage Image 
		{ 
			get
			{
				if (HasCapture)
					return camera.GetCaptureImage(this.position);
				
				return null;
			}
		}
		
		#region IDevice implementation
		public void Start ()
		{
			Initialized = true;
		}
		
		
		public void Stop ()
		{
			Initialized = false;
			//throw new System.NotImplementedException("dealloc in Cameracotroller>video.so ?");
		}
		
		
		public bool Ready {
			get {
				return Initialized && camera.IsRunning;
			}
		}
		
		#endregion		
		
	}
}

using System.Runtime.InteropServices;
using System.Threading;
using System;
using MainFrame.Devices;
using MainFrame.Processes;
using System.Drawing;
using MainFrame.Core.Logger;
using MainFrame.Communication;

namespace MainFrame.Devices.Video
{
	
	public class CameraProcess : ICamera
	{
		
		private const string dllPath = "video.so";
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]		
		protected static extern int _ext_didCapture(int position);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]		
		protected static extern int _ext_isCapturing(int position);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern CvRect _ext_getLatestCapture(int position);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_getScreenWidth();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_getScreenHeight();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_init(int cameraNO );
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_start();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_stop ();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_captureObjectPositionTaken(int position);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_createCaptureObject(int position, string cascadeFileName);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_setCaptureObjectBounds(int position, CvRect bounds);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_releaseCaptureObjectBounds(int position);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern void _ext_capture(int position);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
		unsafe protected static extern byte* _ext_getCurrentFrame();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		unsafe protected static extern byte* _ext_getImageData(int position);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
		protected static extern int _ext_getMaxCaptureObjects();
				
		protected bool isReady;
		protected int cameraDevice;
		
		protected IVideoCapture[] captureObjects;
		
		protected INervousSystem mediator;
		
		public CameraProcess ( int cameraNo)
		{
			cameraDevice = cameraNo;
			
			if (_ext_init(cameraNo) != 0)
				throw new DeviceException("Unable to initiate video.");
			
			captureObjects = new IVideoCapture[_ext_getMaxCaptureObjects()];
			
			//just in case. might crash it starts before native init is done. woho
			Thread.Sleep(100);
			
			
		}
		
		~CameraProcess () 
		{
			_ext_stop();
		}

		public CameraProcess () : this (0)
		{
			
		}

		public IVideoCapture CreateHaarCapture(string haarFileName) 
		{
			
			for (int i = 0; i < captureObjects.Length; i++) 
			{
				if (captureObjects[i] == null || 
				    !captureObjects[i].Initialized)
				{
					
					int createCode = _ext_createCaptureObject(i,haarFileName);
					if ( createCode != 0)
						throw new ApplicationException("Unable to create capture object, code: " + createCode);
					
					captureObjects[i] = new HaarCaptureObject(this, haarFileName, i);
					
					captureObjects[i].Start();
					return captureObjects[i];
				}
			}
			
			Log.w("WARNING: Unable to create IVideoCapture: all slots taken.");
			
			return null;
		}
		
		public bool TryCaptureFor(int position) 
		{
			_ext_capture(position);
			while (_ext_isCapturing(position) == 1)
				Thread.Sleep(20);
			
			return _ext_didCapture(position) == 1;
		}
		
		public CvRect GetCaptureAreaFor(int position) 
		{
			return _ext_getLatestCapture(position);
		}
		
		
		public bool Ready { get {return IsRunning;} }
		
		public void Start ()
		{
			
			try
			{
				
				Log.d("Starting video capture on " + cameraDevice.ToString());
				
				
				//Thread frameCaptureThread = new Thread(captureFrame);
				//frameCaptureThread.IsBackground = true;
				isReady = true;
				while (isReady) 
				{
					int result = _ext_start();
					if (result != 0)
						throw new ProcessException("Unable to start video capture");
				}
			
				
				//videoThread.Start();
				//frameCaptureThread.Start();
			}
			catch (Exception ex)
			{
				Log.x(ex);
			}
			finally 
			{
				isReady = false;
			}			
		}
		
		unsafe protected void captureFrame() {
			while (isReady) {
				Thread.Sleep(1000);
				
				if ((int) _ext_getCurrentFrame() != 0) 
				{
					using (CvBitmap bm = new CvBitmap( _ext_getCurrentFrame(), _ext_getScreenWidth(), _ext_getScreenHeight())) {
						bm.Save("tmp.png");
					}
				}
			}
				
		}
		
		unsafe public ICaptureImage CurrentFrame 
		{
			get 
			{
				return new CvBitmap(_ext_getCurrentFrame(), _ext_getScreenWidth(), _ext_getScreenHeight());
			}
		}
		
		unsafe public ICaptureImage GetCaptureImage(int position) 
		{
			if((int)_ext_getImageData(position) != 0)
			{
				CvRect frame = GetCaptureAreaFor(position);
				return new CvBitmap(_ext_getImageData(position), frame.width, frame.height);
			}
			
			return null;
		}
		
		public void Stop ()
		{
			Log.d("Stopping video capture on " + cameraDevice.ToString());
			isReady = false;
		}
		
		
		public bool IsRunning {
			get {
				return isReady;
			}
		}
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get { 
				yield return typeof(RunSignal);
				yield return typeof(GetSignal<ICamera>);
			}
		}
		
		public T Request<T>(INervousSignal<T> message) 
		{

			if (message != null) 
			{
			
			   	if ( message is RunSignal) 
				{
					if(!((RunSignal)message).Data)
			    		Stop();
			
					return (T)(object)isReady;
				}
				else if (message is GetSignal<ICamera>) 
				{
					return (T) (object) this;
				}
			}
			
			return default(T);
		}
		
		public void SetMediator (INervousSystem mediator) 
		{
			this.mediator = mediator;
		}
		
		public CvSize ScreenSize
		{
			get {return new CvSize() {width = _ext_getScreenWidth(), height = _ext_getScreenHeight()};}
		}
		
	}
}

using System.Runtime.InteropServices;
using System.Threading;
using System;
using MainFrame.Devices;

namespace MainFrame
{


	public class FaceDetectorHolder 
	{
		public static FaceDetector Instance;
	}
	
	public class FaceDetector : IDevice
	{

		protected Thread faceDetectorThread;
		
		public bool IsRunning {get; private set;}
		
		protected delegate void FaceDetectedCallBack(int x, int y, int width, int height);
		
		//protected delegate void FaceDetectedCallBack2(string msg);
		
		private const string dllPath = "facedetect.so";
		
		[DllImport(dllPath)]   	 	
		protected static extern int getMaxWidth();		
		
		[DllImport(dllPath)]   	 	
		protected static extern int getMaxHeight();
		
		[DllImport(dllPath)]
   	 	protected static extern int init(
			FaceDetectedCallBack pointFunc, 
			string faceHaarFile, 
			int captureDevice);

		[DllImport(dllPath)]
   	 	protected static extern void start();

		[DllImport(dllPath)]
   	 	protected static extern void stop();

		public FaceDetector ()
		{
			
			FaceDetectorHolder.Instance = this;
			//Console.WriteLine("men guud så fint: " + FaceDetector.getMaxWidth());
			if (
			    
			    FaceDetector.init(
			         new FaceDetectedCallBack(FaceDetector.FaceDetected), 
			         "haarcascade_frontalface_alt.xml",
			         0) != 0)
			    throw new ApplicationException("Unable to initialize face detection");
			
		}
		
		public void Start()
		{
			if (IsRunning)
				throw new ApplicationException("Face detector is allready running...");
			
			
			IsRunning = true;
			
			faceDetectorThread = new Thread(RunFaceDetection);
			faceDetectorThread.Name = "faceDetectorThread";
			faceDetectorThread.IsBackground = true;
			faceDetectorThread.Start();

			
			
		}
		
		public void RunFaceDetection() 
		{
			Console.WriteLine("face start...");
			FaceDetector.start();
			Console.WriteLine("...face end");
		}

		public void Stop () 
		{
			FaceDetector.stop();
			IsRunning = false;
		}
		
		public static void FaceDetected(int x, int y, int width, int height) 
		{
			
			Console.WriteLine("FACE on: " + x + ", " + y + " width: " + width + " height: " + height + " NAME: " + Thread.CurrentThread.Name);
		}
		
		protected void FaceDetected2(string msg) 
		{
			
			Console.WriteLine("scheisse: " + msg);
		}

		
		
		public bool Ready {
			get {
				throw new System.NotImplementedException();
			}
		}
		
	}
}

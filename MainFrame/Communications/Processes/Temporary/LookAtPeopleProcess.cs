using MainFrame.Devices.Video;
using System;
using System.Threading;
using MainFrame.Core.Logger;
using MainFrame.Devices;
using MainFrame.Communication;

namespace MainFrame.Processes
{


	public class LookAtPeopleProcess  : IProcess, IRequestable
	{
		#pragma warning disable 0414	
		IVideoCapture capture;
		IHead head;
		public System.Drawing.Bitmap Image {get; private set;}
		
		bool isRunning;
		
		public LookAtPeopleProcess ( IVideoCapture capture, IHead head )
		{
			this.capture = capture;
			this.head = head;
			
			head.Rotation = 90;
			while (!head.Ready)
				Thread.Sleep(50);
			head.Up = 90;
			
		}
		
		public bool Ready { get {return IsRunning;} }
		
		#region IProcess implementation
		public void Start ()
		{
			try
			{
				isRunning = true;
				
				double pixelsPerDegree = 10.0;
				double xPerDegree = pixelsPerDegree; //capture.ScreenSize.width / 180.0;
				double yPerDegree = pixelsPerDegree /
					(double)((double)capture.ScreenSize.height / (double)capture.ScreenSize.width);
					 //capture.ScreenSize.height / 180.0;
				double fullRotationX = 22.5;
				double fullRotationY = 11.25;
				
				
				while (isRunning) 
				{
					if (!capture.Ready) 
						Log.w("Capture not ready!");
					
					
					
					if (capture.Capture()) 
					{
						//Image = new System.Drawing.Bitmap( capture.Image.Bitmap);
						
						double x = capture.Area.x + capture.Area.width / 2;
						double y = capture.Area.y + capture.Area.height / 2;
						
						double dx = x - capture.ScreenSize.width / 2;
						double dy = y - capture.ScreenSize.height / 2;
						
						double dRotateX = - dx / xPerDegree;
						double dRotateY = - dy / yPerDegree;
						double rotateX = head.Rotation + dRotateX; //fullRotationX  * (dx / (capture.ScreenSize.width / 2));
						double rotateY = head.Up + dRotateY;//fullRotationY  * (dy / (capture.ScreenSize.height / 2));
							//dy / yPerDegree;
						
						if (rotateX < 30) 
							rotateX = 30;
						if (rotateX > 150)
							rotateX = 150;
						if (rotateY < 0) 
							rotateY = 0;
						if (rotateY > 180)
							rotateY = 180;
						Log.w("Capture area: " + capture.Area.x + "," + capture.Area.y + ","+ capture.Area.width + ","+ capture.Area.height + " (" +  capture.ScreenSize.width + ","  + capture.ScreenSize.height + " )" );
						Log.w("Trying to rotate to.. " + rotateX + ", " + rotateY + " dx: " + (int) dx + " dy: " + (int)dy + " dRx: " + dRotateX + " drY: " + dRotateY);
						
						while (!head.Ready)
							Thread.Sleep(50);
						head.Rotation = rotateX;
						ICaptureImage frame = mediator.Request<ICamera>(new GetSignal<ICamera>()).CurrentFrame;
						
						using (System.Drawing.Bitmap bm = frame.Bitmap) 
						{
							//bm.Save("img_" + capture.Area.x + "_" + capture.Area.y + "____dx" + capture.Area.width + "_dy" + capture.Area.height +  ".png");
						}
						
						while (!head.Ready)
							Thread.Sleep(50);
						head.Up = rotateY;
						
						
					}
					/*
					else
						Console.WriteLine ("KUUUUUUUUUUUUUUUUUNDE IIIIIIIIIIIIIIINTE");
					*/
					//Thread.Sleep(100);
				}
			}
			catch (Exception ex)
			{
				Log.x(ex);
			}
			finally
			{
				isRunning = false;
				
			}
		}
		
		
		public void Stop ()
		{
			isRunning = false;
		}
		
		
		public bool IsRunning {
			get {
				return isRunning;
			}
		}
		
		#endregion		
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)
		
		{
			throw new System.NotImplementedException();
		}
		
		#endregion
		private INervousSystem mediator;
		#region INode implementation
		public void SetMediator (INervousSystem mediator)
		{
			this.mediator = mediator;
		}
		
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield break;
			}
		}
		
		#endregion
	}
}

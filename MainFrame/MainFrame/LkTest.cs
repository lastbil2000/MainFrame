using System;
using System.Runtime.InteropServices;
using System.Threading;
using MainFrame.Devices;
using MainFrame.Communication; 

namespace MainFrame
{


	public class LkTest
	{

		protected delegate void PointsReceivedCallBack(double x, double y);
		protected delegate void StartPointingCallBack(double x, double y);
		
		private const string dllPath = "lk.so";
		
		[DllImport(dllPath /*, CharSet = CharSet.Auto*/)]   	 	
		protected static extern int getMaxWidth();		
		
		[DllImport(dllPath /*, CharSet = CharSet.Auto*/)]   	 	
		protected static extern int getMaxHeight();		
		
		[DllImport(dllPath)]
   	 	protected static extern int start(PointsReceivedCallBack pointFunc,
		                                  StartPointingCallBack startFunc);

		[DllImport(dllPath)]
   	 	protected static extern void stopLk();

		Thread tr;
		INervousSystem mediator;
		
		public LkTest (INervousSystem mediator)
		{
			this.mediator = mediator;
			tr = new Thread(Go);
			tr.Start();
			Console.WriteLine("TOTALT: " + getMaxWidth());
		}
		
		public void Go() {
			
			if (start(PointReceived, StartPointingReceived) != 0)
				throw new ApplicationException("vafan!%");
			else
				Console.WriteLine("det gick ju bra");
		}
		
		public void Die() {
			stopLk();
		}
		
		private double orgX, orgY;
		
		public void StartPointingReceived (double x, double y) 
		{
			Console.WriteLine ("STARTADE : " + x + " och " + y + " maxX: " + getMaxHeight() + " maxY: " + getMaxWidth());
			orgX = x;
			orgY = y;			
		}
		
		Arm3Engines.ArmPosition state;
		private bool waitForSend;
		public void PointReceived (double x, double y) 
		{
			state = mediator.Request<Arm3Engines.ArmPosition>(new Arm3Engines.ArmSignal());
			
			if (state != null)
			{
				double dx =  90 + (orgX - x)/2;
				double dy =  90 - (orgY - y)/2;
				
				if(dx < 1)
					dx = 1;
				if (dx > 179)
					dx = 179;

				if(dy < 30)
					dy = 30;
				if (dy > 179)
					dy = 179;

				
				state.Rotate = dx;
				state.Up = dy;
				state.Open = state.Open;
				
				if (state.Ready)
					new Thread(SendToArm).Start();
				
				else if (!waitForSend)
					new Thread(WaitAndSend).Start();

				
				Console.WriteLine("dx : " + dx + " dy: " + dy  + " maxX: " + getMaxHeight() + " maxY: " + getMaxWidth());
				
			}
			//Console.WriteLine ("nu fick jag : " + x + " och " + y + " maxX: " + getMaxHeight() + " maxY: " + getMaxWidth());
		}
		
		private void WaitAndSend() 
		{
			waitForSend = true;
			Thread.Sleep(500);
			SendToArm();
			waitForSend = false;
		}
		
		private void SendToArm() 
		{
			//state.Ready = false;
			Console.WriteLine("Sending up: " + state.Up + " sending rotate: " + state.Rotate);
			mediator.Request(new Arm3Engines.ArmSignal() { Data = state });
		}
	}
}

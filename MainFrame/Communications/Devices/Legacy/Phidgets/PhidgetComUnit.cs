using System;
using System.Diagnostics;
using System.Threading;

namespace MainFrame.Devices
{


	public sealed class PhidgetComUnit : IComUnit
	{

		private int delay = 20000;
		//private Queue<IComUnitPackage> packages;
		private bool currentPackageOk;
		private Stopwatch transferTimer;		//keep track of the transaction time
		private int transferCount;
		
		public IIOBoard Board {get; private set;}

		public bool Ready {get; private set; }
		public int DataPort {get; private set;}
		public int StatusPort {get; private set;}
		
		public PhidgetComUnit (IIOBoard board, int dataPort, int statusPort)
		{
			if (board == null)
				throw new NullReferenceException("IIOBoard cannot be null.");
			if (!board.Ready)
				throw new ApplicationException ("Unitialized board passed to the ComUnit.");
			
			if (dataPort < board.PortMin || dataPort > board.PortMax)
				throw new ApplicationException ("dataPort is out of range.");
			if (statusPort < board.PortMin || statusPort > board.PortMax)
				throw new ApplicationException ("statusPort is out of range.");
			
			Board = board;
			Board.ErrorOccured += PhidgetException;
			
			board.SetDigitalOutput(statusPort, false);
			board.SetDigitalOutput(dataPort, false);
			
			DataPort = dataPort;
			StatusPort = statusPort;
			
			Ready = true;

		}
		
		public void Start () {}
		public void Stop () {}
		
		private void Delay (double microSeconds)
		{
			
			Stopwatch sw = Stopwatch.StartNew(); 
			while (sw.ElapsedMicroseconds() < microSeconds);
			sw.Stop();
			
		}
		
		private void Transfer (uint data, int count)
		{
	
			for (int serialBit = 0; serialBit < count; serialBit++)
			{
				Board.SetDigitalOutput(DataPort, (data & 1) == 1);
				data = (data >> 1);
				Delay(delay);
			}						
		}
		
		private uint InvertBits (uint data, int count)
		{
			uint ret = 0;
			
			for (int serialBit = 0; serialBit < count; serialBit++)
			{
				ret += (uint)(  (data & 1) << 
					Convert.ToUInt16((count - 1) - serialBit));
				data = (data >> 1);
			}
			
			return ret;
		}
		
		private void PhidgetException (object sender, EventArgs e)
		{
			currentPackageOk = false;
			//currentPackage.Success = false;
		}
		
		private void SendPackage (IComUnitPackage package)
		{
			currentPackageOk = true;
			Transfer(0x1,1);	//say hello
			Transfer((uint)package.OutPort,2);	//send port select
			Transfer((uint)package.Header,4);	//send header
			Transfer(InvertBits((uint)package.Data, 8),8);	//send data
			
			Board.SetDigitalOutput(DataPort,false);	//turn off port
			
			
		}
		
		public void TransferData(IComUnitPackage package) 
		{
			Ready = false;
			transferTimer = Stopwatch.StartNew();
			
			SendPackage(package);
			Console.WriteLine("time: " + transferTimer.ElapsedMilliseconds() + " (no: " + (transferCount++).ToString() + ") DATA: " + package.Data.ToString());
			if (!currentPackageOk || //Transfer failed due to exception
			    transferTimer.ElapsedMicroseconds() > delay * 15 + (delay/3)) //Transfer failed due to timeout
			{
				
				Thread.Sleep(500); //increase?
				TransferData (package);
			}
			
			Board.SetDigitalOutput(StatusPort, true);
			Delay(delay);
			Board.SetDigitalOutput(StatusPort, false);
			Ready = true;
		}
	}
}

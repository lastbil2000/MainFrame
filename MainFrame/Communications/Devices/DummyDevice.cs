using System;

namespace MainFrame.Devices
{
	public class DummyDevice : IDevice
	{
		public DummyDevice ()
		{
		}
		
		public int Value { get; set;}
		
		public void Methodx (string a_string) {
			Console.WriteLine (a_string);
		}

		#region IDevice implementation
		public void Start ()
		{
			
		}
	
		public void Stop ()
		{
			
		}
	
		public bool Ready {
			get {
				return true;
			}
		}
		#endregion
	}
}


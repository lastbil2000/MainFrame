
using System;

namespace MainFrame.Devices
{


	public class ComUnitPackage : IComUnitPackage
	{
		#region IComUnitPackage implementation
		public int OutPort  {get; set;}
		
		public int Header  {get; set;}
		
		public int Data {get; set;}
		
		public bool Success {get; set;}
		
		#endregion

		public ComUnitPackage ()
		{
		}
	}
}

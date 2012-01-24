using System;

namespace MainFrame.Devices
{


	public class DeviceContainer : Object
	{
		private string identifier;
		
		public string Identifier 
		{
			get { return identifier; }
			set 
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Identifier must not be null or empty");
				identifier = value;
			}
		}
		
		public IDevice Device  {get; set;}
		
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
    		
			DeviceContainer device = obj as DeviceContainer;
    		if ((System.Object)device == null)
        		return false;
			
			return device.Identifier == identifier;
		}
		
		public override int GetHashCode () 
		{
			return identifier.GetHashCode();
		}
	}
}

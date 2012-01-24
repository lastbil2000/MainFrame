using System;

namespace MainFrame.Processes
{


	public class ProcessContainer: Object
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
		
		public IProcess Process  {get; set;}
		
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
    		
			ProcessContainer process = obj as ProcessContainer;
    		if ((System.Object)process == null)
        		return false;
			
			return process.Identifier == identifier;
		}
		
		public override int GetHashCode () 
		{
			return identifier.GetHashCode();
		}
	}
}

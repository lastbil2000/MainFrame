using System;
using System.Text.RegularExpressions;
using System.ComponentModel;
using MainFrame.Communication;
using MainFrame.Devices;

namespace MainFrame.Devices
{


	public class BatteryLevelSensor : IRequestable, IDevice
	{
#pragma warning disable 0414
		private INervousSystem _mediator;
		
		public BatteryLevelSensor ()
		{
		}
		
		public int GetLevel ()
		{
			Match batteryResultMatch = new Regex(@"\d+\%").Match(
			                                                    SysLib.ExecuteCommandSync("acpi", " -b", 1));
			if (batteryResultMatch.Success)
				return Convert.ToInt16(
				       batteryResultMatch.Value.Substring(0,
				             batteryResultMatch.Value.Length - 1));
			else 
				return -1;
			
		}
		
		#region IRequestable implementation
		
		public class BatteryLevelSignal : INervousSignal<int>
		{
			public int Data {get; set;}
			public string Identifier {get; set;}
			
		}
		
		public void SetMediator (INervousSystem mediator)
		{
			_mediator = mediator;
		}
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof (BatteryLevelSignal);
			}
		}
		
		#endregion
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)
		
		{
			return (T)(object)GetLevel();
		}
		
		#endregion
		#region IDevice implementation
		public void Start () { }
		
		
		public void Stop () { }
		
		
		public bool Ready {
			get {
				return true;
			}
		}
		
		#endregion
	}
}

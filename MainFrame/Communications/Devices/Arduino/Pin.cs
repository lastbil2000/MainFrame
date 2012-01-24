
using System;

namespace MainFrame.Devices
{


	public class Pin : IPin
	{
		private IPinCom _com;
		private int _index;
		public Pin (IPinCom com, int index)
		{
			_com = com;
			_index = index;
		}
		
		#region IPin implementation
		
		public int Index {
			get {
				return _index;
			}
		}
		
		
		public bool On {
			get {
				return _com.GetPin(_index);
			}
			set {
				_com.SetPin(_index, value);
			}
		}
		
		#endregion
		#region IDevice implementation
		public void Start ()
		{
			On = false;
		}
		
		
		public void Stop ()
		{
			if(_com.Ready)
				On = false;
		}
		
		
		public bool Ready {
			get {
				return _com.Ready;
			}
		}
		
		#endregion


	}
}

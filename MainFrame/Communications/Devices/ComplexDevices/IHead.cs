using MainFrame.Communication;
using System;

namespace MainFrame.Devices
{


	public interface IHead : IDevice, IRequestable
	{
		double X {get; set;}
		double Y {get; set;}
	}
}

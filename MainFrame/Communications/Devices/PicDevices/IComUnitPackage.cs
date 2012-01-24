
using System;

namespace MainFrame.Devices
{

	public interface IComUnitPackage 
	{
		int OutPort {get; set;}
		int Header {get; set;}
		int Data {get; set;}
		bool Success {get; set;}
	}}

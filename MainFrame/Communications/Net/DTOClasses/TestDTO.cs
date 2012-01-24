using System;
using ProtoBuf;

namespace MainFrame.Communication.Net
{

	[ProtoContract]
	public class TestDTO : IDTO
	{

		public TestDTO ()
		{
			
		}
		
		[ProtoMember (1)]
		public string TestVal {get;set;}	
	}
}

using System;
using System.Runtime.InteropServices;
namespace MainFrame.Devices.Video
{


	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
	public struct CvRect {
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int x;
		[System.Runtime.InteropServices.FieldOffset(4)]
		public int y;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public int width;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public int height;
	};
}

using System.Runtime.InteropServices;
namespace MainFrame.Devices.Video
{
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
	public struct CvSize
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int width;
		[System.Runtime.InteropServices.FieldOffset(4)]
		public int height;
	}
}

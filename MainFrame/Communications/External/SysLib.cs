
using System;

namespace MainFrame
{


	public class SysLib
	{

		private SysLib ()
		{
		}
		
		
		
		//http://ckozler.net/?p=95
		public static string ExecuteCommandSync( string szCmd, string szArgs, int wait )
		{
			if( szCmd == null ) 
				throw new NullReferenceException("szCmd cannot be null");
			
			System.Diagnostics.Process myproc = new System.Diagnostics.Process( );
			myproc.EnableRaisingEvents = false;
			myproc.StartInfo.RedirectStandardOutput = true;
			myproc.StartInfo.UseShellExecute = false;
			myproc.StartInfo.CreateNoWindow = true;
			myproc.StartInfo.FileName = szCmd;
			myproc.StartInfo.Arguments = szArgs;
			
			if( myproc.Start( )  )
			{
				//Using WaitForExit( ) allows for the host program
				//to wait for the command its executing before it continues
				if( wait == 1 ) myproc.WaitForExit( );
				else myproc.Close( );
				return myproc.StandardOutput.ReadToEnd();
			}
			else return "";
		}

	}
}

using MainFrame.Devices.Video;
using MainFrame.Devices.Speech;
using System;
using MainFrame.Devices;
using MainFrame.Communication;
using System.Collections.Generic;

namespace MainFrame.Processes
{


	public class ProcessFactory : IDevice
	{

		public ProcessFactory ()
		{
		}
		
		public IProcess GetKeyboardProcess() 
		{
			return new KeyboardProcess();
		}
		
		public ICamera GetVideo(int cameraId) 
		{
			return new CameraProcess(cameraId);
		}
		
		public SphinxASRProcess GetASR(string arpa_lm, string dict, string hmm) 
		{
			return new SphinxASRProcess(arpa_lm, dict, hmm);
		}
		
		public SphinxASRProcess GetASR()
		{
			return new SphinxASRProcess(
				"language.arpa",
				"language.dict",
				"hub4wsj_sc_8k");
				
		}
		
		public ISerialCommunicationProcess GetArduino() 
		{
			return new ArduinoCommunicationProcess();
		}
		
		public LookAtPeopleProcess GetLookAtPeopleProcess(IVideoCapture capture, IHead head)
		{
			return new LookAtPeopleProcess(capture, head);
		}
		
		public RubyProcess GetRuby(string fileName) 
		{
			List<string>  paths = new List<string>();
			paths.Add("/usr/local/ironruby/lib/ruby/site_ruby/1.9.1");
			paths.Add("/usr/local/ironruby/lib/ruby/1.9.1");
			paths.Add("/usr/local/ironruby/lib/ironruby");
			paths.Add("/usr/bin");
			paths.Add("/usr/lib/ruby/1.9.1/x86_64-linux");
			paths.Add("/usr/lib/ruby/1.9.1");
			return new RubyProcess(fileName, paths);
		}

		public void Start () {}
		public void Stop () {}
		public bool Ready { get { return true; } }
	}
}

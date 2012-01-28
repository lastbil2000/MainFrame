using System;
using System.Runtime.InteropServices;
using MainFrame.Communication;

namespace MainFrame.Devices.Speech
{

	public class EspeakTTS : ITTS
	{

		public class SpeechSignal : INervousSignal <string>
		{
			public string Identifier {get; set;}
			public string Data {get; set;}
		}
		
		private const string dllPath = "espeak_stream.so";
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern int _ext_init();

		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern void _ext_speak(string text);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern void _ext_set_rate(int rate);

		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern void _ext_set_pitch(int pitch);
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern void _ext_stop();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern void _ext_pause();
		
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern int _ext_is_playing();
		
		private string _currentText;
		private INervousSystem _mediator;
		private bool _isQuiet;
		private bool _isStarted;
		private static readonly object _lock = new object();
/*
ERROR: Caught a segmentation fault while loading plugin file:
/usr/lib/gstreamer-0.10/libgstaudioconvert.so

Please either:
- remove it and restart.
- run with --gst-disable-segtrap --gst-disable-registry-fork and debug.

Press any key to continue...

*/
		public EspeakTTS ()
		{

		}

		#region ITTS implementation
		public void Say (string text)
		{
			lock (_lock) {
				_currentText = text;
				_mediator.Request(new SphinxASRProcess.SetListenStatus() {Data = false});
				_ext_speak(text);
				_mediator.Request(new SphinxASRProcess.SetListenStatus() {Data = true});
			}
		}

		public void Quiet ()
		{
			_ext_pause();
		}

		public bool IsQuiet {
			get {
				return _isQuiet;
			}
			set {
				_ext_pause();
				_isQuiet = value;
			}
		}

		public string CurrentText {
			get {
				return _currentText;
			}
		}
		#endregion

		#region IDevice implementation
		public void Start ()
		{
			if (_isStarted)
				return;
			
			if (_ext_init() != 1)
				throw new DeviceException("Unable to initialize espeak.");
			
			_isStarted = true;
			//StartUtterance ("yo baby");
		}

		public void Stop ()
		{
			_ext_stop();
			_isStarted = false;
		}

		public bool Ready {
			get {
				return _ext_is_playing() == 0 && _isStarted;
			}
		}
		#endregion

		#region IRequestable implementation
		public T Request<T> (MainFrame.Communication.INervousSignal<T> message)
		{
			if (message is SpeechSignal)
				Say(((SpeechSignal) message).Data);
			
			return default(T);
		}
		#endregion

		#region INode implementation
		public void SetMediator (MainFrame.Communication.INervousSystem mediator)
		{
			_mediator = mediator;
		}

		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof(SpeechSignal);
			}
		}
		#endregion
	}
}


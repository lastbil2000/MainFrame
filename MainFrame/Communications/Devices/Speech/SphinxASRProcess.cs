using System;
using System.Runtime.InteropServices;
using System.Threading;
using MainFrame.Communication; 
using MainFrame.Core.Logger;
using MainFrame.Processes;
namespace MainFrame.Devices.Speech
{

	
	public class SphinxASRinstanceHolder 
	{
		public static SphinxASRProcess ASR;
	}
	
	
	
	public enum ASRErrorCodes 
	{
		InitAlsaFailed = 1024,
		InitConverterFailed = 2048,
		InitVaderFailed = 4096,
		InitPocketSphinxFailed = 8192,
		LinkFailed = 16384,
		GstError = 32768,
		GstEOS = 131072,
		InitSinkFailed = 262144		
	}
	
	public class SphinxASRProcess : IProcess, IRequestable
	{
		
		
		public class SetListenStatus : INervousSignal<bool>
		{
			public string Identifier {get; set;}
			public bool Data {get; set;}
			
			public SetListenStatus() 
			{
				Type = typeof(SphinxASRProcess.SetListenStatus);
			}
			public Type Type{get; set;}
		}
		
		public class TextReceived : INervousSignal<string>
		{
			public string Identifier {get; set;}
			public string Data {get; set;}
			
			public TextReceived() 
			{
				Type = typeof(SphinxASRProcess.TextReceived);
			}
			public Type Type{get; set;}
		}
		
		protected delegate void TextInterpretedCallBack(string message);
		protected delegate void SphinxErrorCallBack(int errorType, string message);
		
//		public delegate void TextReceivedEventHandler(object sender, TextReceivedEventArgs e);
//		public event TextReceivedEventHandler TextReceived;

//		public delegate void ErrorReceivedEventHandler(object sender, ErrorReceivedEventArgs e);
//		public event ErrorReceivedEventHandler ErrorReceived;
		
		protected INervousSystem _mediator;
		
		public bool IsRunning {get; private set;}
		
		private const string dllPath = "sphinx_stream.so";
	
		[DllImport(dllPath, CharSet = CharSet.Auto)]
   	 	protected static extern int asr_start();

		[DllImport( dllPath)]
   	 	protected static extern int asr_turn_off();

		[DllImport( dllPath)]
   	 	protected static extern float amplification(float amplification);

		[DllImport( dllPath)]
   	 	protected static extern float cutoff(float cutoff);
		
		[DllImport(dllPath)]
   	 	protected static extern void set_is_active(bool reportMode);
		
		[DllImport(dllPath)]
   	 	protected static extern bool get_is_active();
		
		[DllImport(dllPath)]
   	 	protected static extern int asr_init(
		   	TextInterpretedCallBack msgFunc,
			SphinxErrorCallBack errorFunc,
		    string lmFile,
		    string dictFile,
		    string hmmFile);
		
		//[DllImport("sphinx_stream.so")]
   	 	//protected static extern void asr_set_text_received_callback(TextInterpretedCallBack msgFunc);

		//[DllImport("sphinx_stream.so")]
   	 	//protected static extern void asr_set_report_error_callback (SphinxErrorCallBack errorFunc);

		public SphinxASRProcess (string lm, string dic, string hmm)
		{
			SphinxASRinstanceHolder.ASR = this;
			
			if (SphinxASRProcess.asr_init(
					 new TextInterpretedCallBack(SphinxASRProcess.DefaultTextReceived), 
			         new SphinxErrorCallBack (SphinxASRProcess.DefaultErrorReceived),
					 	lm, dic, hmm) != 0)
			
				throw new ExternalException("Unable to initialize ASR engine");
			
			IsRunning = false;

		}
		
		protected static void DefaultTextReceived(string text)
		{
			Console.WriteLine("ASR: " + text);

			if (SphinxASRinstanceHolder.ASR._mediator == null)
				throw new NullReferenceException("No mediator set. Do it. NOW!");
			
			SphinxASRinstanceHolder.ASR._mediator.Request(new TextReceived() {Data = text});
			
		}
		
		protected static void DefaultErrorReceived (int errorType, string errorMessage)
		{
			Log.e("ASR ERROR: " + errorMessage + " type: " + errorType.ToString());
			
		}
		
		public bool Ready { get {return IsRunning;} }

		public void Start() 
		{

			if (IsRunning)
				throw new ProcessException("ASR is already running...");
			
			IsRunning = true;
			
			//starting the asr_loop:
			if (asr_start() == 1)
				throw new ProcessException("Unable to start asr!");
			
			IsRunning = false;
		}
		
		public void Stop ()
		{
			//TODO: use turnOff?
			//int turnOff = 
				SphinxASRProcess.asr_turn_off();
			
			SphinxASRinstanceHolder.ASR.IsRunning = false;
		}
		public float Amplification 
		{
			get { return amplification (0); }
			set { amplification(value); } 
		}
		
		public float Cutoff 
		{
			get { return cutoff (0); }
			set { cutoff(value); } 
		}
		
		public bool IsActive 
		{
			get 
			{
				return get_is_active();
			}
			set
			{
				set_is_active(value);
			}
		}

		public void StopListening () 
		{
			StopListening (null, null);
		}	

		public void StartListening () 
		{
			StartListening (null, null);
		}	

		public void StopListening (object sender, EventArgs e)
		{
			SphinxASRinstanceHolder.ASR.IsActive = false;
		}
		
		public void StartListening (object sender, EventArgs e)
		{
			Thread.Sleep(1000);
			SphinxASRinstanceHolder.ASR.IsActive = true;
		}
		
		
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)
		{
			
			if (message is SetListenStatus) 
			{
				SetListenStatus status = (SetListenStatus) message;
				if (status.Data == true)
					StartListening();
				else
					StopListening();
			}
			else if (message is GetSignal<SphinxASRProcess>) 
			{
				return (T)(object)this;
			}
			
			
			return default(T);
			
				
		}
		
		#endregion
		#region INode implementation
		public void SetMediator (INervousSystem mediator)
		{
			SphinxASRinstanceHolder.ASR._mediator = mediator;
		}
		
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof (SetListenStatus);
				yield return typeof (GetSignal<SphinxASRProcess>);
			}
		}
		
		#endregion
	}
	
}

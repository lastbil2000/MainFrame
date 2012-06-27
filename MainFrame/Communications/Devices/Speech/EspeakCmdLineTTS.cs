using System;
using System.Threading;
using Mono.Unix;
using MainFrame.Communication;
using MainFrame.Core.Logger;
using MainFrame.Devices.Speech;

namespace MainFrame.Devices.Speech
{

	public class EspeakTTSHolder 
	{
		public static EspeakCmdLineTTS TTS;
	}
		

	public class EspeakCmdLineTTS : ITTS
	{

		private INervousSystem mediator;
		
		public class SpeechSignal : INervousSignal <string>
		{
			public string Identifier {get; set;}
			public string Data {get; set;}
			public SpeechSignal() 
			{
				Type = typeof(EspeakCmdLineTTS.SpeechSignal);
			}
			public Type Type{get; set;}			
		}

		public EspeakCmdLineTTS () {
			EspeakTTSHolder.TTS = this;
			Speed = 140;
			Pitch = 50;

		}
		private bool isSpeaking;
		private bool isQuiet;
		public bool IsQuiet {
			get
			{
				return isQuiet;
			}
			set 
			{
				isQuiet = value;
			}
			
		}
		
		public int Speed {get; set;}
		public int Pitch {get; set;}
		
		public string CurrentText {get; protected set;}
		
		protected Thread speekThread;
				

		public void Say(string text)
		{
			CurrentText = text;
			//Log.w("I REFUSE TO SAY: " + text);
			//return;
			speekThread = new Thread(Speak);
			speekThread.IsBackground = true;
			speekThread.Start();
			
		}
		
		protected static void Speak ()
		{
			if (EspeakTTSHolder.TTS.isSpeaking) {
				Log.w("Espeak is told to speak in parallel. argh");
				return;
			}
			
			EspeakTTSHolder.TTS.isSpeaking = true;
			if (!EspeakTTSHolder.TTS.isQuiet)
			{
				
				EspeakTTSHolder.TTS.mediator.Request(new SphinxASRProcess.SetListenStatus() {Data = false});
				
				string voice = "en";
				string command =  "espeak \"" + 
					EspeakTTSHolder.TTS.CurrentText + "\" -v " + 
						voice + " -s " + 
						EspeakTTSHolder.TTS.Speed + " -p " + 
						EspeakTTSHolder.TTS.Pitch + " > /dev/null";
				
				Mono.Unix.Native.Stdlib.system(command);
				
				EspeakTTSHolder.TTS.mediator.Request(new SphinxASRProcess.SetListenStatus() {Data = true});
			}
			
			
			EspeakTTSHolder.TTS.isSpeaking = false;
			
			
		}
		
		//TODO: there must be a better way to handle this...
		public void Quiet()
		{
			speekThread.Abort();
			mediator.Request(new SphinxASRProcess.SetListenStatus() {Data = true});
		}
		
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)		
		{
			if (message is SpeechSignal)
				Say(((SpeechSignal) message).Data);
			
			return default(T);
		}
		
		#endregion
		#region INode implementation
		public void SetMediator (INervousSystem mediator)
		{
			this.mediator = mediator;
		}
		
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof(SpeechSignal);
			}
		}
		
		#endregion
		#region IDevice implementation
		public void Start ()
		{
			
		}
		
		
		public void Stop ()
		{
			
		}
		
		
		public bool Ready {
			get {
				return !isSpeaking;
			}
		}
		
		#endregion
	}
}

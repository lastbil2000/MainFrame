using System;
using MainFrame.Communication;
using MainFrame.Devices;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using IronRuby;

namespace MainFrame.Processes
{
	public class RubyProcess : IProcess
	{
		private bool _isRunning;
		private string _fileName;
		private ScriptEngine _engine;
		private ScriptScope _scope;
		private ScriptSource _source;
		
		private dynamic _mainClass;
		
		private MainFrame.Communication.INervousSystem _mediator;
		
		public RubyProcess (string fileName, MainFrame.Communication.INervousSystem mediator)
		{
			_mediator = mediator;
			_fileName = fileName;
			
			_engine = Ruby.CreateEngine();
			_scope = _engine.CreateScope();
			
			
		}
		
		public bool Ready { get {return IsRunning && _mainClass != null;} }
		
		#region IProcess implementation
		public void Start ()
		{
			_isRunning = true;
			_scope.SetVariable("robot", _mediator.Request<Robot>(new GetSignal<Robot>()));
			_scope.SetVariable("this", this);
			_source = _engine.CreateScriptSourceFromFile(_fileName);
			_source.Execute(_scope);
			
			_mainClass = _scope.GetVariable("main_class");
			_mainClass.@setup();
			
			while (_isRunning) 
			{
				_mainClass.@loop();
			}
		}
	
		public void Stop ()
		{
			_isRunning = false;
		}
	
		public bool IsRunning {
			get {
				return _isRunning;
			}
		}
		#endregion

		
		}
}


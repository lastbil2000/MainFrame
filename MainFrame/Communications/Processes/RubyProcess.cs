using System;
using MainFrame.Communication;
using MainFrame.Devices;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using System.Collections.Generic;
using IronRuby;
using MainFrame.Communication.Log;

namespace MainFrame.Processes
{
	public class RubyProcess : IProcess
	{
		public static readonly string HANDLE_MAIN_CLASS = "main_class";
		public static readonly string HANDLE_SHOULD_RUN = "main_class.should_run";		
		public static readonly string HANDLE_STOP = "main_class.stop";		
		public static readonly string HANDLE_SETUP = "main_class.setup";				
		public static readonly string HANDLE_ROBOT = "robot";
		
		private bool _isRunning;
		private string _fileName;
		private ScriptEngine _engine;
		private ScriptScope _scope;
		private ScriptSource _source;
		
		private dynamic _mainClass;
		
		private MainFrame.Communication.INervousSystem _mediator;
		
		public RubyProcess (string fileName, MainFrame.Communication.INervousSystem mediator, ICollection<string> paths)
		{
			_mediator = mediator;
			_fileName = fileName;
			
			_engine = Ruby.CreateEngine();
			_scope = _engine.CreateScope();
			_engine.SetSearchPaths(paths);

		}
		
		public bool Ready { get {return IsRunning && _mainClass != null;} }
		
		#region IProcess implementation
		public void Start ()
		{
			if (IsRunning)
				return;
			
			_scope.SetVariable(HANDLE_ROBOT, _mediator.Request<Robot>(new GetSignal<Robot>()));
			_source = _engine.CreateScriptSourceFromFile(_fileName);
			_source.Execute(_scope);
			
			System.Runtime.Remoting.ObjectHandle tmp;
			
			if (!_scope.TryGetVariableHandle(HANDLE_MAIN_CLASS, out tmp)) {
				StaticLogger.e("ERROR: no " + HANDLE_MAIN_CLASS + " defined for Ruby process: " + _fileName);
			}
				
			_mainClass = _scope.GetVariable(HANDLE_MAIN_CLASS);
			_mainClass.@setup();
			
			while (false != _mainClass.@should_run()) 
			{
				_mainClass.@loop();
			}
			
		}
	
		public void Stop ()
		{
			if (_mainClass != null)
				_mainClass.@stop();
		}
	
		public bool IsRunning {
			get {
				if (_mainClass == null)
					return false;
				
				return  _mainClass.@should_run() == true;
			}
		}
		#endregion

		
		}
}


using System;
using MainFrame.Communication;
using MainFrame.Devices;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using System.Collections.Generic;
using IronRuby;
using MainFrame.Communication.Log;
using Microsoft.CSharp.RuntimeBinder;

namespace MainFrame.Processes
{
	public class RubyProcess : IProcess, IRequestable, IWaitFor
	{
		public static readonly string HANDLE_MAIN_CLASS = "main_class";
		public static readonly string HANDLE_SHOULD_RUN = "main_class.should_run";		
		public static readonly string HANDLE_STOP = "main_class.stop";		
		public static readonly string HANDLE_SETUP = "main_class.setup";	
		public static readonly string HANDLE_MEDIATOR_TYPES = "mediator_types";
		public static readonly string HANDLE_ROBOT = "robot";
		
		private string _fileName;
		private ScriptEngine _engine;
		private ScriptScope _scope;
		private ScriptSource _source;
		
		private dynamic _mainClass;
		
		private bool _hasStarted;
		private bool _hasSyntaxErrors;
		
		private IDictionary<Type, string> _mediatorTypes;
		
		private MainFrame.Communication.INervousSystem _mediator;
		private readonly object _lock = new object();
		
		public RubyProcess (string fileName, ICollection<string> paths)
		{
	
			 
			_fileName = fileName;
			_mediatorTypes = new Dictionary<Type, string>();
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
			
			_hasSyntaxErrors = false;
			//lock (_lock) 
			{
				
				
				_scope.SetVariable(HANDLE_ROBOT, _mediator.Request<Robot>(new GetSignal<Robot>()));
				_source = _engine.CreateScriptSourceFromFile(_fileName);
				try 
				{
					_source.Execute(_scope);
				}
				catch (IronRuby.Builtins.SyntaxError ex) {
					_hasSyntaxErrors = true;
					StaticLogger.e ("Script: " + _fileName + " contains syntax error and will not be started: " + ex.Message);
					return;
				}
				System.Runtime.Remoting.ObjectHandle tmp;
				
				if (!_scope.TryGetVariableHandle(HANDLE_MAIN_CLASS, out tmp)) {
					StaticLogger.e("ERROR: no " + HANDLE_MAIN_CLASS + " defined for Ruby process: " + _fileName);
				}
					
				_mainClass = _scope.GetVariable(HANDLE_MAIN_CLASS);
				_mainClass.@setup();
				
				System.Runtime.Remoting.ObjectHandle tmp2;
				
				if (_scope.TryGetVariableHandle(HANDLE_MEDIATOR_TYPES, out tmp2)) {
					foreach (string typeNameShort in _mainClass.@mediator_types()) {
						string typeName = "MainFrame.Devices." + typeNameShort + ",Communications";
						if (System.Type.GetType(typeName) != null)
							_mediatorTypes.Add(System.Type.GetType(typeName), typeNameShort);
							//Console.WriteLine(System.Type.GetType(typeName));
						else
							throw new TypeLoadException("Type: " + typeName + " not found."); 
					}	
					
				}	
				
				_hasStarted = true;
				try
				{
					while (false != _mainClass.@should_run()) 
					{
						_mainClass.@loop();
					}
				}
				catch (Exception ex) 
				{
					StaticLogger.e(ex.StackTrace);
				}
			}
		}
	
		public void Stop ()
		{
			_mediatorTypes = new Dictionary<Type, string>();
			
			if (_mainClass != null)
				_mainClass.@stop();
			
			//System.Threading.Thread.Sleep(50);
			_hasStarted = false;
		}
	
		public bool IsRunning {
			get {
				if (_mainClass == null)
					return false;
				
				return  _mainClass.@should_run() == true && _hasStarted;
			}
		}
		#endregion

		
		#region IRequestable implementation
		public T Request<T> (MainFrame.Communication.INervousSignal<T> message)
		{
			
			if  (message.Type != null && _mediatorTypes.ContainsKey(message.Type)) 
			{
				dynamic result = _mainClass.@request(_mediatorTypes[message.Type], message);
				
				
				try 
				{
					return (T) result;	
				}
				//Fulhack för att hantera typkonverteringen...
				catch (RuntimeBinderException ex) {}
				
			}
			
			return default(T);
			
			
		}
		#endregion
	
		#region INode implementation
		public void SetMediator (MainFrame.Communication.INervousSystem mediator)
		{
			_mediator = mediator;

		}
	
		public System.Collections.Generic.IEnumerable<System.Type> Subjects 
		{
			get 
			{
				return _mediatorTypes.Keys;
			}
		}
	#endregion

		#region IWaitFor implementation
		public void WaitFor (int milliSeconds)
		{
			//TODO: ffs...
			while (!_hasStarted && !_hasSyntaxErrors)
				System.Threading.Thread.Sleep(500);
		}
		#endregion
	}
}


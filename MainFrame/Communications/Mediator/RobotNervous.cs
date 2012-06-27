using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using MainFrame.Core.Logger;

namespace MainFrame.Communication
{


	public class RobotNervous : INervousSystem
	{

		protected Dictionary<IRequestable,string> clients;
		public RobotNervous ()
		{
			clients = new Dictionary<IRequestable,string>();
		}
		
		public bool Contains(IRequestable client) 
		{
			return clients.ContainsKey(client);
		}
		
		public void Register (IRequestable client, string identifier) 
		{
			if (!Contains(client) )
				clients.Add(client, identifier);
			
			client.SetMediator(this);
			
		}
		public void Register (IRequestable client)
		{
			Register(client, GenerateIdentifier(client));
		}
		
		public void Unregister (IRequestable client) 
		{
			if (!Contains(client) )
				throw new ArgumentOutOfRangeException (client.ToString() + " not found.");

			clients.Remove(client);

		}
		
		public string GetIdentifier (IRequestable client) 
		{
			if (clients.ContainsKey(client))
				return clients[client];
			
			return GenerateIdentifier(client);
		}
		
		private string GenerateIdentifier(IRequestable client) 
		{
			return client.GetHashCode().ToString("X8");
		}
		
		//TODO: add supporT for multiple responses
		T INervousSystem.Request<T> (INervousSignal<T> message)
		{
			if (message == null)
				throw new ApplicationException("Message was null. We need a message to determine type.");
			
			foreach(IRequestable client in clients.Keys)
				foreach (Type subject in client.Subjects)
					//TODO: thread lock here for the specific IRequestable (+ id?)
					if 	(subject == message.GetType() &&
						(message.Identifier == null || 
						 message.Identifier == clients[client]))
						{
							return client.Request<T>(message);
						}
			
			Log.w ("WARNING: unheard signal: " + message.GetType());
			return default(T);
		}
	

		#region IDevice implementation
		public void Start ()
		{
			
		}
	
		public void Stop ()
		{
			
		}
	
		public bool Ready {
			get {
				return true;
			}
		}
		#endregion
	}
}

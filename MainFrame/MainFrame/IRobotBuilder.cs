
using System;

namespace MainFrame
{


	public interface IRobotBuilder
	{
		Robot GetRobot();
		void BuildInternals();
		void BuildArm();
		void BuildHead();
		void AttachProcesses();
		void BuildSpeech ();
		void BuildSensors();
		void BuidEngine();
	}
}

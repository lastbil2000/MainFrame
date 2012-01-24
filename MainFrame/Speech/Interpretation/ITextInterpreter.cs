using System;

namespace MainFrame.Speech
{


	public interface ITextInterpreter
	{
		event EventHandler TextInterpreted;
		event EventHandler TextNotInterpreted;
		void TextReceived(object sender, EventArgs e);
	}
}

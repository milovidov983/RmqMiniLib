using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal interface ICommands {
		IRmqHandler GetHandler(string topic);
		string[] GetAllTopics();
	}
}

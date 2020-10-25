using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal interface IRmqLogger {
		void Debug(string message);
		void Error(string message);
		void Info(string message);
		void Warn(string message);
	}
}

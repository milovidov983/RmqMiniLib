using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal interface IConnectionFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		IConnectionService Create();
	}
}

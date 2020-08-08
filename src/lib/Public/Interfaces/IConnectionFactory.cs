using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	public interface IConnectionFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		IConnection Create();
	}
}

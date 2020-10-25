using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Contracts.Infrastructure {
	public class MicroServiceException : Exception {
		public readonly StatusCodes StatusCode;

		public MicroServiceException(string message, StatusCodes statusCode) : base(message) {
			StatusCode = statusCode;
		}

		public MicroServiceException(string message, Exception ex, StatusCodes statusCode) : base(message, ex) {
			StatusCode = statusCode;
		}
	}
}

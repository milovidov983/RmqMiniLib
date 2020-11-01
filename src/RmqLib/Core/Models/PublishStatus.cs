using System;

namespace RmqLib.Core {
	internal class PublishStatus {
		public bool IsSuccess { get; set; }
		public Exception Error { get; set; }

		public static PublishStatus SuccessStatus = new PublishStatus {
			IsSuccess = true
		};
	}
}

using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerExample.Service.Infrastructure {
	public class RequestContext {
		public readonly DeliveredMessage Message;
		public readonly Database Db;
		public readonly RequestState State = new RequestState();

		public RequestContext(DeliveredMessage messasge, Database db) {
			Message = messasge;
			Db = db;
		}
	}
}

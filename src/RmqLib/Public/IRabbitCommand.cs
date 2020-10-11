﻿using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
		public interface IRabbitCommand {
			Task<MessageProcessResult> Execute(RequestContext message);
			void WithHub(IRabbitHub hub);
		}
	
}
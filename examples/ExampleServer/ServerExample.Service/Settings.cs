using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerExample.Service {
	public class Settings {
		public RmqConfig RmqConfig { get; set; }
		public string Env { get; set; }
	}
}

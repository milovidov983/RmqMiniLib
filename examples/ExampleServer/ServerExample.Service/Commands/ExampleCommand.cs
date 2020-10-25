using ServerExample.Service.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerExample.Service.Commands {
	public class ExampleCommand : ReadonlyCommandHandler {
		public ExampleCommand(Context context) : base(context) {
		}

		protected override Task ExecuteImpl(RequestContext ctx) {
			throw new NotImplementedException();
		}
	}
}

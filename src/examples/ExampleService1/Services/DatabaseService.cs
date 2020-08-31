using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services {
	public class DatabaseService {
		private readonly Dictionary<int, string> db = new Dictionary<int, string> {
			[1] = "HELLO ONE",
			[2] = "HELLO TWO",
		};
		public Task<string> GetData(int id) {
			return Task.FromResult(db.GetValueOrDefault(id));
		}
	}
}

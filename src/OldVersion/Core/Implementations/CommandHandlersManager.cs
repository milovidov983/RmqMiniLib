using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RmqLib.Core {
	internal class CommandHandlersManager : ICommandHandlersManager {
		private readonly ConcurrentDictionary<string, IRmqCommandHandler> commandsHandlers;
		private readonly string[] allTopics;

		internal CommandHandlersManager(List<IRmqCommandHandler> commandImplementations) {
			var cmdHandlers = commandImplementations.ToDictionary(
				x => x.Topic,
				x => x
				);

			allTopics = cmdHandlers.Keys.ToArray();
			commandsHandlers = new ConcurrentDictionary<string, IRmqCommandHandler>(cmdHandlers);
		}

		public IRmqCommandHandler GetHandler(string topic) {
			var isTopicExists = commandsHandlers.TryGetValue(topic, out var command);
			if (isTopicExists) {
				return command;
			}

			throw new RmqException($"Rmq error: handler for command \"{topic}\" not found!", Error.INTERNAL_ERROR);
		}

		public string[] GetAllTopics() {
			return allTopics;
		}
	}
}

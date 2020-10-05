using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace RmqLib2 {
	internal static class InternalHelpers {


        public static string GetString(this ReadOnlyMemory<byte> memory) {
            var body = memory.ToArray();
            return Encoding.UTF8.GetString(body);
        }

    }
}

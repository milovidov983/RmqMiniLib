using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace RmqLib.Helper {
	internal static class InternalHelpers {

        public static byte[] ToByteArray<TRequest>(this TRequest request) {
            var json = JsonSerializer.Serialize(request);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}

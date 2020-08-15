﻿using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace RmqLib {
	internal static class InternalHelpers {
        public static byte[] ObjectToByteArray(object obj) {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static string GetString(this ReadOnlyMemory<byte> memory) {
            var body = memory.ToArray();
            return Encoding.UTF8.GetString(body);
        }

        public static T GetContent<T>(this BasicDeliverEventArgs ea) where T : class {
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            try {
                return JsonSerializer.Deserialize<T>(response);
            } catch(Exception e) {
                var exceptionMessage = $"Failed deserialize incoming data: {e.Message}";
                throw new RmqException(exceptionMessage, e, Error.INTERNAL_ERROR);
			}
        }



        public static string GetFormattedElapsedTime(string title, TimeSpan elapsed) {
            var formattedElapsed = $"{elapsed.TotalMilliseconds} ms";

            return $"{title} message processed. Elapsed time: {formattedElapsed}";
        }

        public static void GetAllTopics() {
            var commands = Assembly.GetEntryAssembly()
                    .GetTypes()
                    .Where(p => typeof(IRmqCommandHandler).IsAssignableFrom(p) && !p.IsInterface)
                    .ToList();

            var notificationHandlers = Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(p => typeof(IRmqNotificationHandler).IsAssignableFrom(p) && !p.IsInterface)
                .ToList();


        }
    }
}
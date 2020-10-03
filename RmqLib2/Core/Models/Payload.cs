using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib2 {
    public class Payload {
        public readonly string Error;
        public readonly byte[] Body;
        public readonly int? StatusCode;

        public Payload(byte[] body, string error,int? statusCode) {
            Body = body ?? Array.Empty<byte>();
			Error = error;
			StatusCode = statusCode;
        }
    }
}

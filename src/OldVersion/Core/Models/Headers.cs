using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
    public static class Headers {
        // TODO  убрать лишние
        // TODO всем использовать этот справочник
        public static readonly string BinaryOffsets = "-x-binary-offsets";
        public static readonly string CreatedAt = "-x-createdAt";
        public static readonly string Topic = "-x-topic";
        public static readonly string Certificate = "-x-certificate";
        public static readonly string Signature = "-x-signature";
        public static readonly string HubVersion = "-x-hub-version";
        public static readonly string RetryCount = "-x-retry-count";
        public static readonly string Error = "-x-error";
        public static readonly string StatusCode = "-x-status-code";
        public static readonly string OnBehalfOf = "-x-on-behalf-of";
        public static readonly string FromHost = "-x-from-host";
        public static readonly string IsRetryForbidden = "-x-retry-forbidden";
        public static readonly string SignatureType = "-x-signature-type";
        public static readonly string ExtendedInfo = "-x-extended-info";
        public static readonly string RpcTimeout = "-x-rpc-timeout";
        public static readonly string DigestInterval = "-x-digest-interval";
        public static readonly string CustomData = "-x-data";

    }
}

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Browser.Interop
{
    internal class TrackedReference
    {
        private TrackedReference(string id, object reference)
        {
            Id = id;
            TrackedInstance = reference;
        }
        private static IDictionary<string, TrackedReference> References { get; } =
            new Dictionary<string, TrackedReference>();

        public string Id { get; }

        public object TrackedInstance { get; }

        public static void Track(string id, object reference)
        {
            var trackedRef = new TrackedReference(id, reference);
            if (References.ContainsKey(id))
            {
                throw new InvalidOperationException($"An element with id '{id}' is already being tracked.");
            }

            References.Add(id, trackedRef);
        }

        public static void Untrack(string id)
        {
            if (!References.ContainsKey(id))
            {
                throw new InvalidOperationException($"An element with id '{id}' is not being tracked.");
            }
        }

        public static TrackedReference Get(string id)
        {
            if (!References.ContainsKey(id))
            {
                throw new InvalidOperationException($"An element with id '{id}' is not being tracked.");
            }

            return References[id];
        }
    }
}

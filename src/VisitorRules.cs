using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class VisitorRules
    {
        internal const int NextOpeningDelay = 1;

        private static readonly HashSet<string> ExpeditedClientIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "courrierRedImp",
            "courrierRedImpFree",
            "courrierChem",
            "courrierChemFree",
            "courrierMining",
            "courrierMiningFree",
            "courrierPharma",
            "courrierPharmaFree",
            "wanted4Normal"
        };

        internal static bool IsExpedited(string? identifier)
        {
            return identifier != null && ExpeditedClientIds.Contains(identifier);
        }

        internal static string? GetAppointmentKey(string? identifier)
        {
            if (!IsExpedited(identifier)) return null;
            switch (identifier)
            {
                case "courrierRedImpFree": return "courrierRedImp";
                case "courrierChemFree": return "courrierChem";
                case "courrierMiningFree": return "courrierMining";
                case "courrierPharmaFree": return "courrierPharma";
                default: return identifier;
            }
        }

        internal static int RemoveDuplicateAppointments(IList<string> identifiers, IList<int> delays)
        {
            if (identifiers == null) throw new ArgumentNullException(nameof(identifiers));
            if (delays == null) throw new ArgumentNullException(nameof(delays));
            if (identifiers.Count != delays.Count)
                throw new ArgumentException("Future client identifiers and delays must have equal lengths.");

            var seen = new HashSet<string>(StringComparer.Ordinal);
            int removed = 0;
            for (int i = 0; i < identifiers.Count; i++)
            {
                string? appointmentKey = GetAppointmentKey(identifiers[i]);
                if (appointmentKey == null || seen.Add(appointmentKey)) continue;
                identifiers.RemoveAt(i);
                delays.RemoveAt(i);
                removed++;
                i--;
            }
            return removed;
        }

        internal static int NormalizeDelays(IList<string> identifiers, IList<int> delays)
        {
            if (identifiers == null) throw new ArgumentNullException(nameof(identifiers));
            if (delays == null) throw new ArgumentNullException(nameof(delays));
            if (identifiers.Count != delays.Count)
                throw new ArgumentException("Future client identifiers and delays must have equal lengths.");

            int changed = 0;
            for (int i = 0; i < identifiers.Count; i++)
            {
                if (!IsExpedited(identifiers[i]) || delays[i] == NextOpeningDelay) continue;
                delays[i] = NextOpeningDelay;
                changed++;
            }
            return changed;
        }

        internal static int Prioritize<T>(List<T> queue, Func<T, string?> getIdentifier)
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));
            if (getIdentifier == null) throw new ArgumentNullException(nameof(getIdentifier));

            List<T> expedited = queue.FindAll(item => IsExpedited(getIdentifier(item)));
            if (expedited.Count == 0) return 0;

            queue.RemoveAll(item => IsExpedited(getIdentifier(item)));
            queue.InsertRange(0, expedited);
            return expedited.Count;
        }
    }
}

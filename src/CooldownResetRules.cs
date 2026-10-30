using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal readonly struct CooldownResetResult
    {
        internal CooldownResetResult(int resetCount, int skippedCount)
        {
            ResetCount = resetCount;
            SkippedCount = skippedCount;
        }

        internal int ResetCount { get; }
        internal int SkippedCount { get; }
    }

    internal static class CooldownResetRules
    {
        private static readonly long[] ResetContactNumbers =
        {
            StorePhoneClient.RED_IMP_REP_NUMBER,
            StorePhoneClient.MINING_NUMBER,
            StorePhoneClient.PHARMA_NUMBER,
            StorePhoneClient.CHEM_NUMBER,
            StorePhoneClient.REV_MERCHANT_NUMBER
        };

        private static readonly IReadOnlyDictionary<long, string[]> AppointmentIds =
            new Dictionary<long, string[]>
            {
                { StorePhoneClient.RED_IMP_REP_NUMBER, new[] { "courrierRedImp", "courrierRedImpFree" } },
                { StorePhoneClient.MINING_NUMBER, new[] { "courrierMining", "courrierMiningFree" } },
                { StorePhoneClient.PHARMA_NUMBER, new[] { "courrierPharma", "courrierPharmaFree" } },
                { StorePhoneClient.CHEM_NUMBER, new[] { "courrierChem", "courrierChemFree" } },
                { StorePhoneClient.REV_MERCHANT_NUMBER, new[] { "wanted4Normal" } }
            };

        internal static bool IsResetTarget(long number)
        {
            return AppointmentIds.ContainsKey(number);
        }

        internal static bool HasPendingAppointment(long number, IList<string> futureAppointments)
        {
            if (futureAppointments == null) throw new ArgumentNullException(nameof(futureAppointments));
            if (!AppointmentIds.TryGetValue(number, out string[]? identifiers)) return false;

            foreach (string appointment in futureAppointments)
            {
                foreach (string identifier in identifiers)
                {
                    if (string.Equals(appointment, identifier, StringComparison.Ordinal)) return true;
                }
            }
            return false;
        }

        internal static CooldownResetResult ResetCooldowns<T>(
            IReadOnlyDictionary<long, T> contacts,
            IList<string> futureAppointments,
            Func<T, bool> isCoolingDown,
            Action<T> reset)
        {
            if (contacts == null) throw new ArgumentNullException(nameof(contacts));
            if (futureAppointments == null) throw new ArgumentNullException(nameof(futureAppointments));
            if (isCoolingDown == null) throw new ArgumentNullException(nameof(isCoolingDown));
            if (reset == null) throw new ArgumentNullException(nameof(reset));

            int resetCount = 0;
            int skippedCount = 0;
            foreach (long number in ResetContactNumbers)
            {
                if (!contacts.TryGetValue(number, out T? contact)) continue;
                if (HasPendingAppointment(number, futureAppointments))
                {
                    skippedCount++;
                    continue;
                }
                if (!isCoolingDown(contact)) continue;
                reset(contact);
                resetCount++;
            }
            return new CooldownResetResult(resetCount, skippedCount);
        }
    }
}

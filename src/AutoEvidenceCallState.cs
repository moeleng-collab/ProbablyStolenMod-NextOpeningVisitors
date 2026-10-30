using System;
using System.Globalization;

namespace NextOpeningVisitors
{
    internal enum AutoEvidenceAcceptanceStatus
    {
        Valid,
        NoPendingOffer,
        StaleOffer,
        InvestigationAlreadyClear,
        InvalidPrice,
        InsufficientCash
    }

    internal sealed class AutoEvidenceCallState
    {
        internal const int CurrentSchemaVersion = 1;
        internal const int InitialPrice = 1000;

        internal int SchemaVersion = CurrentSchemaVersion;
        internal int LastOfferDay = -1;
        internal bool HasPendingOffer;
        internal int OfferPrice;
        internal int OfferDay = -1;

        internal bool BeginDay(int day, int evidenceLevel, int currentPrice)
        {
            if (HasPendingOffer && OfferDay != day) ClearPending();
            if (evidenceLevel < 100)
            {
                if (HasPendingOffer) ClearPending();
                return false;
            }
            if (HasPendingOffer || LastOfferDay >= day || currentPrice <= 0) return false;

            LastOfferDay = day;
            OfferDay = day;
            OfferPrice = currentPrice;
            HasPendingOffer = true;
            return true;
        }

        internal bool Reject(int day)
        {
            if (!HasPendingOffer || OfferDay != day) return false;
            ClearPending();
            return true;
        }

        internal void CancelPending()
        {
            ClearPending();
        }

        internal void CompleteAcceptance()
        {
            ClearPending();
        }

        internal string Serialize()
        {
            return string.Join("|", new[]
            {
                SchemaVersion.ToString(CultureInfo.InvariantCulture),
                LastOfferDay.ToString(CultureInfo.InvariantCulture),
                HasPendingOffer ? "1" : "0",
                OfferPrice.ToString(CultureInfo.InvariantCulture),
                OfferDay.ToString(CultureInfo.InvariantCulture)
            });
        }

        internal static bool TryDeserialize(string serialized, out AutoEvidenceCallState state)
        {
            state = new AutoEvidenceCallState();
            if (string.IsNullOrEmpty(serialized)) return true;
            string[] parts = serialized.Split('|');
            if (parts.Length != 5 ||
                !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int schema) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int lastOfferDay) ||
                !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int offerPrice) ||
                !int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int offerDay) ||
                (parts[2] != "0" && parts[2] != "1"))
                return false;

            bool pending = parts[2] == "1";
            if (schema != CurrentSchemaVersion || lastOfferDay < -1 || offerPrice < 0 || offerDay < -1 ||
                (pending && (offerPrice <= 0 || offerDay < 0)) ||
                (!pending && (offerPrice != 0 || offerDay != -1)))
                return false;

            state.SchemaVersion = schema;
            state.LastOfferDay = lastOfferDay;
            state.HasPendingOffer = pending;
            state.OfferPrice = offerPrice;
            state.OfferDay = offerDay;
            return true;
        }

        private void ClearPending()
        {
            HasPendingOffer = false;
            OfferPrice = 0;
            OfferDay = -1;
        }
    }

    internal static class AutoEvidenceCallRules
    {
        internal static AutoEvidenceAcceptanceStatus ValidateAcceptance(
            AutoEvidenceCallState state, int day, int evidenceLevel, int playerCash)
        {
            if (state == null || !state.HasPendingOffer) return AutoEvidenceAcceptanceStatus.NoPendingOffer;
            if (state.OfferDay != day) return AutoEvidenceAcceptanceStatus.StaleOffer;
            if (evidenceLevel < 100) return AutoEvidenceAcceptanceStatus.InvestigationAlreadyClear;
            if (state.OfferPrice <= 0) return AutoEvidenceAcceptanceStatus.InvalidPrice;
            if (playerCash < state.OfferPrice) return AutoEvidenceAcceptanceStatus.InsufficientCash;
            return AutoEvidenceAcceptanceStatus.Valid;
        }

        internal static int GetNextPrice(int currentPrice)
        {
            if (currentPrice <= 0) throw new ArgumentOutOfRangeException(nameof(currentPrice));
            double next = Math.Round(currentPrice * 1.2);
            if (next > int.MaxValue) throw new OverflowException("The automatic evidence cleanup price exceeds the supported range.");
            return (int)next;
        }
    }
}

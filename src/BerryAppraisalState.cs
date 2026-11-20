using System;
using System.Collections.Generic;
using System.Globalization;

namespace NextOpeningVisitors
{
    internal enum BerryMarkerStyle
    {
        None = 0,
        Moldy = 1,
        Dry = 2,
        Normal = 3,
        Small = 4
    }

    internal sealed class BerryAppraisalState
    {
        private const int CurrentSchemaVersion = 1;
        private readonly Dictionary<int, BerryQuality> _qualities = new Dictionary<int, BerryQuality>();

        internal int Count => _qualities.Count;

        internal bool Mark(int uniqueId, BerryQuality quality)
        {
            if (uniqueId <= 0) return false;
            if (quality == BerryQuality.Unknown) return _qualities.Remove(uniqueId);
            if (!IsKnown(quality)) return false;
            _qualities[uniqueId] = quality;
            return true;
        }

        internal bool TryGetQuality(int uniqueId, out BerryQuality quality)
        {
            return _qualities.TryGetValue(uniqueId, out quality);
        }

        internal string Serialize()
        {
            var ids = new List<int>(_qualities.Keys);
            ids.Sort();
            var entries = new List<string>(ids.Count);
            foreach (int id in ids)
            {
                entries.Add(id.ToString(CultureInfo.InvariantCulture) + ":" +
                    ((int)_qualities[id]).ToString(CultureInfo.InvariantCulture));
            }
            return CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture) + "|" + string.Join(",", entries);
        }

        internal static bool TryDeserialize(string serialized, out BerryAppraisalState state)
        {
            state = new BerryAppraisalState();
            if (string.IsNullOrEmpty(serialized)) return true;
            string[] sections = serialized.Split('|');
            if (sections.Length != 2 || sections[0] != CurrentSchemaVersion.ToString(CultureInfo.InvariantCulture))
                return false;
            if (sections[1].Length == 0) return true;

            string[] entries = sections[1].Split(',');
            foreach (string entry in entries)
            {
                string[] pair = entry.Split(':');
                if (pair.Length != 2 ||
                    !int.TryParse(pair[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int uniqueId) ||
                    !int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int numericQuality) ||
                    uniqueId <= 0 || !IsKnown((BerryQuality)numericQuality) || state._qualities.ContainsKey(uniqueId))
                    return false;
                state._qualities.Add(uniqueId, (BerryQuality)numericQuality);
            }
            return true;
        }

        private static bool IsKnown(BerryQuality quality)
        {
            return quality == BerryQuality.Normal || quality == BerryQuality.Small ||
                quality == BerryQuality.Moldy || quality == BerryQuality.Unripe || quality == BerryQuality.Dry;
        }
    }
}

using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class NightOwlRules
    {
        internal const string PerkId = "nextopeningvisitors_night_owl";
        internal const int PerkCost = 2;

        internal static bool HasVisitedToday(string locationId, int visits, int lastVisitDay, int currentDay)
        {
            return !string.IsNullOrEmpty(locationId) && visits > 0 && lastVisitDay == currentDay;
        }

        internal static bool CanVisit(bool nightOwlActive, string locationId, int visits, int lastVisitDay, int currentDay)
        {
            return !nightOwlActive || !HasVisitedToday(locationId, visits, lastVisitDay, currentDay);
        }

        internal static bool HasUnvisited<T>(
            IEnumerable<T> locations,
            Func<T, string> locationId,
            Func<T, int> visits,
            Func<T, int> lastVisitDay,
            int currentDay) where T : class
        {
            if (locations == null) return false;
            foreach (T location in locations)
            {
                if (location == null) continue;
                string id = locationId(location);
                if (!string.IsNullOrEmpty(id) &&
                    !HasVisitedToday(id, visits(location), lastVisitDay(location), currentDay))
                    return true;
            }
            return false;
        }
    }
}

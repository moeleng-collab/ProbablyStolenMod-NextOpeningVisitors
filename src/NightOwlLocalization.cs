using System;

namespace NextOpeningVisitors
{
    internal static class NightOwlLocalization
    {
        internal static string Name(string? localeCode)
        {
            switch (ModLocalization.NormalizeLocale(localeCode))
            {
                case "zh": return "夜猫子";
                case "ja": return "夜猫族";
                case "de": return "Nachteule";
                case "ru": return "Сова";
                case "fr": return "Noctambule";
                case "es": return "Noctámbulo";
                case "pl": return "Nocny marek";
                case "ptBR": return "Notívago";
                case "ko": return "올빼미";
                case "it": return "Nottambulo";
                default: return "Night Owl";
            }
        }

        internal static string Description(string? localeCode)
        {
            switch (ModLocalization.NormalizeLocale(localeCode))
            {
                case "zh": return "夜间外出不限次数，但同一地点每晚只能前往一次。其他外出限制仍然有效。";
                case "ja": return "夜間の外出回数が無制限になりますが、同じ場所には1晩に1回しか行けません。その他の外出制限は有効です。";
                case "de": return "Nachts unbegrenzt oft ausgehen, aber jeder Ort ist nur einmal pro Nacht besuchbar. Andere Ausgangsbeschränkungen bleiben bestehen.";
                case "ru": return "Неограниченное число ночных выходов, но каждое место можно посетить лишь раз за ночь. Прочие ограничения сохраняются.";
                case "fr": return "Sorties nocturnes illimitées, mais chaque lieu n’est accessible qu’une fois par nuit. Les autres restrictions de sortie restent actives.";
                case "es": return "Salidas nocturnas ilimitadas, pero cada lugar solo se puede visitar una vez por noche. Las demás restricciones siguen activas.";
                case "pl": return "Nieograniczona liczba nocnych wyjść, ale każde miejsce można odwiedzić tylko raz na noc. Pozostałe ograniczenia nadal obowiązują.";
                case "ptBR": return "Saídas noturnas ilimitadas, mas cada local só pode ser visitado uma vez por noite. As demais restrições continuam ativas.";
                case "ko": return "야간 외출 횟수가 무제한이지만 같은 장소는 하룻밤에 한 번만 방문할 수 있습니다. 다른 외출 제한은 그대로 적용됩니다.";
                case "it": return "Uscite notturne illimitate, ma ogni luogo può essere visitato una sola volta a notte. Le altre restrizioni restano attive.";
                default: return "Unlimited nightly outings, but each location can only be visited once per night. Other outing restrictions still apply.";
            }
        }

        internal static string RepeatVisit(string? localeCode)
        {
            switch (ModLocalization.NormalizeLocale(localeCode))
            {
                case "zh": return "夜猫子：今晚已经去过这个地点。";
                case "ja": return "夜猫族：この場所には今夜すでに行きました。";
                case "de": return "Nachteule: Dieser Ort wurde heute Nacht bereits besucht.";
                case "ru": return "Сова: это место уже посещено сегодня ночью.";
                case "fr": return "Noctambule : ce lieu a déjà été visité cette nuit.";
                case "es": return "Noctámbulo: ya has visitado este lugar esta noche.";
                case "pl": return "Nocny marek: to miejsce zostało już odwiedzone tej nocy.";
                case "ptBR": return "Notívago: este local já foi visitado nesta noite.";
                case "ko": return "올빼미: 오늘 밤 이미 방문한 장소입니다.";
                case "it": return "Nottambulo: hai già visitato questo luogo stanotte.";
                default: return "Night Owl: this location has already been visited tonight.";
            }
        }

        internal static string AllLocationsVisited(string? localeCode)
        {
            switch (ModLocalization.NormalizeLocale(localeCode))
            {
                case "zh": return "夜猫子：今晚没有尚未去过的地点。";
                case "ja": return "夜猫族：今夜まだ行っていない場所はありません。";
                case "de": return "Nachteule: Alle Orte wurden heute Nacht bereits besucht.";
                case "ru": return "Сова: сегодня ночью посещены все доступные места.";
                case "fr": return "Noctambule : tous les lieux ont déjà été visités cette nuit.";
                case "es": return "Noctámbulo: ya has visitado todos los lugares esta noche.";
                case "pl": return "Nocny marek: tej nocy odwiedzono już wszystkie miejsca.";
                case "ptBR": return "Notívago: todos os locais já foram visitados nesta noite.";
                case "ko": return "올빼미: 오늘 밤 갈 수 있는 장소를 모두 방문했습니다.";
                case "it": return "Nottambulo: hai già visitato tutti i luoghi stanotte.";
                default: return "Night Owl: all locations have already been visited tonight.";
            }
        }
    }
}

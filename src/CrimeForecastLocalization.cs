using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class CrimeForecastLocalization
    {
        internal const int TradeEvidence = 0;
        internal const int TradeCrime = 1;
        internal const int NightCrime = 2;
        internal const int NightDelta = 3;
        internal const int NightEvidence = 4;
        internal const int CapWarning = 5;
        internal const int ServiceTitle = 6;
        internal const int ServiceDescription = 7;
        internal const int ServiceStatus = 8;
        internal const int CallOffer = 9;
        internal const int Accept = 10;
        internal const int Decline = 11;
        internal const int Success = 12;
        internal const int InsufficientCash = 13;
        internal const int Rejected = 14;
        internal const int Expired = 15;
        internal const int Failure = 16;
        internal const int CallOfferPrice = 17;
        internal const int CallOfferTerms = 18;
        internal const int KeyCount = 19;

        private static readonly string[] English =
        {
            "Investigation: {0}/100",
            "Crime value: {0} → {1} (+{2})",
            "Night crime value: {0} → {1}",
            "Expected investigation change: +{0}",
            "Next investigation: {0} → {1}/100",
            " (will reach the investigation cap)",
            "Automatic Evidence Cleanup",
            "When investigation reaches 100, Wild calls automatically. Accepting pays cash to reset investigation and case state only; crime values and dispatched officers remain.",
            "Automatic service enabled. Next accepted cleanup: {0} cash.",
            "Your investigation is at 100/100. Things are getting hot over there.",
            "Pay {0} to clear investigation",
            "Not today",
            "Investigation cleared. Next cleanup price: {0} cash.",
            "Not enough cash. You need {0}; no charge was made.",
            "Understood. I can call again tomorrow if investigation is still at 100.",
            "Investigation no longer needs clearing. No charge was made.",
            "Cleanup failed. No charge was made; check the log.",
            "For {0} cash, I can reset your investigation and case state.",
            "Crime values and dispatched officers remain. This buys you breathing room, not immunity. Deal?"
        };

        private static readonly Dictionary<string, string[]> Tables = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", English },
            { "zh", new[]
            {
                "调查进度：{0}/100", "案值：{0} → {1}（+{2}）", "预计夜间案值：{0} → {1}",
                "预计调查变化：+{0}", "次日调查进度：{0} → {1}/100", "（将达到调查上限）",
                "自动证据清除", "调查达到 100 后王哥自动来电。接受后仅用现金清除调查进度和案件状态；案值与已派遣治安官保留。",
                "自动服务已启用。下一次接受清除价格：{0} 现金。",
                "哟，兄弟，你那边可有点热闹了。调查都顶到 100 了，再不管，治安部就该登门跟你聊聊了。",
                "{0} 给你，动手吧。", "今天先不办，我再想想。", "成了，调查已经替你清干净了，案子状态也归位了。下回再找我，{0}。兄弟，价钱只会往上涨，别总把自己玩到这条线上。",
                "兄弟，{0} 都凑不齐，这事儿可没法办。先去凑钱吧，这次不动你的账。", "行，那今天你自己掂量着办。明天调查要是还顶在 100，我再给你打。到时候可别怪我没提醒你。",
                "呵，看来已经有人替你收拾过了。那没我什么事了，钱你留着。", "等等……这边出了点岔子，事儿没办成。钱没动，调查也没碰。让懂行的人看看日志，别自己瞎折腾。",
                "给我 {0} 现金，我替你把手尾收一收：调查清零，案子状态归位。",
                "不过丑话说前头——案值还在，已经派出来的治安官也撤不回来。我卖的是喘息的机会，不是免死金牌。怎么样，办不办？"
            } },
            { "ja", new[]
            {
                "調査進行：{0}/100", "案値：{0} → {1}（+{2}）", "夜間の予想案値：{0} → {1}",
                "予想調査変化：+{0}", "翌日の調査：{0} → {1}/100", "（調査上限に到達）",
                "自動証拠消去", "調査が100に達するとWildが自動で電話します。承諾すると現金で調査と事件状態のみをリセットします。案値と派遣済み警官は残ります。",
                "自動サービス有効。次回の消去費用：{0} 現金。",
                "調査が100/100です。そろそろまずい状況です。",
                "{0} を払って調査を消去", "今日は辞退", "調査を消去しました。次回費用：{0} 現金。",
                "現金不足です。{0} が必要です。支払いはありません。", "了解しました。明日も100なら再度電話します。",
                "消去は不要になりました。支払いはありません。", "消去に失敗しました。支払いはありません。ログを確認してください。",
                "{0} 現金で、調査と事件状態をリセットしましょう。",
                "案値と派遣済み警官は残ります。買えるのは猶予だけです。どうしますか？"
            } },
            { "de", new[]
            {
                "Ermittlung: {0}/100", "Tatwert: {0} → {1} (+{2})", "Nächtlicher Tatwert: {0} → {1}",
                "Erwartete Ermittlungsänderung: +{0}", "Nächste Ermittlung: {0} → {1}/100", " (Ermittlungslimit wird erreicht)",
                "Automatische Beweislöschung", "Bei 100 Ermittlung ruft Wild automatisch an. Annahme setzt nur Ermittlung und Fallstatus gegen Bargeld zurück; Tatwerte und entsandte Beamte bleiben.",
                "Automatischer Dienst aktiv. Nächste Löschung: {0} Bargeld.",
                "Deine Ermittlung steht bei 100/100. Es wird brenzlig für dich.",
                "{0} zahlen und Ermittlung löschen", "Heute nicht", "Ermittlung gelöscht. Nächster Preis: {0} Bargeld.",
                "Nicht genug Bargeld. Benötigt: {0}; keine Abbuchung.", "Verstanden. Ich rufe morgen wieder an, falls die Ermittlung noch 100 beträgt.",
                "Keine Löschung mehr nötig. Keine Abbuchung.", "Löschung fehlgeschlagen. Keine Abbuchung; Protokoll prüfen.",
                "Für {0} Bargeld setze ich Ermittlung und Fallstatus zurück.",
                "Tatwerte und entsandte Beamte bleiben. Du kaufst dir nur Zeit, keine Straffreiheit. Sind wir im Geschäft?"
            } },
            { "ru", new[]
            {
                "Расследование: {0}/100", "Сумма дел: {0} → {1} (+{2})", "Ночная сумма дел: {0} → {1}",
                "Ожидаемое изменение расследования: +{0}", "Расследование завтра: {0} → {1}/100", " (будет достигнут предел)",
                "Автоочистка доказательств", "При расследовании 100 Уайлд позвонит автоматически. Согласие за наличные сбросит только расследование и статус дела; суммы дел и офицеры останутся.",
                "Автоуслуга включена. Следующая очистка: {0} наличными.",
                "Расследование достигло 100/100. Тебе стоит поторопиться.",
                "Заплатить {0} и очистить расследование", "Не сегодня", "Расследование очищено. Следующая цена: {0} наличными.",
                "Недостаточно наличных. Нужно {0}; оплата не взималась.", "Понял. Позвоню завтра, если расследование снова будет 100.",
                "Очистка больше не нужна. Оплата не взималась.", "Очистка не удалась. Оплата не взималась; проверьте журнал.",
                "За {0} наличными я сброшу расследование и статус дела.",
                "Суммы дел и отправленные офицеры останутся. Я продаю передышку, а не неприкосновенность. Договорились?"
            } },
            { "fr", new[]
            {
                "Enquête : {0}/100", "Valeur des affaires : {0} → {1} (+{2})", "Valeur nocturne : {0} → {1}",
                "Variation prévue de l’enquête : +{0}", "Enquête demain : {0} → {1}/100", " (le plafond sera atteint)",
                "Effacement automatique des preuves", "À 100 d’enquête, Wild appelle automatiquement. Accepter paie en espèces et réinitialise seulement l’enquête et l’affaire ; les valeurs et agents restent.",
                "Service automatique actif. Prochain effacement : {0} en espèces.",
                "Votre enquête est à 100/100. La situation devient chaude.",
                "Payer {0} pour effacer l’enquête", "Pas aujourd’hui", "Enquête effacée. Prochain prix : {0} en espèces.",
                "Espèces insuffisantes. Il faut {0} ; aucun débit.", "Compris. Je rappellerai demain si l’enquête est toujours à 100.",
                "Aucun effacement nécessaire. Aucun débit.", "Échec de l’effacement. Aucun débit ; consultez le journal.",
                "Pour {0} en espèces, je réinitialise l’enquête et l’état de l’affaire.",
                "Les valeurs et les agents dépêchés restent. J’offre un répit, pas une immunité. Marché conclu ?"
            } },
            { "es", new[]
            {
                "Investigación: {0}/100", "Valor de los casos: {0} → {1} (+{2})", "Valor nocturno: {0} → {1}",
                "Cambio previsto de investigación: +{0}", "Investigación mañana: {0} → {1}/100", " (alcanzará el límite)",
                "Borrado automático de pruebas", "Al llegar a 100, Wild llama automáticamente. Aceptar paga en efectivo y solo reinicia investigación y estado del caso; los valores y agentes permanecen.",
                "Servicio automático activo. Próximo borrado: {0} en efectivo.",
                "Tu investigación está en 100/100. La cosa se está calentando.",
                "Pagar {0} para borrar la investigación", "Hoy no", "Investigación borrada. Próximo precio: {0} en efectivo.",
                "Efectivo insuficiente. Necesitas {0}; no se cobró nada.", "Entendido. Volveré a llamar mañana si sigue en 100.",
                "Ya no hace falta borrarla. No se cobró nada.", "Error al borrar. No se cobró nada; revisa el registro.",
                "Por {0} en efectivo, reinicio tu investigación y el estado del caso.",
                "Los valores y los agentes enviados permanecen. Te vendo un respiro, no inmunidad. ¿Trato hecho?"
            } },
            { "pl", new[]
            {
                "Śledztwo: {0}/100", "Wartość spraw: {0} → {1} (+{2})", "Nocna wartość spraw: {0} → {1}",
                "Przewidywana zmiana śledztwa: +{0}", "Śledztwo jutro: {0} → {1}/100", " (limit zostanie osiągnięty)",
                "Automatyczne usuwanie dowodów", "Przy śledztwie 100 Wild dzwoni automatycznie. Akceptacja za gotówkę resetuje tylko śledztwo i stan sprawy; wartości i funkcjonariusze pozostają.",
                "Usługa automatyczna aktywna. Następne usunięcie: {0} gotówką.",
                "Śledztwo wynosi 100/100. Robi się niebezpiecznie.",
                "Zapłać {0} i wyczyść śledztwo", "Nie dzisiaj", "Śledztwo wyczyszczone. Następna cena: {0} gotówką.",
                "Za mało gotówki. Potrzeba {0}; nie pobrano opłaty.", "Rozumiem. Zadzwonię jutro, jeśli śledztwo nadal będzie wynosić 100.",
                "Czyszczenie nie jest już potrzebne. Nie pobrano opłaty.", "Czyszczenie nie powiodło się. Nie pobrano opłaty; sprawdź dziennik.",
                "Za {0} gotówką zresetuję śledztwo i stan sprawy.",
                "Wartości i wysłani funkcjonariusze zostają. Sprzedaję chwilę oddechu, nie immunitet. Zgoda?"
            } },
            { "ptBR", new[]
            {
                "Investigação: {0}/100", "Valor dos casos: {0} → {1} (+{2})", "Valor noturno: {0} → {1}",
                "Mudança prevista da investigação: +{0}", "Investigação amanhã: {0} → {1}/100", " (atingirá o limite)",
                "Limpeza automática de provas", "Ao chegar a 100, Wild liga automaticamente. Aceitar paga em dinheiro e redefine apenas investigação e estado do caso; valores e policiais permanecem.",
                "Serviço automático ativo. Próxima limpeza: {0} em dinheiro.",
                "Sua investigação está em 100/100. O clima está esquentando.",
                "Pagar {0} para limpar a investigação", "Hoje não", "Investigação limpa. Próximo preço: {0} em dinheiro.",
                "Dinheiro insuficiente. Você precisa de {0}; nada foi cobrado.", "Entendido. Ligo amanhã se a investigação ainda estiver em 100.",
                "A limpeza não é mais necessária. Nada foi cobrado.", "Falha na limpeza. Nada foi cobrado; veja o registro.",
                "Por {0} em dinheiro, eu redefino sua investigação e o estado do caso.",
                "Os valores e os policiais enviados permanecem. Vendo um respiro, não imunidade. Fechado?"
            } },
            { "ko", new[]
            {
                "조사 진행: {0}/100", "사건 가치: {0} → {1} (+{2})", "야간 예상 사건 가치: {0} → {1}",
                "예상 조사 변화: +{0}", "다음 날 조사: {0} → {1}/100", " (조사 상한에 도달)",
                "자동 증거 제거", "조사가 100이 되면 Wild가 자동으로 전화합니다. 수락하면 현금으로 조사와 사건 상태만 초기화하며 사건 가치와 파견된 경찰은 유지됩니다.",
                "자동 서비스 활성화됨. 다음 제거 비용: {0} 현금.",
                "조사가 100/100입니다. 슬슬 위험해지고 있습니다.",
                "{0}을 지불하고 조사 제거", "오늘은 안 됨", "조사가 제거되었습니다. 다음 가격: {0} 현금.",
                "현금이 부족합니다. {0}이 필요하며 결제되지 않았습니다.", "알겠습니다. 내일도 100이면 다시 전화하겠습니다.",
                "더는 제거할 필요가 없습니다. 결제되지 않았습니다.", "제거에 실패했습니다. 결제되지 않았습니다. 로그를 확인하세요.",
                "{0} 현금으로 조사와 사건 상태를 초기화해 드리겠습니다.",
                "사건 가치와 파견된 경찰은 그대로입니다. 파는 건 시간이지 면죄부가 아닙니다. 하시겠습니까?"
            } },
            { "it", new[]
            {
                "Indagine: {0}/100", "Valore dei casi: {0} → {1} (+{2})", "Valore notturno: {0} → {1}",
                "Variazione prevista dell’indagine: +{0}", "Indagine domani: {0} → {1}/100", " (raggiungerà il limite)",
                "Cancellazione automatica prove", "A 100, Wild chiama automaticamente. Accettare paga in contanti e azzera solo indagine e stato del caso; valori e agenti restano.",
                "Servizio automatico attivo. Prossima cancellazione: {0} in contanti.",
                "La tua indagine è a 100/100. La situazione si sta scaldando.",
                "Paga {0} per cancellare l’indagine", "Non oggi", "Indagine cancellata. Prossimo prezzo: {0} in contanti.",
                "Contanti insufficienti. Servono {0}; nessun addebito.", "Capito. Richiamerò domani se l’indagine sarà ancora a 100.",
                "Non serve più cancellare. Nessun addebito.", "Cancellazione non riuscita. Nessun addebito; controlla il registro.",
                "Per {0} in contanti, azzero indagine e stato del caso.",
                "Valori e agenti inviati restano. Ti vendo una tregua, non l’immunità. Affare fatto?"
            } }
        };

        internal static string Get(int key, string localeCode, params object[] args)
        {
            string locale = ModLocalization.NormalizeLocale(localeCode);
            if (!Tables.TryGetValue(locale, out string[]? table) || table == null) table = English;
            string format = key >= 0 && key < table.Length && !string.IsNullOrEmpty(table[key])
                ? table[key]
                : English[key];
            return args == null || args.Length == 0 ? format : string.Format(format, args);
        }
    }
}

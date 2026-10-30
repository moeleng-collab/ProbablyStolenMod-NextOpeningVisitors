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
        internal const int KeyCount = 17;

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
            "Your investigation is at 100/100. I can clear it for {0} cash. This resets only investigation and case state; crime values and dispatched officers remain.",
            "Pay {0} to clear investigation",
            "Not today",
            "Investigation cleared. Next cleanup price: {0} cash.",
            "Not enough cash. You need {0}; no charge was made.",
            "Understood. I can call again tomorrow if investigation is still at 100.",
            "Investigation no longer needs clearing. No charge was made.",
            "Cleanup failed. No charge was made; check the log."
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
                "你的调查已达到 100/100。支付 {0} 现金即可清除。只重置调查进度和案件状态，案值与已派遣治安官不会撤销。",
                "支付 {0} 清除调查", "今天不用", "调查已清除。下次清除价格：{0} 现金。",
                "现金不足，需要 {0}；本次未扣款。", "明白。若明天调查仍为 100，我可以再次来电。",
                "调查已无需清除，本次未扣款。", "清除失败，未扣款；请查看日志。"
            } },
            { "ja", new[]
            {
                "調査進行：{0}/100", "案値：{0} → {1}（+{2}）", "夜間の予想案値：{0} → {1}",
                "予想調査変化：+{0}", "翌日の調査：{0} → {1}/100", "（調査上限に到達）",
                "自動証拠消去", "調査が100に達するとWildが自動で電話します。承諾すると現金で調査と事件状態のみをリセットします。案値と派遣済み警官は残ります。",
                "自動サービス有効。次回の消去費用：{0} 現金。",
                "調査が100/100です。{0} 現金で消去できます。調査と事件状態のみをリセットし、案値と派遣済み警官は残ります。",
                "{0} を払って調査を消去", "今日は辞退", "調査を消去しました。次回費用：{0} 現金。",
                "現金不足です。{0} が必要です。支払いはありません。", "了解しました。明日も100なら再度電話します。",
                "消去は不要になりました。支払いはありません。", "消去に失敗しました。支払いはありません。ログを確認してください。"
            } },
            { "de", new[]
            {
                "Ermittlung: {0}/100", "Tatwert: {0} → {1} (+{2})", "Nächtlicher Tatwert: {0} → {1}",
                "Erwartete Ermittlungsänderung: +{0}", "Nächste Ermittlung: {0} → {1}/100", " (Ermittlungslimit wird erreicht)",
                "Automatische Beweislöschung", "Bei 100 Ermittlung ruft Wild automatisch an. Annahme setzt nur Ermittlung und Fallstatus gegen Bargeld zurück; Tatwerte und entsandte Beamte bleiben.",
                "Automatischer Dienst aktiv. Nächste Löschung: {0} Bargeld.",
                "Deine Ermittlung steht bei 100/100. Für {0} Bargeld lösche ich sie. Nur Ermittlung und Fallstatus werden zurückgesetzt; Tatwerte und Beamte bleiben.",
                "{0} zahlen und Ermittlung löschen", "Heute nicht", "Ermittlung gelöscht. Nächster Preis: {0} Bargeld.",
                "Nicht genug Bargeld. Benötigt: {0}; keine Abbuchung.", "Verstanden. Ich rufe morgen wieder an, falls die Ermittlung noch 100 beträgt.",
                "Keine Löschung mehr nötig. Keine Abbuchung.", "Löschung fehlgeschlagen. Keine Abbuchung; Protokoll prüfen."
            } },
            { "ru", new[]
            {
                "Расследование: {0}/100", "Сумма дел: {0} → {1} (+{2})", "Ночная сумма дел: {0} → {1}",
                "Ожидаемое изменение расследования: +{0}", "Расследование завтра: {0} → {1}/100", " (будет достигнут предел)",
                "Автоочистка доказательств", "При расследовании 100 Уайлд позвонит автоматически. Согласие за наличные сбросит только расследование и статус дела; суммы дел и офицеры останутся.",
                "Автоуслуга включена. Следующая очистка: {0} наличными.",
                "Расследование достигло 100/100. Очищу за {0} наличными. Сбросятся только расследование и статус дела; суммы и офицеры останутся.",
                "Заплатить {0} и очистить расследование", "Не сегодня", "Расследование очищено. Следующая цена: {0} наличными.",
                "Недостаточно наличных. Нужно {0}; оплата не взималась.", "Понял. Позвоню завтра, если расследование снова будет 100.",
                "Очистка больше не нужна. Оплата не взималась.", "Очистка не удалась. Оплата не взималась; проверьте журнал."
            } },
            { "fr", new[]
            {
                "Enquête : {0}/100", "Valeur des affaires : {0} → {1} (+{2})", "Valeur nocturne : {0} → {1}",
                "Variation prévue de l’enquête : +{0}", "Enquête demain : {0} → {1}/100", " (le plafond sera atteint)",
                "Effacement automatique des preuves", "À 100 d’enquête, Wild appelle automatiquement. Accepter paie en espèces et réinitialise seulement l’enquête et l’affaire ; les valeurs et agents restent.",
                "Service automatique actif. Prochain effacement : {0} en espèces.",
                "Votre enquête est à 100/100. Je peux l’effacer pour {0} en espèces. Seuls l’enquête et l’état de l’affaire sont réinitialisés ; les valeurs et agents restent.",
                "Payer {0} pour effacer l’enquête", "Pas aujourd’hui", "Enquête effacée. Prochain prix : {0} en espèces.",
                "Espèces insuffisantes. Il faut {0} ; aucun débit.", "Compris. Je rappellerai demain si l’enquête est toujours à 100.",
                "Aucun effacement nécessaire. Aucun débit.", "Échec de l’effacement. Aucun débit ; consultez le journal."
            } },
            { "es", new[]
            {
                "Investigación: {0}/100", "Valor de los casos: {0} → {1} (+{2})", "Valor nocturno: {0} → {1}",
                "Cambio previsto de investigación: +{0}", "Investigación mañana: {0} → {1}/100", " (alcanzará el límite)",
                "Borrado automático de pruebas", "Al llegar a 100, Wild llama automáticamente. Aceptar paga en efectivo y solo reinicia investigación y estado del caso; los valores y agentes permanecen.",
                "Servicio automático activo. Próximo borrado: {0} en efectivo.",
                "Tu investigación está en 100/100. Puedo borrarla por {0} en efectivo. Solo se reinician investigación y estado; los valores y agentes permanecen.",
                "Pagar {0} para borrar la investigación", "Hoy no", "Investigación borrada. Próximo precio: {0} en efectivo.",
                "Efectivo insuficiente. Necesitas {0}; no se cobró nada.", "Entendido. Volveré a llamar mañana si sigue en 100.",
                "Ya no hace falta borrarla. No se cobró nada.", "Error al borrar. No se cobró nada; revisa el registro."
            } },
            { "pl", new[]
            {
                "Śledztwo: {0}/100", "Wartość spraw: {0} → {1} (+{2})", "Nocna wartość spraw: {0} → {1}",
                "Przewidywana zmiana śledztwa: +{0}", "Śledztwo jutro: {0} → {1}/100", " (limit zostanie osiągnięty)",
                "Automatyczne usuwanie dowodów", "Przy śledztwie 100 Wild dzwoni automatycznie. Akceptacja za gotówkę resetuje tylko śledztwo i stan sprawy; wartości i funkcjonariusze pozostają.",
                "Usługa automatyczna aktywna. Następne usunięcie: {0} gotówką.",
                "Śledztwo wynosi 100/100. Usunę je za {0} gotówką. Resetuje to tylko śledztwo i stan sprawy; wartości i funkcjonariusze pozostają.",
                "Zapłać {0} i wyczyść śledztwo", "Nie dzisiaj", "Śledztwo wyczyszczone. Następna cena: {0} gotówką.",
                "Za mało gotówki. Potrzeba {0}; nie pobrano opłaty.", "Rozumiem. Zadzwonię jutro, jeśli śledztwo nadal będzie wynosić 100.",
                "Czyszczenie nie jest już potrzebne. Nie pobrano opłaty.", "Czyszczenie nie powiodło się. Nie pobrano opłaty; sprawdź dziennik."
            } },
            { "ptBR", new[]
            {
                "Investigação: {0}/100", "Valor dos casos: {0} → {1} (+{2})", "Valor noturno: {0} → {1}",
                "Mudança prevista da investigação: +{0}", "Investigação amanhã: {0} → {1}/100", " (atingirá o limite)",
                "Limpeza automática de provas", "Ao chegar a 100, Wild liga automaticamente. Aceitar paga em dinheiro e redefine apenas investigação e estado do caso; valores e policiais permanecem.",
                "Serviço automático ativo. Próxima limpeza: {0} em dinheiro.",
                "Sua investigação está em 100/100. Posso limpar por {0} em dinheiro. Apenas investigação e estado são redefinidos; valores e policiais permanecem.",
                "Pagar {0} para limpar a investigação", "Hoje não", "Investigação limpa. Próximo preço: {0} em dinheiro.",
                "Dinheiro insuficiente. Você precisa de {0}; nada foi cobrado.", "Entendido. Ligo amanhã se a investigação ainda estiver em 100.",
                "A limpeza não é mais necessária. Nada foi cobrado.", "Falha na limpeza. Nada foi cobrado; veja o registro."
            } },
            { "ko", new[]
            {
                "조사 진행: {0}/100", "사건 가치: {0} → {1} (+{2})", "야간 예상 사건 가치: {0} → {1}",
                "예상 조사 변화: +{0}", "다음 날 조사: {0} → {1}/100", " (조사 상한에 도달)",
                "자동 증거 제거", "조사가 100이 되면 Wild가 자동으로 전화합니다. 수락하면 현금으로 조사와 사건 상태만 초기화하며 사건 가치와 파견된 경찰은 유지됩니다.",
                "자동 서비스 활성화됨. 다음 제거 비용: {0} 현금.",
                "조사가 100/100입니다. {0} 현금으로 제거할 수 있습니다. 조사와 사건 상태만 초기화되며 사건 가치와 경찰은 유지됩니다.",
                "{0}을 지불하고 조사 제거", "오늘은 안 됨", "조사가 제거되었습니다. 다음 가격: {0} 현금.",
                "현금이 부족합니다. {0}이 필요하며 결제되지 않았습니다.", "알겠습니다. 내일도 100이면 다시 전화하겠습니다.",
                "더는 제거할 필요가 없습니다. 결제되지 않았습니다.", "제거에 실패했습니다. 결제되지 않았습니다. 로그를 확인하세요."
            } },
            { "it", new[]
            {
                "Indagine: {0}/100", "Valore dei casi: {0} → {1} (+{2})", "Valore notturno: {0} → {1}",
                "Variazione prevista dell’indagine: +{0}", "Indagine domani: {0} → {1}/100", " (raggiungerà il limite)",
                "Cancellazione automatica prove", "A 100, Wild chiama automaticamente. Accettare paga in contanti e azzera solo indagine e stato del caso; valori e agenti restano.",
                "Servizio automatico attivo. Prossima cancellazione: {0} in contanti.",
                "La tua indagine è a 100/100. Posso cancellarla per {0} in contanti. Solo indagine e stato vengono azzerati; valori e agenti restano.",
                "Paga {0} per cancellare l’indagine", "Non oggi", "Indagine cancellata. Prossimo prezzo: {0} in contanti.",
                "Contanti insufficienti. Servono {0}; nessun addebito.", "Capito. Richiamerò domani se l’indagine sarà ancora a 100.",
                "Non serve più cancellare. Nessun addebito.", "Cancellazione non riuscita. Nessun addebito; controlla il registro."
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

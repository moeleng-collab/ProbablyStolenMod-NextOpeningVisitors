using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class FeatureSettingsLocalization
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: Features",
            ["ui.intro"] = "Each toggle applies immediately and is saved with the run. Press F8 again to close.",
            ["ui.enableAll"] = "Enable all",
            ["ui.disableAll"] = "Disable all",
            ["ui.groupVisitors"] = "Visitors and phone",
            ["ui.groupForecasts"] = "Forecasts and item highlights",
            ["ui.groupAppraisal"] = "Appraisal and identity",
            ["feature.VisitorPriority"] = "Prioritize suppliers and Odin next opening",
            ["feature.AppointmentDeduplication"] = "Merge duplicate appointments, keeping the earliest",
            ["feature.PhoneCooldownReset"] = "One-click phone cooldown reset",
            ["feature.PhoneDialogueText"] = "Next-opening phone dialogue text",
            ["feature.TradeForecast"] = "Trade crime and investigation forecast",
            ["feature.NightForecast"] = "Nightly crime and investigation forecast",
            ["feature.AutomaticEvidenceCleanup"] = "Automatic evidence-cleanup calls",
            ["feature.CustomerDemandHighlight"] = "Customer-demand item highlights",
            ["feature.SecurityInspectionHighlight"] = "Security-inspection item highlights",
            ["feature.GunsmithHighlight"] = "Gunsmith-order item highlights",
            ["feature.AutomaticAppraisalValue"] = "Automatic accurate appraisal values",
            ["feature.AppraisalDetails"] = "Magnifier authenticity, value and quality details",
            ["feature.BerryQualityMarker"] = "Appraised glowberry quality markers",
            ["feature.MagnifierAugDetection"] = "Magnifier augmented-human detection and reasons",
            ["feature.ArrivalIdentityAlert"] = "Augmented and wanted customer arrival alerts",
        };
        private static readonly Dictionary<string, string> Chinese = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening：功能开关",
            ["ui.intro"] = "每项独立生效并随存档保存。再次按 F8 关闭。",
            ["ui.enableAll"] = "全部开启",
            ["ui.disableAll"] = "全部关闭",
            ["ui.groupVisitors"] = "访客与电话",
            ["ui.groupForecasts"] = "预测与物品高亮",
            ["ui.groupAppraisal"] = "鉴定与身份提醒",
            ["feature.VisitorPriority"] = "供货商与奥丁下一次开店优先到访",
            ["feature.AppointmentDeduplication"] = "合并重复预约并保留最早时间",
            ["feature.PhoneCooldownReset"] = "电话联系人冷却一键清除",
            ["feature.PhoneDialogueText"] = "电话文案改为下一次开店",
            ["feature.TradeForecast"] = "交易案值与调查进度预测",
            ["feature.NightForecast"] = "夜间案值与调查进度预测",
            ["feature.AutomaticEvidenceCleanup"] = "调查满值自动清除来电",
            ["feature.CustomerDemandHighlight"] = "顾客需求物品高亮",
            ["feature.SecurityInspectionHighlight"] = "治安盘查物品高亮",
            ["feature.GunsmithHighlight"] = "武器定制与改造物品高亮",
            ["feature.AutomaticAppraisalValue"] = "放大镜自动填入真实估值",
            ["feature.AppraisalDetails"] = "放大镜真伪、价值与品质详情",
            ["feature.BerryQualityMarker"] = "荧光莓鉴定后品质标志",
            ["feature.MagnifierAugDetection"] = "放大镜改造人检测与理由",
            ["feature.ArrivalIdentityAlert"] = "改造体与通缉犯到店提醒",
        };
        private static readonly Dictionary<string, string> Japanese = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening：機能スイッチ",
            ["ui.intro"] = "各項目はすぐ適用され、セーブデータに保存されます。F8をもう一度押すと閉じます。",
            ["ui.enableAll"] = "すべて有効",
            ["ui.disableAll"] = "すべて無効",
            ["ui.groupVisitors"] = "来客と電話",
            ["ui.groupForecasts"] = "予測とアイテム強調",
            ["ui.groupAppraisal"] = "鑑定と身元警告",
            ["feature.VisitorPriority"] = "次回開店時に仕入先とオーディンを優先",
            ["feature.AppointmentDeduplication"] = "重複予約をまとめ、最も早い時間を残す",
            ["feature.PhoneCooldownReset"] = "電話のクールダウンをワンクリック解除",
            ["feature.PhoneDialogueText"] = "電話文面を次回開店に変更",
            ["feature.TradeForecast"] = "取引の事件値と調査進度の予測",
            ["feature.NightForecast"] = "夜間の事件値と調査進度の予測",
            ["feature.AutomaticEvidenceCleanup"] = "調査上限時の自動証拠消去電話",
            ["feature.CustomerDemandHighlight"] = "客の需要アイテムを強調",
            ["feature.SecurityInspectionHighlight"] = "治安検査アイテムを強調",
            ["feature.GunsmithHighlight"] = "武器のカスタムと改造アイテムを強調",
            ["feature.AutomaticAppraisalValue"] = "拡大鏡で正確な鑑定値を自動入力",
            ["feature.AppraisalDetails"] = "拡大鏡の真贋・価値・品質の詳細",
            ["feature.BerryQualityMarker"] = "鑑定済みグロウベリーの品質マーク",
            ["feature.MagnifierAugDetection"] = "拡大鏡による強化人間の検出と理由",
            ["feature.ArrivalIdentityAlert"] = "強化人間・指名手配客の来店警告",
        };
        private static readonly Dictionary<string, string> German = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: Funktionen",
            ["ui.intro"] = "Jede Option wirkt sofort und wird mit dem Spielstand gespeichert. Erneut F8 zum Schließen.",
            ["ui.enableAll"] = "Alle aktivieren",
            ["ui.disableAll"] = "Alle deaktivieren",
            ["ui.groupVisitors"] = "Besucher und Telefon",
            ["ui.groupForecasts"] = "Prognosen und Gegenstandsmarkierung",
            ["ui.groupAppraisal"] = "Prüfung und Identität",
            ["feature.VisitorPriority"] = "Lieferanten und Odin bei der nächsten Öffnung bevorzugen",
            ["feature.AppointmentDeduplication"] = "Doppelte Termine zusammenführen und den frühesten behalten",
            ["feature.PhoneCooldownReset"] = "Telefon-Abklingzeiten mit einem Klick löschen",
            ["feature.PhoneDialogueText"] = "Telefondialog auf nächste Öffnung umstellen",
            ["feature.TradeForecast"] = "Prognose zu Handelsstraftaten und Ermittlungen",
            ["feature.NightForecast"] = "Nächtliche Prognose zu Straftaten und Ermittlungen",
            ["feature.AutomaticEvidenceCleanup"] = "Automatische Anrufe zur Beweismittelbeseitigung",
            ["feature.CustomerDemandHighlight"] = "Markierung der von Kunden nachgefragten Gegenstände",
            ["feature.SecurityInspectionHighlight"] = "Markierung der Gegenstände für Sicherheitskontrollen",
            ["feature.GunsmithHighlight"] = "Markierung der Gegenstände für Waffenschmiedaufträge",
            ["feature.AutomaticAppraisalValue"] = "Automatische genaue Prüfungswerte",
            ["feature.AppraisalDetails"] = "Echtheit, Wert und Qualität unter der Lupe",
            ["feature.BerryQualityMarker"] = "Qualitätsmarkierungen geprüfter Glow Berries",
            ["feature.MagnifierAugDetection"] = "Erkennung Augmentierter unter der Lupe samt Begründung",
            ["feature.ArrivalIdentityAlert"] = "Ankunftshinweise für augmentierte und gesuchte Kunden",
        };
        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: функции",
            ["ui.intro"] = "Каждый переключатель действует сразу и сохраняется с записью. Нажмите F8 ещё раз, чтобы закрыть.",
            ["ui.enableAll"] = "Включить все",
            ["ui.disableAll"] = "Отключить все",
            ["ui.groupVisitors"] = "Посетители и телефон",
            ["ui.groupForecasts"] = "Прогнозы и подсветка предметов",
            ["ui.groupAppraisal"] = "Оценка и личность",
            ["feature.VisitorPriority"] = "Приоритет поставщикам и Одину при следующем открытии",
            ["feature.AppointmentDeduplication"] = "Объединять повторяющиеся записи, оставляя самое раннее время",
            ["feature.PhoneCooldownReset"] = "Сброс телефонной перезарядки одним нажатием",
            ["feature.PhoneDialogueText"] = "Тексты звонков про следующее открытие",
            ["feature.TradeForecast"] = "Прогноз торговых преступлений и расследования",
            ["feature.NightForecast"] = "Ночной прогноз преступлений и расследования",
            ["feature.AutomaticEvidenceCleanup"] = "Автоматические звонки для очистки улик",
            ["feature.CustomerDemandHighlight"] = "Подсветка предметов по спросу клиентов",
            ["feature.SecurityInspectionHighlight"] = "Подсветка предметов для проверок безопасности",
            ["feature.GunsmithHighlight"] = "Подсветка предметов заказов оружейника",
            ["feature.AutomaticAppraisalValue"] = "Автоматическая подстановка точной оценки",
            ["feature.AppraisalDetails"] = "Сведения о подлинности, стоимости и качестве под лупой",
            ["feature.BerryQualityMarker"] = "Метки качества оценённых светящихся ягод",
            ["feature.MagnifierAugDetection"] = "Обнаружение аугментированных под лупой и причины",
            ["feature.ArrivalIdentityAlert"] = "Оповещения о приходе аугментированных и разыскиваемых клиентов",
        };
        private static readonly Dictionary<string, string> French = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening : fonctionnalités",
            ["ui.intro"] = "Chaque option s’applique immédiatement et est enregistrée avec la partie. Appuyez de nouveau sur F8 pour fermer.",
            ["ui.enableAll"] = "Tout activer",
            ["ui.disableAll"] = "Tout désactiver",
            ["ui.groupVisitors"] = "Visiteurs et téléphone",
            ["ui.groupForecasts"] = "Prévisions et surbrillance des objets",
            ["ui.groupAppraisal"] = "Expertise et identité",
            ["feature.VisitorPriority"] = "Prioriser les fournisseurs et Odin à la prochaine ouverture",
            ["feature.AppointmentDeduplication"] = "Fusionner les rendez-vous en double en conservant le plus tôt",
            ["feature.PhoneCooldownReset"] = "Effacement des délais téléphoniques en un clic",
            ["feature.PhoneDialogueText"] = "Dialogues téléphoniques à la prochaine ouverture",
            ["feature.TradeForecast"] = "Prévision des délits commerciaux et de l’enquête",
            ["feature.NightForecast"] = "Prévision nocturne des délits et de l’enquête",
            ["feature.AutomaticEvidenceCleanup"] = "Appels automatiques d’effacement des preuves",
            ["feature.CustomerDemandHighlight"] = "Surbrillance des objets demandés par les clients",
            ["feature.SecurityInspectionHighlight"] = "Surbrillance des objets de contrôle de sécurité",
            ["feature.GunsmithHighlight"] = "Surbrillance des objets de commande d’armurier",
            ["feature.AutomaticAppraisalValue"] = "Valeurs d’expertise exactes remplies automatiquement",
            ["feature.AppraisalDetails"] = "Détails d’authenticité, de valeur et de qualité à la loupe",
            ["feature.BerryQualityMarker"] = "Marqueurs de qualité des baies lumineuses expertisées",
            ["feature.MagnifierAugDetection"] = "Détection des humains augmentés à la loupe et motifs",
            ["feature.ArrivalIdentityAlert"] = "Alertes d’arrivée des clients augmentés et recherchés",
        };
        private static readonly Dictionary<string, string> Spanish = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: funciones",
            ["ui.intro"] = "Cada interruptor se aplica de inmediato y se guarda con la partida. Pulsa F8 otra vez para cerrar.",
            ["ui.enableAll"] = "Activar todo",
            ["ui.disableAll"] = "Desactivar todo",
            ["ui.groupVisitors"] = "Visitantes y teléfono",
            ["ui.groupForecasts"] = "Pronósticos y resaltado de objetos",
            ["ui.groupAppraisal"] = "Tasación e identidad",
            ["feature.VisitorPriority"] = "Priorizar a proveedores y a Odín en la próxima apertura",
            ["feature.AppointmentDeduplication"] = "Fusionar citas duplicadas y conservar la más temprana",
            ["feature.PhoneCooldownReset"] = "Restablecer de un clic los tiempos de espera del teléfono",
            ["feature.PhoneDialogueText"] = "Textos telefónicos de la próxima apertura",
            ["feature.TradeForecast"] = "Pronóstico de delitos comerciales y de la investigación",
            ["feature.NightForecast"] = "Pronóstico nocturno de delitos y de la investigación",
            ["feature.AutomaticEvidenceCleanup"] = "Llamadas automáticas de limpieza de pruebas",
            ["feature.CustomerDemandHighlight"] = "Resaltado de objetos pedidos por clientes",
            ["feature.SecurityInspectionHighlight"] = "Resaltado de objetos de inspección de seguridad",
            ["feature.GunsmithHighlight"] = "Resaltado de objetos de encargos del armero",
            ["feature.AutomaticAppraisalValue"] = "Valores de tasación exactos rellenados automáticamente",
            ["feature.AppraisalDetails"] = "Detalles de autenticidad, valor y calidad con la lupa",
            ["feature.BerryQualityMarker"] = "Marcadores de calidad de bayas luminosas tasadas",
            ["feature.MagnifierAugDetection"] = "Detección de humanos aumentados con la lupa y motivos",
            ["feature.ArrivalIdentityAlert"] = "Alertas de llegada de clientes aumentados y buscados",
        };
        private static readonly Dictionary<string, string> Polish = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: funkcje",
            ["ui.intro"] = "Każda opcja działa od razu i jest zapisywana wraz z zapisem. Naciśnij F8 ponownie, aby zamknąć.",
            ["ui.enableAll"] = "Włącz wszystko",
            ["ui.disableAll"] = "Wyłącz wszystko",
            ["ui.groupVisitors"] = "Goście i telefon",
            ["ui.groupForecasts"] = "Prognozy i podświetlenie przedmiotów",
            ["ui.groupAppraisal"] = "Wycena i tożsamość",
            ["feature.VisitorPriority"] = "Priorytet dla dostawców i Odina przy następnym otwarciu",
            ["feature.AppointmentDeduplication"] = "Scalaj zdublowane wizyty, zachowując najwcześniejszy termin",
            ["feature.PhoneCooldownReset"] = "Jednym kliknięciem usuń czas odnowienia telefonu",
            ["feature.PhoneDialogueText"] = "Teksty telefoniczne o następnym otwarciu",
            ["feature.TradeForecast"] = "Prognoza przestępstw handlowych i śledztwa",
            ["feature.NightForecast"] = "Nocna prognoza przestępstw i śledztwa",
            ["feature.AutomaticEvidenceCleanup"] = "Automatyczne połączenia czyszczenia dowodów",
            ["feature.CustomerDemandHighlight"] = "Podświetlenie przedmiotów żądanych przez klientów",
            ["feature.SecurityInspectionHighlight"] = "Podświetlenie przedmiotów kontroli bezpieczeństwa",
            ["feature.GunsmithHighlight"] = "Podświetlenie przedmiotów zleceń rusznikarza",
            ["feature.AutomaticAppraisalValue"] = "Automatyczne wpisywanie dokładnej wyceny",
            ["feature.AppraisalDetails"] = "Szczegóły autentyczności, wartości i jakości pod lupą",
            ["feature.BerryQualityMarker"] = "Znaczniki jakości wycenionych świetlistych jagód",
            ["feature.MagnifierAugDetection"] = "Wykrywanie augmentowanych pod lupą i powody",
            ["feature.ArrivalIdentityAlert"] = "Alerty przybycia klientów augmentowanych i poszukiwanych",
        };
        private static readonly Dictionary<string, string> BrazilianPortuguese = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: recursos",
            ["ui.intro"] = "Cada opção vale imediatamente e é salva com o jogo. Pressione F8 de novo para fechar.",
            ["ui.enableAll"] = "Ativar tudo",
            ["ui.disableAll"] = "Desativar tudo",
            ["ui.groupVisitors"] = "Visitantes e telefone",
            ["ui.groupForecasts"] = "Previsões e destaque de itens",
            ["ui.groupAppraisal"] = "Avaliação e identidade",
            ["feature.VisitorPriority"] = "Priorizar fornecedores e Odin na próxima abertura",
            ["feature.AppointmentDeduplication"] = "Unir compromissos duplicados, mantendo o mais cedo",
            ["feature.PhoneCooldownReset"] = "Limpar recargas do telefone com um clique",
            ["feature.PhoneDialogueText"] = "Textos de telefone da próxima abertura",
            ["feature.TradeForecast"] = "Previsão de crimes comerciais e da investigação",
            ["feature.NightForecast"] = "Previsão noturna de crimes e da investigação",
            ["feature.AutomaticEvidenceCleanup"] = "Chamadas automáticas de limpeza de provas",
            ["feature.CustomerDemandHighlight"] = "Destaque de itens pedidos pelos clientes",
            ["feature.SecurityInspectionHighlight"] = "Destaque de itens de inspeção de segurança",
            ["feature.GunsmithHighlight"] = "Destaque de itens de encomenda do armeiro",
            ["feature.AutomaticAppraisalValue"] = "Valores exatos de avaliação preenchidos automaticamente",
            ["feature.AppraisalDetails"] = "Detalhes de autenticidade, valor e qualidade na lupa",
            ["feature.BerryQualityMarker"] = "Marcadores de qualidade das bagas brilhantes avaliadas",
            ["feature.MagnifierAugDetection"] = "Detecção de humanos aprimorados na lupa e motivos",
            ["feature.ArrivalIdentityAlert"] = "Alertas de chegada de clientes aprimorados e procurados",
        };
        private static readonly Dictionary<string, string> Korean = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: 기능 설정",
            ["ui.intro"] = "각 항목은 즉시 적용되며 저장 데이터에 보관됩니다. F8을 다시 누르면 닫힙니다.",
            ["ui.enableAll"] = "모두 켜기",
            ["ui.disableAll"] = "모두 끄기",
            ["ui.groupVisitors"] = "방문객과 전화",
            ["ui.groupForecasts"] = "예측과 아이템 강조",
            ["ui.groupAppraisal"] = "감정과 신원 알림",
            ["feature.VisitorPriority"] = "다음 개점 시 공급업체와 오딘 우선",
            ["feature.AppointmentDeduplication"] = "중복 예약을 합치고 가장 이른 시간을 유지",
            ["feature.PhoneCooldownReset"] = "전화 재사용 시간 원클릭 초기화",
            ["feature.PhoneDialogueText"] = "전화 문구를 다음 개점으로 변경",
            ["feature.TradeForecast"] = "거래 사건값과 조사 진행 예측",
            ["feature.NightForecast"] = "야간 사건값과 조사 진행 예측",
            ["feature.AutomaticEvidenceCleanup"] = "조사 한도 도달 시 자동 증거 제거 전화",
            ["feature.CustomerDemandHighlight"] = "고객 수요 아이템 강조",
            ["feature.SecurityInspectionHighlight"] = "치안 검색 아이템 강조",
            ["feature.GunsmithHighlight"] = "무기 제작·개조 아이템 강조",
            ["feature.AutomaticAppraisalValue"] = "확대경에서 정확한 감정값을 자동 입력",
            ["feature.AppraisalDetails"] = "확대경 진위·가치·품질 상세",
            ["feature.BerryQualityMarker"] = "감정한 형광베리 품질 표시",
            ["feature.MagnifierAugDetection"] = "확대경 개조 인간 감지와 이유",
            ["feature.ArrivalIdentityAlert"] = "개조·수배 고객 방문 알림",
        };
        private static readonly Dictionary<string, string> Italian = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ui.title"] = "Next Opening: funzioni",
            ["ui.intro"] = "Ogni opzione si applica subito e viene salvata con la partita. Premi di nuovo F8 per chiudere.",
            ["ui.enableAll"] = "Attiva tutto",
            ["ui.disableAll"] = "Disattiva tutto",
            ["ui.groupVisitors"] = "Visitatori e telefono",
            ["ui.groupForecasts"] = "Previsioni ed evidenziazione oggetti",
            ["ui.groupAppraisal"] = "Perizia e identità",
            ["feature.VisitorPriority"] = "Dai priorità a fornitori e Odin alla prossima apertura",
            ["feature.AppointmentDeduplication"] = "Unisci gli appuntamenti duplicati, tenendo il più precoce",
            ["feature.PhoneCooldownReset"] = "Azzera i tempi di attesa del telefono con un clic",
            ["feature.PhoneDialogueText"] = "Testi telefonici della prossima apertura",
            ["feature.TradeForecast"] = "Previsione di reati commerciali e indagine",
            ["feature.NightForecast"] = "Previsione notturna di reati e indagine",
            ["feature.AutomaticEvidenceCleanup"] = "Chiamate automatiche di rimozione delle prove",
            ["feature.CustomerDemandHighlight"] = "Evidenziazione degli oggetti richiesti dai clienti",
            ["feature.SecurityInspectionHighlight"] = "Evidenziazione degli oggetti per le ispezioni di sicurezza",
            ["feature.GunsmithHighlight"] = "Evidenziazione degli oggetti degli ordini dell’armaiolo",
            ["feature.AutomaticAppraisalValue"] = "Valori di perizia esatti compilati automaticamente",
            ["feature.AppraisalDetails"] = "Dettagli di autenticità, valore e qualità con la lente",
            ["feature.BerryQualityMarker"] = "Contrassegni di qualità delle bacche luminose periziate",
            ["feature.MagnifierAugDetection"] = "Rilevamento di umani potenziati con la lente e motivi",
            ["feature.ArrivalIdentityAlert"] = "Avvisi di arrivo di clienti potenziati e ricercati",
        };

        private static readonly Dictionary<string, Dictionary<string, string>> Tables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", English },
            { "zh", Chinese },
            { "ja", Japanese },
            { "de", German },
            { "ru", Russian },
            { "fr", French },
            { "es", Spanish },
            { "pl", Polish },
            { "ptBR", BrazilianPortuguese },
            { "ko", Korean },
            { "it", Italian },
        };

        internal static IEnumerable<string> AllKeys => English.Keys;

        internal static string Get(string key, string localeCode)
        {
            string locale = ModLocalization.NormalizeLocale(localeCode);
            if (!Tables.TryGetValue(locale, out Dictionary<string, string>? table) || table == null)
                table = English;
            if (!table.TryGetValue(key, out string? text) || string.IsNullOrEmpty(text))
                text = English.TryGetValue(key, out string? fallback) ? fallback : key;
            return text;
        }

        internal static string Label(ModFeature feature, string localeCode)
        {
            return Get("feature." + feature, localeCode);
        }
    }
}

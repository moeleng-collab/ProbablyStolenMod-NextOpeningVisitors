using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class AssistantLocalization
    {
        internal const int NoAug = 0;
        internal const int AugDetected = 1;
        internal const int VisibleModifications = 2;
        internal const int Freeze = 3;
        internal const int Seizure = 4;
        internal const int SevereAnomaly = 5;
        internal const int NoVisibleClue = 6;
        internal const int KeyCount = 7;

        private static readonly string[] English =
        {
            "Inspection: no augmented human traits detected.",
            "Inspection: augmented human confirmed.",
            "Reason: visible mechanical modifications in the portrait.",
            "Reason: repeated freezing during idle movement.",
            "Reason: involuntary seizure-like idle movement.",
            "Reason: severe tier-4 augmentation anomaly.",
            "Identity confirmed by game state; no verifiable visual clue is available."
        };

        private static readonly Dictionary<string, string[]> Tables = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", English },
            { "zh", new[]
            {
                "检查结果：未检测到改造人特征。", "检查结果：确认是改造人。",
                "理由：头像有明显的机械改造特征。", "理由：待机动作会反复僵住。",
                "理由：待机动作出现非自主抽搐。", "理由：存在四级严重改造异常。",
                "身份由游戏判定确认，暂无可核实外观线索。"
            } },
            { "ja", new[]
            {
                "検査結果：強化人間の特徴は検出されませんでした。", "検査結果：強化人間と確認されました。",
                "理由：肖像に明確な機械改造が見られます。", "理由：待機動作が繰り返し硬直します。",
                "理由：待機中に不随意の痙攣が見られます。", "理由：ティア4の重度の強化異常があります。",
                "ゲーム判定では本人ですが、確認できる外見上の手がかりはありません。"
            } },
            { "de", new[]
            {
                "Prüfung: keine Merkmale eines Augmentierten erkannt.", "Prüfung: augmentierter Mensch bestätigt.",
                "Grund: sichtbare mechanische Modifikationen im Porträt.", "Grund: wiederholtes Einfrieren in Ruhebewegungen.",
                "Grund: unwillkürliche krampfartige Ruhebewegungen.", "Grund: schwere Augmentationsanomalie der Stufe 4.",
                "Identität durch den Spielzustand bestätigt; kein überprüfbarer Sichthinweis verfügbar."
            } },
            { "ru", new[]
            {
                "Проверка: признаков аугментированного человека не обнаружено.", "Проверка: аугментированный человек подтверждён.",
                "Причина: заметные механические модификации на портрете.", "Причина: повторяющееся замира́ние в состоянии покоя.",
                "Причина: непроизвольные судорожные движения в покое.", "Причина: серьёзная аномалия аугментации 4-го уровня.",
                "Личность подтверждена игрой; проверяемых визуальных признаков нет."
            } },
            { "fr", new[]
            {
                "Inspection : aucun trait d’humain augmenté détecté.", "Inspection : humain augmenté confirmé.",
                "Motif : modifications mécaniques visibles sur le portrait.", "Motif : figements répétés pendant les mouvements d’attente.",
                "Motif : mouvements involontaires de type convulsif.", "Motif : anomalie grave d’augmentation de niveau 4.",
                "Identité confirmée par le jeu ; aucun indice visuel vérifiable n’est disponible."
            } },
            { "es", new[]
            {
                "Inspección: no se detectan rasgos de humano aumentado.", "Inspección: humano aumentado confirmado.",
                "Motivo: modificaciones mecánicas visibles en el retrato.", "Motivo: congelaciones repetidas durante la espera.",
                "Motivo: movimientos involuntarios similares a convulsiones.", "Motivo: anomalía grave de aumento de nivel 4.",
                "Identidad confirmada por el juego; no hay pistas visuales verificables."
            } },
            { "pl", new[]
            {
                "Kontrola: nie wykryto cech człowieka augmentowanego.", "Kontrola: potwierdzono człowieka augmentowanego.",
                "Powód: widoczne mechaniczne modyfikacje na portrecie.", "Powód: powtarzające się zamrożenia ruchu w spoczynku.",
                "Powód: mimowolne ruchy przypominające drgawki.", "Powód: poważna anomalia augmentacji poziomu 4.",
                "Tożsamość potwierdzona przez grę; brak weryfikowalnych wizualnych wskazówek."
            } },
            { "ptBR", new[]
            {
                "Inspeção: nenhum traço de humano aprimorado detectado.", "Inspeção: humano aprimorado confirmado.",
                "Motivo: modificações mecânicas visíveis no retrato.", "Motivo: congelamentos repetidos durante o repouso.",
                "Motivo: movimentos involuntários semelhantes a convulsões.", "Motivo: anomalia grave de aprimoramento de nível 4.",
                "Identidade confirmada pelo jogo; nenhuma pista visual verificável disponível."
            } },
            { "ko", new[]
            {
                "검사 결과: 개조 인간 특징이 감지되지 않았습니다.", "검사 결과: 개조 인간으로 확인되었습니다.",
                "이유: 초상화에 기계 개조 흔적이 선명합니다.", "이유: 대기 동작이 반복적으로 멈춥니다.",
                "이유: 대기 중 비자발적 경련이 나타납니다.", "이유: 4등급의 심각한 개조 이상이 있습니다.",
                "게임 판정으로 신원은 확인되었지만 검증 가능한 외형 단서가 없습니다."
            } },
            { "it", new[]
            {
                "Controllo: nessun tratto da umano potenziato rilevato.", "Controllo: umano potenziato confermato.",
                "Motivo: modifiche meccaniche visibili nel ritratto.", "Motivo: ripetuti blocchi durante i movimenti di attesa.",
                "Motivo: movimenti involontari simili a convulsioni.", "Motivo: grave anomalia di potenziamento di livello 4.",
                "Identità confermata dal gioco; nessun indizio visivo verificabile disponibile."
            } }
        };

        internal static string Get(int key, string localeCode)
        {
            string locale = ModLocalization.NormalizeLocale(localeCode);
            if (!Tables.TryGetValue(locale, out string[]? table) || table == null) table = English;
            return key >= 0 && key < table.Length && !string.IsNullOrEmpty(table[key]) ? table[key] : English[key];
        }

        internal static int ReasonKey(AugDetectionReason reason)
        {
            switch (reason)
            {
                case AugDetectionReason.VisibleModifications: return VisibleModifications;
                case AugDetectionReason.Freeze: return Freeze;
                case AugDetectionReason.Seizure: return Seizure;
                case AugDetectionReason.SevereAnomaly: return SevereAnomaly;
                case AugDetectionReason.NoVisibleClue: return NoVisibleClue;
                default: return NoAug;
            }
        }
    }
}

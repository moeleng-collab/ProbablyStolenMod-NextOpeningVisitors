using System;

namespace NextOpeningVisitors
{
    internal static class ModLocalization
    {
        internal static string NormalizeLocale(string? localeCode)
        {
            if (string.IsNullOrEmpty(localeCode)) return "en";
            string code = localeCode!.Replace("-", "").Replace("_", "");
            if (code.Equals("zh", StringComparison.OrdinalIgnoreCase) ||
                code.Equals("zhHans", StringComparison.OrdinalIgnoreCase) ||
                code.Equals("zhHant", StringComparison.OrdinalIgnoreCase)) return "zh";
            if (code.Equals("ptBR", StringComparison.OrdinalIgnoreCase) ||
                code.Equals("pt", StringComparison.OrdinalIgnoreCase)) return "ptBR";
            if (code.Equals("ja", StringComparison.OrdinalIgnoreCase)) return "ja";
            if (code.Equals("de", StringComparison.OrdinalIgnoreCase)) return "de";
            if (code.Equals("ru", StringComparison.OrdinalIgnoreCase)) return "ru";
            if (code.Equals("fr", StringComparison.OrdinalIgnoreCase)) return "fr";
            if (code.Equals("es", StringComparison.OrdinalIgnoreCase)) return "es";
            if (code.Equals("pl", StringComparison.OrdinalIgnoreCase)) return "pl";
            if (code.Equals("ko", StringComparison.OrdinalIgnoreCase)) return "ko";
            if (code.Equals("it", StringComparison.OrdinalIgnoreCase)) return "it";
            return "en";
        }

        internal static string ResetButton(string? localeCode)
        {
            switch (NormalizeLocale(localeCode))
            {
                case "zh": return "清除冷却";
                case "ja": return "クールダウン解除";
                case "de": return "Abklingzeiten löschen";
                case "ru": return "Сбросить перезарядку";
                case "fr": return "Effacer les délais";
                case "es": return "Borrar tiempos de espera";
                case "pl": return "Usuń czas odnowienia";
                case "ptBR": return "Limpar recargas";
                case "ko": return "재사용 시간 초기화";
                case "it": return "Azzera i tempi di attesa";
                default: return "Clear Cooldowns";
            }
        }

        internal static string ResetStatus(string? localeCode, int resetCount, int skippedCount)
        {
            if (resetCount == 0 && skippedCount == 0) return NoCooldowns(localeCode);
            switch (NormalizeLocale(localeCode))
            {
                case "zh": return $"已清除 {resetCount} 个冷却，跳过 {skippedCount} 个待到访联系人";
                case "ja": return $"クールダウンを{resetCount}件解除、訪問予定の{skippedCount}件を保持";
                case "de": return $"{resetCount} Abklingzeiten gelöscht, {skippedCount} ausstehende Besuche übersprungen";
                case "ru": return $"Сброшено: {resetCount}; пропущено с ожидаемым визитом: {skippedCount}";
                case "fr": return $"{resetCount} délais effacés, {skippedCount} contacts avec visite prévue ignorés";
                case "es": return $"{resetCount} tiempos restablecidos; {skippedCount} contactos con visita pendiente omitidos";
                case "pl": return $"Usunięto: {resetCount}; pominięto kontakty z oczekującą wizytą: {skippedCount}";
                case "ptBR": return $"{resetCount} recargas limpas; {skippedCount} contatos com visita pendente ignorados";
                case "ko": return $"재사용 시간 {resetCount}개 초기화, 방문 예정 연락처 {skippedCount}개 제외";
                case "it": return $"{resetCount} tempi di attesa azzerati; {skippedCount} contatti con visita in sospeso saltati";
                default: return $"Cleared {resetCount} cooldown(s); skipped {skippedCount} contact(s) with pending visits.";
            }
        }

        internal static string ResetFailure(string? localeCode)
        {
            switch (NormalizeLocale(localeCode))
            {
                case "zh": return "清除冷却失败，请查看日志";
                case "ja": return "解除に失敗しました。ログを確認してください";
                case "de": return "Löschen fehlgeschlagen. Bitte Protokoll prüfen";
                case "ru": return "Не удалось сбросить перезарядку. Проверьте журнал";
                case "fr": return "Échec de l’effacement. Consultez le journal";
                case "es": return "Error al restablecer. Consulta el registro";
                case "pl": return "Nie udało się usunąć. Sprawdź dziennik";
                case "ptBR": return "Falha ao limpar. Consulte o registro";
                case "ko": return "초기화에 실패했습니다. 로그를 확인하세요";
                case "it": return "Azzeramento non riuscito. Controlla il registro";
                default: return "Failed to clear cooldowns. Check the log.";
            }
        }

        private static string NoCooldowns(string? localeCode)
        {
            switch (NormalizeLocale(localeCode))
            {
                case "zh": return "当前没有需要清除的冷却";
                case "ja": return "解除が必要なクールダウンはありません";
                case "de": return "Keine Abklingzeiten zu löschen";
                case "ru": return "Нет перезарядки для сброса";
                case "fr": return "Aucun délai à effacer";
                case "es": return "No hay tiempos de espera que restablecer";
                case "pl": return "Brak czasu odnowienia do usunięcia";
                case "ptBR": return "Nenhuma recarga para limpar";
                case "ko": return "초기화할 재사용 시간이 없습니다";
                case "it": return "Nessun tempo di attesa da azzerare";
                default: return "No cooldowns need clearing.";
            }
        }
    }
}

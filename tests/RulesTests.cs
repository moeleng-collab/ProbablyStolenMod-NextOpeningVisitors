using System;
using System.Collections.Generic;
using NextOpeningVisitors;

internal static class RulesTests
{
    private static int _assertions;

    private static void Main()
    {
        TestAllSupportedVisitors();
        TestUnrelatedVisitorsUntouched();
        TestBeforeOpeningAndAfterClosingBothUseNextOpening();
        TestStablePriorityOrder();
        TestEmptyQueue();
        TestMismatchedQueuesRejected();
        TestCooldownReset();
        TestPhoneDialogueText();
        TestInvestigationThresholds();
        TestTradeFactionsAndCrimeTypes();
        TestTradePerksAndReadOnlyCases();
        TestNightFixerAndDecay();
        TestNightPerkOrderAndCap();
        TestAutomaticOfferDailyRules();
        TestAutomaticOfferPersistence();
        TestAutomaticAcceptanceRules();
        TestForecastLocalization();
        Console.WriteLine($"PASS: {_assertions} assertions; visitor, forecast, automatic offer and localization rules verified.");
    }

    private static void TestAllSupportedVisitors()
    {
        string[] ids = { "courrierRedImp", "courrierRedImpFree", "courrierChem", "courrierChemFree", "courrierMining", "courrierMiningFree", "courrierPharma", "courrierPharmaFree", "wanted4Normal" };
        foreach (string id in ids) Assert(VisitorRules.IsExpedited(id), $"target: {id}");
        Assert(!VisitorRules.IsExpedited("wanted4"), "story variant is not Odin's phone order");
        Assert(!VisitorRules.IsExpedited(null), "null is ignored");
    }

    private static void TestUnrelatedVisitorsUntouched()
    {
        string[] ids = { "landlord", "ordinary", "story" };
        int[] days = { 2, 0, 4 };
        int changed = VisitorRules.NormalizeDelays(ids, days);
        Assert(changed == 0, "unrelated schedules unchanged count");
        Assert(days[0] == 2 && days[1] == 0 && days[2] == 4, "unrelated schedules unchanged values");
    }

    private static void TestBeforeOpeningAndAfterClosingBothUseNextOpening()
    {
        string[] ids = { "courrierRedImp", "courrierMining", "wanted4Normal", "ordinary" };
        int[] days = { 3, 2, 2, 5 };
        int changed = VisitorRules.NormalizeDelays(ids, days);
        Assert(changed == 3, "three target schedules normalized");
        for (int i = 0; i < 3; i++)
        {
            days[i]--;
            Assert(days[i] == 0, $"arrives when opening handles queue: {ids[i]}");
        }
        Assert(days[3] == 5, "other visitor keeps original delay");
        Assert(VisitorRules.NormalizeDelays(ids, new[] { 1, 1, 1, 5 }) == 0, "already normalized is idempotent");
    }

    private static void TestStablePriorityOrder()
    {
        var queue = new List<string> { "ordinaryA", "courrierRedImp", "ordinaryB", "wanted4Normal", "courrierChem" };
        int count = VisitorRules.Prioritize(queue, value => value);
        Assert(count == 3, "three visitors prioritized");
        Assert(string.Join(",", queue) == "courrierRedImp,wanted4Normal,courrierChem,ordinaryA,ordinaryB", "stable target and non-target order");
        VisitorRules.Prioritize(queue, value => value);
        Assert(string.Join(",", queue) == "courrierRedImp,wanted4Normal,courrierChem,ordinaryA,ordinaryB", "priority is idempotent");
    }

    private static void TestEmptyQueue()
    {
        var queue = new List<string>();
        Assert(VisitorRules.Prioritize(queue, value => value) == 0, "empty queue ignored");
    }

    private static void TestMismatchedQueuesRejected()
    {
        bool threw = false;
        try { VisitorRules.NormalizeDelays(new[] { "courrierRedImp" }, Array.Empty<int>()); }
        catch (ArgumentException) { threw = true; }
        Assert(threw, "mismatched parallel queues rejected");
    }

    private static void TestCooldownReset()
    {
        var contacts = new Dictionary<long, FakeContact>
        {
            { StorePhoneClient.RED_IMP_REP_NUMBER, new FakeContact { CoolingDown = true } },
            { StorePhoneClient.MINING_NUMBER, new FakeContact { CoolingDown = false } },
            { StorePhoneClient.REV_MERCHANT_NUMBER, new FakeContact { CoolingDown = true } },
            { StorePhoneClient.WILD_CLIENT_NUMBER, new FakeContact { CoolingDown = true } }
        };
        var pending = new List<string> { "courrierRedImp" };
        CooldownResetResult result = CooldownResetRules.ResetCooldowns(
            contacts,
            pending,
            contact => contact.CoolingDown,
            contact => { contact.CoolingDown = false; contact.ResetCount++; });
        Assert(result.ResetCount == 1 && result.SkippedCount == 1, "pending supplier skipped while Odin resets");
        Assert(contacts[StorePhoneClient.RED_IMP_REP_NUMBER].CoolingDown, "pending supplier keeps cooldown");
        Assert(contacts[StorePhoneClient.RED_IMP_REP_NUMBER].ResetCount == 0, "pending supplier is not reset");
        Assert(!contacts[StorePhoneClient.REV_MERCHANT_NUMBER].CoolingDown, "Odin without pending appointment resets");
        Assert(contacts[StorePhoneClient.MINING_NUMBER].ResetCount == 0, "ready supplier untouched");
        Assert(contacts[StorePhoneClient.WILD_CLIENT_NUMBER].CoolingDown, "unrelated contact untouched");
        Assert(CooldownResetRules.HasPendingAppointment(StorePhoneClient.RED_IMP_REP_NUMBER, pending), "regular supplier appointment detected");
        Assert(!CooldownResetRules.HasPendingAppointment(StorePhoneClient.MINING_NUMBER, pending), "other supplier has no pending appointment");

        pending[0] = "courrierRedImpFree";
        Assert(CooldownResetRules.HasPendingAppointment(StorePhoneClient.RED_IMP_REP_NUMBER, pending), "intro appointment belongs to same supplier");
        pending.Clear();
        result = CooldownResetRules.ResetCooldowns(contacts, pending, contact => contact.CoolingDown, contact => { contact.CoolingDown = false; contact.ResetCount++; });
        Assert(result.ResetCount == 1 && result.SkippedCount == 0, "supplier resets after appointment is gone");
        result = CooldownResetRules.ResetCooldowns(contacts, pending, contact => contact.CoolingDown, contact => contact.ResetCount++);
        Assert(result.ResetCount == 0, "second reset is idempotent for targets");
        Assert(CooldownResetRules.IsResetTarget(StorePhoneClient.CHEM_NUMBER), "chemist is a reset target");
        Assert(!CooldownResetRules.IsResetTarget(StorePhoneClient.WILD_CLIENT_NUMBER), "Wild is not a reset target");

        var ids = new List<string> { "courrierRedImp", "ordinary", "courrierRedImpFree", "wanted4Normal", "courrierRedImp", "ordinaryB", "wanted4Normal" };
        var days = new List<int> { 2, 7, 1, 2, 1, 3, 1 };
        int removed = VisitorRules.RemoveDuplicateAppointments(ids, days);
        Assert(removed == 3, "all duplicate target appointments removed");
        Assert(string.Join(",", ids) == "courrierRedImp,ordinary,wanted4Normal,ordinaryB", "earliest appointments and unrelated visitors retained");
        Assert(string.Join(",", days) == "2,7,2,3", "parallel delays stay aligned after deduplication");
    }

    private static void TestPhoneDialogueText()
    {
        string[] keys =
        {
            "red_imp_order1", "red_imp_order2", "chem_order1", "chem_order2",
            "mining_order1", "mining_order2", "pharma_order1", "pharma_order2",
            "red_imp_rep4", "ashish_ready", "caulson_ready", "dale_ready",
            "red_imp_rep_intro4", "red_imp_rep_intro6", "ashish_intro4", "ashish_intro6",
            "caulson_intro5", "caulson_intro7", "dale_intro6", "dale_intro9", "odin_dropby"
        };
        string[] locales = { "en", "zh", "ja", "de", "ru", "fr", "es", "pl", "ptBR", "ko", "it" };
        foreach (string locale in locales)
        {
            foreach (string key in keys)
            {
                Assert(PhoneDialogueTextRules.TryRewrite(key, new object[] { 7, 2 }, locale, out string? localized),
                    $"Text mapped for {locale}: {key}");
                Assert(!string.IsNullOrWhiteSpace(localized), $"Text is nonempty for {locale}: {key}");
            }
            Assert(!string.IsNullOrWhiteSpace(ModLocalization.ResetButton(locale)), "Reset button localized: " + locale);
            string status = ModLocalization.ResetStatus(locale, 2, 3);
            Assert(status.Contains("2") && status.Contains("3"), "Reset status keeps counts: " + locale);
            Assert(!string.IsNullOrWhiteSpace(ModLocalization.ResetStatus(locale, 0, 0)), "Empty status localized: " + locale);
            Assert(!string.IsNullOrWhiteSpace(ModLocalization.ResetFailure(locale)), "Failure status localized: " + locale);
        }
        Assert(ModLocalization.NormalizeLocale("pt-BR") == "ptBR", "Brazilian Portuguese aliases normalized");
        Assert(ModLocalization.NormalizeLocale("zh-Hans") == "zh", "Chinese aliases normalized");

        PhoneDialogueTextRules.TryRewrite("red_imp_order1", new object[] { 8, 1 }, "zh", out string? zhOrder);
        Assert(zhOrder == "[订购8份啤酒（下次开店送达）]", "Chinese order keeps quantity and removes day delay");
        PhoneDialogueTextRules.TryRewrite("pharma_order2", new object[] { 9, 2 }, "en", out string? enOrder);
        Assert(enOrder == "[Order 9 Oxymore Injectors (Next Opening Delivery)]", "English order keeps quantity and removes day delay");
        PhoneDialogueTextRules.TryRewrite("red_imp_rep_intro6", null, "zh", out string? zhIntro);
        Assert(zhIntro != null && zhIntro.Contains("下次开店") && !zhIntro.Contains("明天"), "intro no longer promises tomorrow");
        PhoneDialogueTextRules.TryRewrite("odin_dropby", new object[] { 2 }, "en", out string? enOdin);
        Assert(enOdin == "I'll visit when you next open the shop.", "Odin text matches next opening");
        PhoneDialogueTextRules.TryRewrite("odin_dropby", new object[] { 2 }, "nl", out string? fallback);
        Assert(fallback == enOdin, "unsupported locale uses correct English fallback");
        Assert(!PhoneDialogueTextRules.TryRewrite("red_imp_order1", null, "zh", out _), "missing order quantity leaves original text untouched");
        Assert(!PhoneDialogueTextRules.TryRewrite("goodbye", null, "zh", out _), "unrelated phone text untouched");
        Assert(!PhoneDialogueTextRules.TryRewrite("red_imp_out2", new object[] { 4 }, "zh", out _), "cooldown text untouched");
    }

    private static void TestInvestigationThresholds()
    {
        (int value, int delta)[] cases =
        {
            (99, 0), (100, 2), (299, 2), (300, 6), (849, 6), (850, 9),
            (1199, 9), (1200, 12), (1499, 12), (1500, 20), (2499, 20), (2500, 33)
        };
        foreach ((int value, int delta) test in cases)
            Assert(CrimeForecastRules.GetBaseInvestigationDelta(test.value) == test.delta,
                $"Investigation threshold {test.value} => {test.delta}");
    }

    private static void TestTradeFactionsAndCrimeTypes()
    {
        (string faction, bool tightLips, int expected)[] cases =
        {
            ("FACTION_BLACK_MARKET", false, 60),
            ("FACTION_REVOLUTION", false, 40),
            ("FACTION_LOWER_LEVEL", false, 120),
            ("FACTION_LOWER_LEVEL", true, 60),
            ("FACTION_CARTEL", false, 0),
            ("FACTION_OTHER", false, 120)
        };
        foreach ((string faction, bool tightLips, int expected) test in cases)
        {
            TradeForecastResult result = Trade(new[]
            {
                new TradeItemForecastInput
                {
                    NegotiatedValue = 120,
                    ContrabandKind = TradeContrabandKind.Weapon
                }
            }, test.faction, test.tightLips);
            Assert(result.AddedCrimeTotal == test.expected, $"Faction modifier: {test.faction}, tightLips={test.tightLips}");
            Assert(result.CrimeIncreases[SecData.WEAPON_TRAFFICKING] == test.expected, "Weapon crime ID retained");
        }

        TradeForecastResult dual = Trade(new[]
        {
            new TradeItemForecastInput
            {
                NegotiatedValue = 200,
                IsStolen = true,
                StolenHeat = 50,
                ContrabandKind = TradeContrabandKind.Substance
            }
        });
        Assert(dual.CrimeIncreases[SecData.FENCING] == 100, "Stolen heat applied before faction and perks");
        Assert(dual.CrimeIncreases[SecData.DRUG_TRAFFICKING] == 200, "Contraband uses full negotiated value");
        Assert(dual.AddedCrimeTotal == 300, "Stolen and contraband labels both contribute");

        TradeForecastResult access = Trade(new[]
        {
            new TradeItemForecastInput { NegotiatedValue = 30, ContrabandKind = TradeContrabandKind.AccessCard },
            new TradeItemForecastInput { NegotiatedValue = 20, ContrabandKind = TradeContrabandKind.Other }
        });
        Assert(access.CrimeIncreases[SecData.CARD_TRAFFICKING] == 30, "Access card crime ID retained");
        Assert(access.CrimeIncreases[SecData.CONTRABAND_TRAFFICKING] == 20, "Other contraband crime ID retained");
    }

    private static void TestTradePerksAndReadOnlyCases()
    {
        TradeForecastResult perks = Trade(new[]
        {
            new TradeItemForecastInput { NegotiatedValue = 5, ContrabandKind = TradeContrabandKind.Other }
        }, intimidating: true, blabbermouth: true);
        Assert(perks.AddedCrimeTotal == 4, "Intimidating then Blabbermouth rounding matches CommitCrime");
        Assert(CrimeForecastRules.MultiplyAndRound(5, 1.5) == 8, "7.5 uses banker's rounding to 8");
        Assert(CrimeForecastRules.MultiplyAndRound(3, 1.5) == 4, "4.5 uses banker's rounding to 4");
        Assert(CrimeForecastRules.MultiplyAndRound(1, 0.5) == 0, "0.5 uses banker's rounding to 0");

        var legalInput = new TradeForecastInput { EvidenceLevel = 42 };
        legalInput.CurrentCrimeAmounts.Add(100);
        legalInput.Items.Add(new TradeItemForecastInput { NegotiatedValue = 500 });
        TradeForecastResult legal = CrimeForecastRules.ForecastTrade(legalInput);
        Assert(legal.AddedCrimeTotal == 0 && legal.ResultingCrimeTotal == 100, "Legal item predicts +0");
        Assert(legal.EvidenceLevel == 42 && legalInput.CurrentCrimeAmounts[0] == 100 && legalInput.Items.Count == 1,
            "Trade forecast does not mutate its input");

        var excluded = new TradeForecastInput { ClientDoesNotContributeToEvidence = true };
        excluded.Items.Add(new TradeItemForecastInput
        {
            NegotiatedValue = 900,
            IsStolen = true,
            StolenHeat = 100,
            ContrabandKind = TradeContrabandKind.Weapon
        });
        Assert(CrimeForecastRules.ForecastTrade(excluded).AddedCrimeTotal == 0,
            "No-evidence client zeroes all contributions");
    }

    private static void TestNightFixerAndDecay()
    {
        (int tier, int expected)[] tiers = { (1, 800), (2, 650), (3, 350) };
        foreach ((int tier, int expected) test in tiers)
        {
            NightForecastResult result = Night(new[] { 1000 }, evidence: 90, useFixer: true, fixerTier: test.tier);
            Assert(result.AfterFixerCrimeTotal == test.expected, $"Fixer tier {test.tier} percentage then flat fee");
            Assert(result.AfterDecayCrimeTotal == test.expected, "No decay at evidence 80 or above");
        }

        NightForecastResult below = Night(new[] { 100, 10, 0 }, evidence: 79);
        Assert(below.AfterDecayCrimeTotal == 80 && below.AfterDecayAmounts[0] == 80 && below.AfterDecayAmounts[1] == 0,
            "Below 80 decays every crime by 20 and clamps at zero");
        Assert(below.EvidenceDelta == 0 && below.NextEvidenceLevel == 79, "Decayed total below 100 adds no investigation");

        NightForecastResult atBoundary = Night(new[] { 100, 10, 0 }, evidence: 80);
        Assert(atBoundary.AfterDecayCrimeTotal == 110 && atBoundary.EvidenceDelta == 2 && atBoundary.NextEvidenceLevel == 82,
            "Evidence 80 does not decay and uses the resulting total");

        NightForecastResult clamped = Night(new[] { 10 }, evidence: 90, useFixer: true, fixerTier: 3);
        Assert(clamped.AfterFixerCrimeTotal == 0 && clamped.AfterDecayCrimeTotal == 0,
            "Fixer and decay never create negative crime values");
    }

    private static void TestNightPerkOrderAndCap()
    {
        NightForecastResult result = Night(new[] { 300 }, evidence: 95,
            secTrusted: true, secPriority: true, secWatchlist: true, ulPetition: true);
        Assert(result.BaseEvidenceDelta == 6 && result.EvidenceDelta == 6,
            "Trusted halves, priority wins over watchlist, then petition multiplies with banker's rounding");
        Assert(result.NextEvidenceLevel == 100 && result.ReachesInvestigationCap,
            "Forecast warns when the night reaches 100");

        NightForecastResult watchlist = Night(new[] { 1500 }, evidence: 80,
            secTrusted: true, secWatchlist: true, ulPetition: true);
        Assert(watchlist.BaseEvidenceDelta == 20 && watchlist.EvidenceDelta == 18,
            "Watchlist is applied after trusted and before petition");
        Assert(!watchlist.ReachesInvestigationCap && watchlist.NextEvidenceLevel == 98,
            "Below-cap forecast remains unclamped");
    }

    private static void TestAutomaticOfferDailyRules()
    {
        var state = new AutoEvidenceCallState();
        Assert(!state.BeginDay(1, 99, 1000) && !state.HasPendingOffer, "Below 100 does not create an offer");
        Assert(state.BeginDay(1, 100, 1000), "Investigation 100 creates the initial offer");
        Assert(state.OfferPrice == 1000 && state.OfferDay == 1 && state.LastOfferDay == 1, "Initial price and day locked");
        string first = state.Serialize();
        Assert(!state.BeginDay(1, 100, 1200) && state.Serialize() == first, "Same-day offer generation is idempotent");
        Assert(state.Reject(1) && !state.HasPendingOffer && state.LastOfferDay == 1, "Reject clears pending but keeps daily marker");
        Assert(!state.BeginDay(1, 100, 1000), "Rejected offer does not return on the same day");
        Assert(state.BeginDay(2, 100, 1000) && state.OfferDay == 2, "Rejected offer can return next day");

        state.CompleteAcceptance();
        Assert(!state.HasPendingOffer && !state.BeginDay(2, 100, 1200), "Accepted offer cannot be generated twice in one day");
        Assert(state.BeginDay(3, 100, 1200) && state.OfferPrice == 1200, "Accepted cleanup uses increased price next day");
        state.BeginDay(4, 99, 1440);
        Assert(!state.HasPendingOffer && state.LastOfferDay == 3, "Below-100 day cancels stale pending without a new offer");
    }

    private static void TestAutomaticOfferPersistence()
    {
        var state = new AutoEvidenceCallState();
        state.BeginDay(7, 100, 1200);
        string saved = state.Serialize();
        Assert(AutoEvidenceCallState.TryDeserialize(saved, out AutoEvidenceCallState? restored), "Pending state deserializes");
        Assert(restored.HasPendingOffer && restored.OfferDay == 7 && restored.OfferPrice == 1200 && restored.LastOfferDay == 7,
            "Pending price and day survive save/load");
        Assert(!restored.BeginDay(7, 100, 9999) && restored.OfferPrice == 1200,
            "Loading alone does not regenerate or reprice an existing offer");
        Assert(restored.BeginDay(8, 100, 1200) && restored.OfferDay == 8,
            "Cross-day pending offer expires before the new daily offer");
        Assert(!AutoEvidenceCallState.TryDeserialize("broken", out AutoEvidenceCallState? fallback) && !fallback.HasPendingOffer,
            "Malformed state falls back safely");
    }

    private static void TestAutomaticAcceptanceRules()
    {
        var state = new AutoEvidenceCallState();
        state.BeginDay(3, 100, 1000);
        Assert(AutoEvidenceCallRules.ValidateAcceptance(state, 3, 100, 1000) == AutoEvidenceAcceptanceStatus.Valid,
            "Exact cash is enough");
        Assert(AutoEvidenceCallRules.ValidateAcceptance(state, 3, 100, 999) == AutoEvidenceAcceptanceStatus.InsufficientCash,
            "Insufficient cash is rejected before mutation");
        Assert(state.HasPendingOffer && state.OfferPrice == 1000, "Insufficient cash keeps offer and price unchanged");
        Assert(AutoEvidenceCallRules.ValidateAcceptance(state, 3, 99, 1000) == AutoEvidenceAcceptanceStatus.InvestigationAlreadyClear,
            "Already-cleared investigation cannot be purchased");
        Assert(AutoEvidenceCallRules.ValidateAcceptance(state, 4, 100, 1000) == AutoEvidenceAcceptanceStatus.StaleOffer,
            "Cross-day acceptance is stale");

        state.CompleteAcceptance();
        Assert(AutoEvidenceCallRules.ValidateAcceptance(state, 3, 100, 1000) == AutoEvidenceAcceptanceStatus.NoPendingOffer,
            "Repeated acceptance succeeds only once");
        Assert(AutoEvidenceCallRules.GetNextPrice(1000) == 1200 && AutoEvidenceCallRules.GetNextPrice(1200) == 1440,
            "Accepted cleanup increases price by Math.Round(price * 1.2)");
        AssertThrows<OverflowException>(() => AutoEvidenceCallRules.GetNextPrice(int.MaxValue), "Price overflow rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => AutoEvidenceCallRules.GetNextPrice(0), "Invalid price rejected");
    }

    private static void TestForecastLocalization()
    {
        string[] locales = { "en", "zh", "ja", "de", "ru", "fr", "es", "pl", "ptBR", "ko", "it" };
        foreach (string locale in locales)
        {
            for (int key = 0; key < CrimeForecastLocalization.KeyCount; key++)
            {
                string text = CrimeForecastLocalization.Get(key, locale, 1, 2, 3);
                Assert(!string.IsNullOrWhiteSpace(text), $"Forecast text {key} localized for {locale}");
            }
        }
        string fallback = CrimeForecastLocalization.Get(CrimeForecastLocalization.CallOffer, "nl", 1000);
        Assert(fallback == CrimeForecastLocalization.Get(CrimeForecastLocalization.CallOffer, "en", 1000),
            "Unsupported forecast locale uses English");
    }

    private static TradeForecastResult Trade(TradeItemForecastInput[] items, string faction = "",
        bool tightLips = false, bool intimidating = false, bool blabbermouth = false)
    {
        var input = new TradeForecastInput
        {
            ClientFaction = faction,
            LowerLevelTightLips = tightLips,
            Intimidating = intimidating,
            Blabbermouth = blabbermouth
        };
        foreach (TradeItemForecastInput item in items) input.Items.Add(item);
        return CrimeForecastRules.ForecastTrade(input);
    }

    private static NightForecastResult Night(int[] amounts, int evidence, bool useFixer = false, int fixerTier = 1,
        bool secTrusted = false, bool secPriority = false, bool secWatchlist = false, bool ulPetition = false)
    {
        var input = new NightForecastInput
        {
            EvidenceLevel = evidence,
            UseFixer = useFixer,
            FixerTier = fixerTier,
            SecTrusted = secTrusted,
            SecPriority = secPriority,
            SecWatchlist = secWatchlist,
            UlPetition = ulPetition
        };
        foreach (int amount in amounts) input.CurrentCrimeAmounts.Add(amount);
        return CrimeForecastRules.ForecastNight(input);
    }

    private static void AssertThrows<T>(Action action, string name) where T : Exception
    {
        bool threw = false;
        try { action(); }
        catch (T) { threw = true; }
        Assert(threw, name);
    }

    private sealed class FakeContact
    {
        internal bool CoolingDown;
        internal int ResetCount;
    }

    private static void Assert(bool condition, string name)
    {
        _assertions++;
        if (!condition) throw new InvalidOperationException("FAIL: " + name);
    }
}

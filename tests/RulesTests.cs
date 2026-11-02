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
        TestDemandHighlightRules();
        TestAppraisalAssistantRules();
        TestAssistantLocalization();
        Console.WriteLine($"PASS: {_assertions} assertions; visitor, forecast, automatic offer, highlighting, appraisal and localization rules verified.");
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
        string intro = CrimeForecastLocalization.Get(CrimeForecastLocalization.CallOffer, "zh", 1000);
        string price = CrimeForecastLocalization.Get(CrimeForecastLocalization.CallOfferPrice, "zh", 1000);
        string terms = CrimeForecastLocalization.Get(CrimeForecastLocalization.CallOfferTerms, "zh", 1000);
        Assert(intro != price && price != terms && intro != terms, "Automatic call body has three distinct pages");
        Assert(!intro.Contains("1000") && price.Contains("1000") && !terms.Contains("1000"),
            "Automatic call price appears only on the price page");
    }

    private static void TestDemandHighlightRules()
    {
        Assert(DemandHighlightRules.Classify(false, true, true) == DemandHighlightLevel.None,
            "Unmatched flagged item is not highlighted");
        Assert(DemandHighlightRules.Classify(true, false, false) == DemandHighlightLevel.Demand,
            "Ordinary demand is green");
        Assert(DemandHighlightRules.Classify(true, true, false) == DemandHighlightLevel.Stolen,
            "Stolen demand is amber");
        Assert(DemandHighlightRules.Classify(true, false, true) == DemandHighlightLevel.Contraband,
            "Contraband demand is red");
        Assert(DemandHighlightRules.Classify(true, true, true) == DemandHighlightLevel.Contraband,
            "Contraband wins over stolen");
        Assert(DemandHighlightRules.Merge(DemandHighlightLevel.Stolen, DemandHighlightLevel.Demand) == DemandHighlightLevel.Stolen,
            "Nested stolen risk overrides ordinary demand");
        Assert(DemandHighlightRules.Merge(DemandHighlightLevel.Contraband, DemandHighlightLevel.Stolen) == DemandHighlightLevel.Contraband,
            "Nested contraband has highest priority");
        Assert(DemandHighlightRules.Merge(DemandHighlightLevel.None, DemandHighlightLevel.Demand) == DemandHighlightLevel.Demand,
            "Container inherits a matching child");
        Assert(DemandHighlightRules.CanDescend(false, false, false, false), "Unlocked demand container can be searched");
        Assert(!DemandHighlightRules.CanDescend(false, true, false, false), "Item behind a lock is not traversed");
        Assert(!DemandHighlightRules.CanDescend(false, false, true, false), "Locked demand container stops recursion");
        Assert(DemandHighlightRules.CanDescend(false, false, false, true), "Search blocker does not hide ordinary demand");
        Assert(!DemandHighlightRules.CanDescend(true, false, false, true), "Inspection search blocker stops recursion");
        Assert(DemandHighlightRules.CanDescend(true, false, false, false), "Visible inspection container can be searched");
        Assert(DemandHighlightRules.ShouldInspectTrash(true, false), "Normal inspection includes the trashcan root");
        Assert(!DemandHighlightRules.ShouldInspectTrash(true, true), "Lazy inspector skips the trashcan like vanilla");
        Assert(!DemandHighlightRules.ShouldInspectTrash(false, false), "Demand mode does not treat trash as a sales inventory");
        Assert(DemandHighlightRules.IsGunsmithOrder(true, true),
            "A gunsmith order enables weapon demand highlighting");
        Assert(!DemandHighlightRules.IsGunsmithOrder(true, false),
            "A gunsmith client without order data does not scan weapons");
        Assert(!DemandHighlightRules.IsGunsmithOrder(false, true),
            "Ordinary purchases do not use the gunsmith branch");
        Assert(DemandHighlightRules.MatchesGunsmithWeapon(false, true, true),
            "A normal custom order highlights an advanced gun with accepted caliber");
        Assert(!DemandHighlightRules.MatchesGunsmithWeapon(false, true, false),
            "A fixed caliber requirement rejects other calibers");
        Assert(!DemandHighlightRules.MatchesGunsmithWeapon(false, false, true),
            "Gunsmith orders do not highlight non-gun items");
        Assert(!DemandHighlightRules.MatchesGunsmithWeapon(true, true, true),
            "Service orders do not highlight unrelated player guns");
        Assert(DemandHighlightRules.IsGunsmithRequirementPending(false),
            "An unmet order requirement keeps its related parts highlighted");
        Assert(!DemandHighlightRules.IsGunsmithRequirementPending(true),
            "A met order requirement stops highlighting its related parts");
        Assert(DemandHighlightRules.MatchesGunsmithStatPart(true, true, 5, 0),
            "Positive percentage stats satisfy a higher-stat request");
        Assert(DemandHighlightRules.MatchesGunsmithStatPart(true, true, 0, 2),
            "Positive flat stats satisfy a higher-stat request");
        Assert(!DemandHighlightRules.MatchesGunsmithStatPart(true, true, -5, 0),
            "Negative stats do not satisfy a higher-stat request");
        Assert(DemandHighlightRules.MatchesGunsmithStatPart(true, false, -5, 0),
            "Negative percentage stats satisfy a lower-stat request");
        Assert(DemandHighlightRules.MatchesGunsmithStatPart(true, false, 0, -2),
            "Negative flat stats satisfy a lower-stat request");
        Assert(!DemandHighlightRules.MatchesGunsmithStatPart(true, false, 5, 0),
            "Positive stats do not satisfy a lower-stat request");
        Assert(!DemandHighlightRules.MatchesGunsmithStatPart(false, true, 5, 0),
            "Non-mod items never use the weapon-part branch");
        Assert(DemandHighlightRules.MatchesGunsmithSizePart(true, true),
            "Weapon mods contribute to a larger-size request");
        Assert(!DemandHighlightRules.MatchesGunsmithSizePart(true, false),
            "Weapon mods do not contribute to a smaller-size request");
        Assert(DemandHighlightRules.MatchesGunsmithSuppressor(true, true, 2, 1),
            "A proper suppressor satisfies a proper-suppressor request");
        Assert(!DemandHighlightRules.MatchesGunsmithSuppressor(true, true, 1, 1),
            "A makeshift suppressor does not satisfy a proper-suppressor request");
        Assert(!DemandHighlightRules.MatchesGunsmithSuppressor(true, false, 2, 0),
            "Non-suppressor mods do not satisfy a suppressor request");
        Assert(DemandHighlightRules.MaximumDepth == 32, "Recursive traversal has a fixed depth limit");
        Assert(DemandHighlightRules.BorderWidth(48) == 3f, "Border uses the thinner grid-relative width");
        Assert(DemandHighlightRules.BorderWidth(64) == 4f, "Border scales with the native grid");
        Assert(DemandHighlightRules.BorderWidth(8) == 1f, "Border never becomes thinner than one unit");
    }

    private static void TestAppraisalAssistantRules()
    {
        Assert(AppraisalAssistantRules.CanAutoFill(true, true, 125), "Valid appraisal service value can be filled");
        Assert(!AppraisalAssistantRules.CanAutoFill(false, true, 125), "Non-appraisal customer is untouched");
        Assert(!AppraisalAssistantRules.CanAutoFill(true, false, 125), "Non-appraisal item is untouched");
        Assert(!AppraisalAssistantRules.CanAutoFill(true, true, 0), "Missing correct value preserves original estimate");
        Assert(!AppraisalAssistantRules.CanAutoFill(true, true, -5), "Negative correct value preserves original estimate");

        Assert(AppraisalAssistantRules.DetectAug(false, true, true, true, 4) == AugDetectionReason.None,
            "Non-augmented customer never gets an aug reason");
        Assert(AppraisalAssistantRules.DetectAug(true, true, true, false, 2) == AugDetectionReason.VisibleModifications,
            "Visible portrait modifications are reported first");
        Assert(AppraisalAssistantRules.DetectAug(true, false, true, false, 2) == AugDetectionReason.Freeze,
            "Freeze tell is a detection reason");
        Assert(AppraisalAssistantRules.DetectAug(true, false, false, true, 2) == AugDetectionReason.Seizure,
            "Seizure tell is a detection reason");
        Assert(AppraisalAssistantRules.DetectAug(true, false, false, false, 4) == AugDetectionReason.SevereAnomaly,
            "Tier-four anomaly is a detection reason");
        Assert(AppraisalAssistantRules.DetectAug(true, false, false, false, 1) == AugDetectionReason.NoVisibleClue,
            "Disguised aug does not invent a visible clue");
        Assert(AppraisalAssistantRules.HasVisibleAugSprite("BM_Aug_01"), "Aug portrait is recognized case-insensitively");
        Assert(AppraisalAssistantRules.HasVisibleAugSprite("cyber_lower_2"), "Cyber portrait is recognized");
        Assert(AppraisalAssistantRules.HasVisibleAugSprite("android_upper_1"), "Android portrait is recognized");
        Assert(!AppraisalAssistantRules.HasVisibleAugSprite("ordinary"), "Ordinary portrait is not an aug clue");
        Assert(!AppraisalAssistantRules.HasVisibleAugSprite("laughing"), "Incidental aug letters are not an aug clue");
        Assert(!AppraisalAssistantRules.HasVisibleAugSprite(null), "Missing portrait is not an aug clue");

        Assert(AppraisalAssistantRules.ShouldAlertArrival(true, true, false), "Aug alert appears on arrival");
        Assert(AppraisalAssistantRules.ShouldAlertArrival(true, false, true), "Wanted alert appears on arrival");
        Assert(AppraisalAssistantRules.ShouldAlertArrival(true, true, true), "Combined customer alert appears on arrival");
        Assert(!AppraisalAssistantRules.ShouldAlertArrival(false, true, true), "No arrival alert before the customer arrives");
        Assert(!AppraisalAssistantRules.ShouldAlertArrival(true, false, false), "Ordinary customer has no arrival alert");

        Assert(AppraisalAssistantRules.GetBerryQuality(-2, 0) == BerryQuality.Moldy, "Berry mold mapping");
        Assert(AppraisalAssistantRules.GetBerryQuality(2, null) == BerryQuality.Small, "Small berry mapping");
        Assert(AppraisalAssistantRules.GetBerryQuality(0, 0) == BerryQuality.Normal, "Normal berry mapping");
        Assert(AppraisalAssistantRules.GetBerryQuality(1, 2) == BerryQuality.Normal, "Second normal berry mapping");
        Assert(AppraisalAssistantRules.GetBerryQuality(-1, 0) == BerryQuality.Unripe, "First unripe berry variant");
        Assert(AppraisalAssistantRules.GetBerryQuality(-1, 1) == BerryQuality.Unripe, "Second unripe berry variant");
        Assert(AppraisalAssistantRules.GetBerryQuality(-1, 2) == BerryQuality.Dry, "Dry berry mapping");
        Assert(AppraisalAssistantRules.GetBerryQuality(-1, null) == BerryQuality.Unknown, "Incomplete berry state stays unknown");
        Assert(AppraisalAssistantRules.GetBerryQuality(9, 0) == BerryQuality.Unknown, "Unexpected berry state stays unknown");

        AppraisalValueBreakdown? tactical = AppraisalAssistantRules.Hand(
            "CARBON_COMPOSITE", "EMG_PLUS", "NONE", "ARMORED", "BLADED");
        Assert(tactical != null && tactical.Total == 425 && tactical.MatchesNative(425),
            "Tactical hand includes every component and set bonus");
        AppraisalValueBreakdown? deluxeHand = AppraisalAssistantRules.Hand(
            "GOLD_PLATED", "NEURAL", "NONE", "NONE", "NONE");
        Assert(deluxeHand != null && deluxeHand.Total == 620, "Deluxe hand set bonus");
        AppraisalValueBreakdown? minimumHand = AppraisalAssistantRules.Hand(
            "ALUMINUM_ALLOY", "UNSPECIFIED", "DAMAGE", "NONE", "NONE");
        Assert(minimumHand != null && minimumHand.Total == 50 && minimumHand.Subtotal < 50,
            "Hand minimum value is enforced");
        Assert(AppraisalAssistantRules.Hand("UNKNOWN", "EMG", "NONE", "NONE", "NONE") == null,
            "Unknown hand material never produces a guessed breakdown");

        AppraisalValueBreakdown? ignition = AppraisalAssistantRules.Plasma(
            "NONE", "NONE", "NONE", "NONE", "HIGH_QUALITY", "IGNITION_LAB");
        Assert(ignition != null && ignition.Total == 1200, "Ignition Lab plasma breakdown");
        AppraisalValueBreakdown? wild = AppraisalAssistantRules.Plasma(
            "NONE", "NONE", "NONE", "NONE", "POOR1", "LL");
        Assert(wild != null && wild.Total == 210, "Wild bonus also applies to low-grade colors");
        AppraisalValueBreakdown? minimumPlasma = AppraisalAssistantRules.Plasma(
            "WEAR_HEAVY", "CRACK", "UNCOVERED", "NONE", "POOR1", "NONE");
        Assert(minimumPlasma != null && minimumPlasma.Total == 90 && minimumPlasma.Subtotal < 90,
            "Plasma minimum value is enforced");
        Assert(AppraisalAssistantRules.Plasma("NONE", "NONE", "NONE", "NONE", "UNKNOWN", "NONE") == null,
            "Unknown plasma color never produces a guessed breakdown");

        AppraisalValueBreakdown? ribwich = AppraisalAssistantRules.Ribwich(
            0, 1430, 1, 1, 1, 1, 425, 1, 0, -1, 0, 0);
        Assert(ribwich != null && ribwich.Total == 1430 && ribwich.Lines.Count == 6,
            "Ribwich authentic value includes meat, deluxe, date, run and facility components");
        AppraisalValueBreakdown? opened = AppraisalAssistantRules.Ribwich(
            1, 40, 0, 0, 0, 1, 425, 1, 0, -1, 0, 0);
        Assert(opened != null && opened.Total == 40 && opened.FixedLabel == "value.opened",
            "Opened Ribwich uses its fixed value");
        AppraisalValueBreakdown? fake = AppraisalAssistantRules.Ribwich(
            0, 40, 0, 0, 0, 1, 440, 1, 0, 0, 0, 0);
        Assert(fake != null && fake.Total == 40 && fake.Reasons.Count == 1 &&
            fake.Reasons[0].Label == "reason.invalidYear", "Counterfeit Ribwich explains its invalid year");
        Assert(AppraisalAssistantRules.Ribwich(
            0, 40, 0, 0, 0, 1, 425, 1, 0, -1, 0, 0) == null,
            "Native counterfeit state without a detected reason is not explained by guesswork");
    }

    private static void TestAssistantLocalization()
    {
        string[] locales = { "en", "zh", "ja", "de", "ru", "fr", "es", "pl", "ptBR", "ko", "it" };
        foreach (string locale in locales)
        {
            for (int key = 0; key < AssistantLocalization.KeyCount; key++)
                Assert(!string.IsNullOrWhiteSpace(AssistantLocalization.Get(key, locale)),
                    $"Assistant text {key} localized for {locale}");
        }
        Assert(AssistantLocalization.Get(AssistantLocalization.AugDetected, "nl") ==
            AssistantLocalization.Get(AssistantLocalization.AugDetected, "en"),
            "Unsupported assistant locale uses English");
        Assert(AssistantLocalization.ReasonKey(AugDetectionReason.Freeze) == AssistantLocalization.Freeze,
            "Freeze reason maps to its localized text");
        Assert(AssistantLocalization.ReasonKey(AugDetectionReason.Seizure) == AssistantLocalization.Seizure,
            "Seizure reason maps to its localized text");
        Assert(AssistantLocalization.ReasonKey(AugDetectionReason.NoVisibleClue) == AssistantLocalization.NoVisibleClue,
            "Hidden aug maps to the honest no-clue explanation");
        Assert(AssistantLocalization.ReasonKey(AugDetectionReason.None) == AssistantLocalization.NoAug,
            "Non-aug maps to the negative inspection result");
        foreach (string locale in locales)
        {
            Assert(!string.IsNullOrWhiteSpace(AssistantAppraisalLocalization.Get("auth.counterfeit", locale)),
                $"Authenticity result available for {locale}");
            Assert(!string.IsNullOrWhiteSpace(AssistantAppraisalLocalization.Get("arrival.both", locale)),
                $"Combined arrival alert available for {locale}");
        }
        Assert(AssistantAppraisalLocalization.Get("auth.genuine", "zh") == "真伪鉴定：正品。",
            "Chinese genuine result is direct");
        Assert(AssistantAppraisalLocalization.Berry(BerryQuality.Dry, "zh") == "干枯",
            "Chinese berry quality is direct");
        AppraisalValueBreakdown sample = AppraisalAssistantRules.Hand(
            "CARBON_COMPOSITE", "EMG_PLUS", "NONE", "ARMORED", "BLADED")!;
        string formula = AssistantAppraisalLocalization.FormatBreakdown(sample, "zh");
        Assert(formula.Contains("基础 100") && formula.Contains("EMG+ 接口 45") &&
            formula.Contains("刀刃组件 80") && formula.EndsWith("425"),
            "Chinese value formula names and amounts every component");
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

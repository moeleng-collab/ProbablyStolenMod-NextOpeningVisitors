using System;
using System.Collections.Generic;

namespace NextOpeningVisitors
{
    internal static class PhoneDialogueTextRules
    {
        private sealed class LocaleTexts
        {
            internal LocaleTexts(
                string orderFormat,
                string beer,
                string dreamDust,
                string ores,
                string oxymore,
                string ready,
                string delivery,
                string freeSample,
                string ashishDelivery,
                string caulsonDelivery,
                string daleDelivery,
                string odinVisit)
            {
                OrderFormat = orderFormat;
                Beer = beer;
                DreamDust = dreamDust;
                Ores = ores;
                Oxymore = oxymore;
                Ready = ready;
                Delivery = delivery;
                FreeSample = freeSample;
                AshishDelivery = ashishDelivery;
                CaulsonDelivery = caulsonDelivery;
                DaleDelivery = daleDelivery;
                OdinVisit = odinVisit;
            }

            internal string OrderFormat { get; }
            internal string Beer { get; }
            internal string DreamDust { get; }
            internal string Ores { get; }
            internal string Oxymore { get; }
            internal string Ready { get; }
            internal string Delivery { get; }
            internal string FreeSample { get; }
            internal string AshishDelivery { get; }
            internal string CaulsonDelivery { get; }
            internal string DaleDelivery { get; }
            internal string OdinVisit { get; }
        }

        private static readonly Dictionary<string, LocaleTexts> Locales = new Dictionary<string, LocaleTexts>(StringComparer.Ordinal)
        {
            { "en", new LocaleTexts(
                "[Order {0} {1} (Next Opening Delivery)]",
                "Beers", "Dream Dust", "Ores", "Oxymore Injectors",
                "Ok, I'll arrange that for you. It will arrive when you next open the shop.",
                "And we will deliver when you next open the shop.",
                "We will send you a free sample when you next open the shop.",
                "You'll get your stuff when you next open the shop. No fuss.",
                "-our crew'll come for ya when you next open the shop. Simple as that.",
                "a courier will handle it when you next open the shop.",
                "I'll visit when you next open the shop.") },
            { "zh", new LocaleTexts(
                "[订购{0}{1}（下次开店送达）]",
                "份啤酒", "份梦尘", "份矿石", "支奥克西莫注射器",
                "好的，我会为你安排，货物会在你下次开店时送到。",
                "我们会在你下次开店时派人送货。",
                "我们会在你下次开店时送你一份免费样品。",
                "你下次开店时交货。不扯有的没的。",
                "——我们的伙计会在你下次开店时送上门。就这么简单。",
                "会在你下次开店时安排快递员送货。",
                "我会在你下一次开店时到访。") },
            { "ja", new LocaleTexts(
                "[{1}を{0}点注文（次回開店時に配達）]",
                "ビール", "ドリームダスト", "鉱石", "オキシモア注射器",
                "承知しました。次回開店時に届くよう手配します。",
                "次回開店時に配達します。",
                "次回開店時に無料サンプルをお送りします。",
                "次回開店時に商品をお渡しします。余計な手間はかけません。",
                "―次回開店時に私たちの配達員が伺います。簡単なことです。",
                "次回開店時に配達員が届けます。",
                "次回開店時に伺います。") },
            { "de", new LocaleTexts(
                "[{0} × {1} bestellen (Lieferung beim nächsten Öffnen)]",
                "Bier", "Traumstaub", "Erze", "Oxymore-Injektoren",
                "In Ordnung, ich bereite alles vor. Es kommt, wenn du das nächste Mal öffnest.",
                "Wir liefern, wenn du das nächste Mal öffnest.",
                "Wir schicken dir eine kostenlose Probe, wenn du das nächste Mal öffnest.",
                "Du bekommst deine Ware, wenn du das nächste Mal öffnest. Ganz unkompliziert.",
                "– unsere Leute kommen, wenn du das nächste Mal öffnest. So einfach ist das.",
                "ein Kurier liefert, wenn du das nächste Mal öffnest.",
                "Ich komme vorbei, wenn du das nächste Mal öffnest.") },
            { "ru", new LocaleTexts(
                "[Заказать: {1} × {0} (доставка при следующем открытии)]",
                "пиво", "«Сонная пыль»", "руда", "инъекторы Oxymore",
                "Хорошо, я всё подготовлю. Доставка будет при следующем открытии магазина.",
                "Мы доставим товар при следующем открытии магазина.",
                "Мы отправим бесплатный образец при следующем открытии магазина.",
                "Получишь товар при следующем открытии магазина. Без лишних разговоров.",
                "— наши ребята придут при следующем открытии магазина. Всё просто.",
                "курьер доставит заказ при следующем открытии магазина.",
                "Я зайду при следующем открытии магазина.") },
            { "fr", new LocaleTexts(
                "[Commander {0} × {1} (livraison à la prochaine ouverture)]",
                "bières", "poudre de rêve", "minerais", "injecteurs d’Oxymore",
                "D’accord, je vais tout préparer. Ce sera livré à votre prochaine ouverture.",
                "Nous livrerons à votre prochaine ouverture.",
                "Nous vous enverrons un échantillon gratuit à votre prochaine ouverture.",
                "Vous recevrez votre marchandise à votre prochaine ouverture. Sans détour.",
                "– notre équipe passera à votre prochaine ouverture. C’est aussi simple que ça.",
                "un livreur s’en chargera à votre prochaine ouverture.",
                "Je passerai à votre prochaine ouverture.") },
            { "es", new LocaleTexts(
                "[Pedir {0} × {1} (entrega en la próxima apertura)]",
                "cervezas", "polvo de sueños", "minerales", "inyectores de Oxymore",
                "De acuerdo, lo prepararé. Llegará cuando vuelvas a abrir la tienda.",
                "Lo entregaremos cuando vuelvas a abrir la tienda.",
                "Te enviaremos una muestra gratuita cuando vuelvas a abrir la tienda.",
                "Recibirás tu mercancía cuando vuelvas a abrir la tienda. Sin rodeos.",
                "—nuestro equipo irá cuando vuelvas a abrir la tienda. Así de simple.",
                "un mensajero lo entregará cuando vuelvas a abrir la tienda.",
                "Te visitaré cuando vuelvas a abrir la tienda.") },
            { "pl", new LocaleTexts(
                "[Zamów: {1} × {0} (dostawa przy następnym otwarciu)]",
                "piwo", "pył snów", "ruda", "wstrzykiwacze Oxymore",
                "Dobrze, przygotuję zamówienie. Dotrze przy następnym otwarciu sklepu.",
                "Dostarczymy towar przy następnym otwarciu sklepu.",
                "Wyślemy bezpłatną próbkę przy następnym otwarciu sklepu.",
                "Otrzymasz towar przy następnym otwarciu sklepu. Bez zbędnych ceregieli.",
                "— nasi ludzie przyjdą przy następnym otwarciu sklepu. To proste.",
                "kurier dostarczy zamówienie przy następnym otwarciu sklepu.",
                "Odwiedzę cię przy następnym otwarciu sklepu.") },
            { "ptBR", new LocaleTexts(
                "[Pedir {0} × {1} (entrega na próxima abertura)]",
                "cervejas", "pó dos sonhos", "minérios", "injetores de Oxymore",
                "Certo, vou preparar tudo. Chegará na próxima vez que você abrir a loja.",
                "Faremos a entrega na próxima vez que você abrir a loja.",
                "Enviaremos uma amostra grátis na próxima vez que você abrir a loja.",
                "Você receberá suas mercadorias na próxima vez que abrir a loja. Sem enrolação.",
                "— nossa equipe irá na próxima vez que você abrir a loja. Simples assim.",
                "um entregador cuidará disso na próxima vez que você abrir a loja.",
                "Farei uma visita na próxima vez que você abrir a loja.") },
            { "ko", new LocaleTexts(
                "[{1} {0}개 주문(다음 영업 시작 시 배송)]",
                "맥주", "드림 더스트", "광석", "옥시모어 주사기",
                "알겠습니다. 준비해서 다음에 가게를 열 때 배달하겠습니다.",
                "다음에 가게를 열 때 배송해 드리겠습니다.",
                "다음에 가게를 열 때 묣료 샘플을 본내 드리겠습니다.",
                "다음에 가게를 열 때 물건을 받을 수 있습니다. 다른 건 신경 쓸 필요 없습니다.",
                "—다음에 가게를 열 때 우리 사람이 갈 겁니다. 간단하죠.",
                "다음에 가게를 열 때 배달원이 본낼 겁니다.",
                "다음에 가게를 열 때 방문하겠습니다.") },
            { "it", new LocaleTexts(
                "[Ordina {0} × {1} (consegna alla prossima apertura)]",
                "birre", "polvere dei sogni", "minerali", "iniettori di Oxymore",
                "Va bene, la preparerò. Arriverà alla tua prossima apertura.",
                "Consegneremo alla tua prossima apertura.",
                "Ti invieremo un campione gratuito alla tua prossima apertura.",
                "Riceverai la tua merce alla tua prossima apertura. Senza complicazioni.",
                "– la nostra squadra verrà alla tua prossima apertura. È semplice.",
                "un corriere consegnerà alla tua prossima apertura.",
                "Ti visiterò alla tua prossima apertura.") }
        };

        internal static bool TryRewrite(string? key, object[]? arguments, string? localeCode, out string? result)
        {
            result = null;
            if (key == null) return false;
            LocaleTexts texts = Locales[ModLocalization.NormalizeLocale(localeCode)];

            switch (key)
            {
                case "red_imp_order1":
                case "red_imp_order2":
                    return TryFormatOrder(texts, texts.Beer, arguments, out result);
                case "chem_order1":
                case "chem_order2":
                    return TryFormatOrder(texts, texts.DreamDust, arguments, out result);
                case "mining_order1":
                case "mining_order2":
                    return TryFormatOrder(texts, texts.Ores, arguments, out result);
                case "pharma_order1":
                case "pharma_order2":
                    return TryFormatOrder(texts, texts.Oxymore, arguments, out result);
                case "red_imp_rep4":
                case "ashish_ready":
                case "caulson_ready":
                case "dale_ready":
                    result = texts.Ready;
                    return true;
                case "red_imp_rep_intro4":
                    result = texts.Delivery;
                    return true;
                case "red_imp_rep_intro6":
                case "ashish_intro6":
                case "caulson_intro7":
                case "dale_intro9":
                    result = texts.FreeSample;
                    return true;
                case "ashish_intro4":
                    result = texts.AshishDelivery;
                    return true;
                case "caulson_intro5":
                    result = texts.CaulsonDelivery;
                    return true;
                case "dale_intro6":
                    result = texts.DaleDelivery;
                    return true;
                case "odin_dropby":
                    result = texts.OdinVisit;
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryFormatOrder(LocaleTexts texts, string item, object[]? arguments, out string? result)
        {
            result = null;
            if (arguments == null || arguments.Length == 0) return false;
            result = string.Format(texts.OrderFormat, arguments[0], item);
            return true;
        }
    }
}

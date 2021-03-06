﻿#pragma warning disable 1591

using Discord;
using Sanakan.Database.Models;
using Sanakan.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanakan.Extensions
{
    public static class UserExtension
    {
        public static bool SendAnyMsgInMonth(this User u)
            => (u.MessagesCnt - u.MessagesCntAtDate) > 0;

        public static bool IsCharCounterActive(this User u)
            => DateTime.Now.Month == u.MeasureDate.Month && DateTime.Now.Year == u.MeasureDate.Year;

        public static User Default(this User u, ulong id)
        {
            var user = new User
            {
                Id = id,
                Level = 1,
                TcCnt = 0,
                ScCnt = 100,
                ExpCnt = 10,
                Shinden = 0,
                MessagesCnt = 0,
                CommandsCnt = 0,
                MessagesCntAtDate = 0,
                IsBlacklisted = false,
                CharacterCntFromDate = 0,
                ShowWaifuInProfile = false,
                ProfileType = ProfileType.Stats,
                StatsReplacementProfileUri = "none",
                TimeStatuses = new List<TimeStatus>(),
                BackgroundProfileUri = $"./Pictures/defBg.png",
                MeasureDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                GameDeck = new GameDeck
                {
                    Id = id,
                    Waifu = 0,
                    CTCnt = 0,
                    Karma = 0,
                    ItemsDropped = 0,
                    Items = new List<Item>(),
                    Cards = new List<Card>(),
                    WishlistIsPrivate = false,
                    Wishes = new List<WishlistObject>(),
                    PvPStats = new List<CardPvPStats>(),
                    BoosterPacks = new List<BoosterPack>(),
                    ExpContainer = new ExpContainer
                    {
                        Id = id,
                        ExpCount = 0,
                        Level = ExpContainerLevel.Disabled
                    }
                },
                Stats = new UserStats
                {
                    Hit = 0,
                    Head = 0,
                    Misd = 0,
                    Tail = 0,
                    ScLost = 0,
                    IncomeInSc = 0,
                    RightAnswers = 0,
                    TotalAnswers = 0,
                    UpgaredCards = 0,
                    YamiUpgrades = 0,
                    YatoUpgrades = 0,
                    RaitoUpgrades = 0,
                    ReleasedCards = 0,
                    TurnamentsWon = 0,
                    UpgradedToSSS = 0,
                    UnleashedCards = 0,
                    SacraficeCards = 0,
                    DestroyedCards = 0,
                    WastedTcOnCards = 0,
                    SlotMachineGames = 0,
                    WastedTcOnCookies = 0,
                    OpenedBoosterPacks = 0,
                    OpenedBoosterPacksActivity = 0,
                },
                SMConfig = new SlotMachineConfig
                {
                    PsayMode = 0,
                    Beat = SlotMachineBeat.b1,
                    Rows = SlotMachineSelectedRows.r1,
                    Multiplier = SlotMachineBeatMultiplier.x1,
                }
            };

            user.GameDeck.BoosterPacks.Add(new BoosterPack
            {
                CardCnt = 5,
                MinRarity = Rarity.A,
                Name = "Startowy pakiet",
                IsCardFromPackTradable = true
            });

            return user;
        }

        public static bool ReachedDailyMaxItemsCount(this GameDeck deck)
            => deck.ItemsDropped >= 200;

        public static bool CanFightPvP(this GameDeck deck) =>
            deck.GetMaxDeckPower() >= deck.GetDeckPower();

        public static double GetDeckPower(this GameDeck deck)
            => deck.Cards.Where(x => x.Active).Sum(x => x.GetCardPower());

        public static double GetMaxDeckPower(this GameDeck deck) => 500;

        public static string GetUserNameStatus(this GameDeck deck)
        {
            if (deck.Karma >= 2000) return $"Papaj";
            if (deck.Karma >= 1600) return $"Miłościwy kumpel";
            if (deck.Karma >= 1200) return $"Oślepiony bugiem";
            if (deck.Karma >= 800) return $"Pan pokoiku";
            if (deck.Karma >= 400) return $"Błogosławiony rycerz";
            if (deck.Karma >= 200) return $"Pionek buga";
            if (deck.Karma >= 100) return $"Sługa buga";
            if (deck.Karma >= 50) return $"Biały koleś";
            if (deck.Karma >= 10) return $"Pantofel";
            if (deck.Karma >= 5) return $"Lizus";
            if (deck.Karma <= -2000) return $"Mroczny panocek";
            if (deck.Karma <= -1600) return $"Nienawistny koleżka";
            if (deck.Karma <= -1200) return $"Mściwy ślepiec";
            if (deck.Karma <= -800) return $"Pan wojenki";
            if (deck.Karma <= -400) return $"Przeklęty rycerz";
            if (deck.Karma <= -200) return $"Ciemny pionek";
            if (deck.Karma <= -100) return $"Sługa mroku";
            if (deck.Karma <= -50) return $"Murzynek";
            if (deck.Karma <= -10) return $"Rzezimieszek";
            if (deck.Karma <= -5) return $"Buntownik";
            return "Wieśniak";
        }

        public static bool CanCreateDemon(this GameDeck deck) => deck.Karma <= -2000;

        public static bool CanCreateAngel(this GameDeck deck) => deck.Karma >= 2000;

        public static bool IsMarketDisabled(this GameDeck deck) => deck.Karma <= -400;

        public static bool IsBlackMarketDisabled(this GameDeck deck) => deck.Karma > -400;

        public static bool IsEvil(this GameDeck deck) => deck.Karma <= -10;

        public static bool IsGood(this GameDeck deck) => deck.Karma >= 10;

        public static double AffectionFromKarma(this GameDeck deck)
        {
            var karmaDif = deck.Karma / 150d;
            if (karmaDif < -6) karmaDif = -6;
            if (karmaDif > 6) karmaDif = 6;
            return karmaDif;
        }

        public static double GetStrongestCardPower(this GameDeck deck)
        {
            return deck.Cards.OrderByDescending(x => x.GetCardPower()).FirstOrDefault()?.GetCardPower() ?? 0;
        }

        public static List<ulong> GetTitlesWishList(this GameDeck deck)
        {
            return deck.Wishes.Where(x => x.Type == WishlistObjectType.Title).Select(x => x.ObjectId).ToList();
        }

        public static List<ulong> GetCardsWishList(this GameDeck deck)
        {
            return deck.Wishes.Where(x => x.Type == WishlistObjectType.Card).Select(x => x.ObjectId).ToList();
        }

        public static List<ulong> GetCharactersWishList(this GameDeck deck)
        {
            return deck.Wishes.Where(x => x.Type == WishlistObjectType.Character).Select(x => x.ObjectId).ToList();
        }

        public static void RemoveCharacterFromWishList(this GameDeck deck, ulong id)
        {
            var en = deck.Wishes.FirstOrDefault(x => x.Type == WishlistObjectType.Character && x.ObjectId == id);
            if (en != null) deck.Wishes.Remove(en);
        }

        public static void RemoveCardFromWishList(this GameDeck deck, ulong id)
        {
            var en = deck.Wishes.FirstOrDefault(x => x.Type == WishlistObjectType.Card && x.ObjectId == id);
            if (en != null) deck.Wishes.Remove(en);
        }

        public static EmbedBuilder GetStatsView(this User u, IUser user)
        {
            string stats = $"**Wiadomości**: {u.MessagesCnt}\n**Polecenia:** {u.CommandsCnt}";

            return new EmbedBuilder
            {
                Color = EMType.Info.Color(),
                Description = $"**Statystyki** {user.Mention}:\n\n{stats}\n{u.Stats.ToView()}".TrimToLength(1950)
            };
        }

        public static string GetViewValueForTop(this User u, TopType type)
        {
            switch (type)
            {
                default:
                case TopType.Level:
                    return $"{u.Level} **LVL** ({u.ExpCnt} **EXP**)";

                case TopType.ScCnt:
                    return $"{u.ScCnt} **SC**";

                case TopType.TcCnt:
                    return $"{u.TcCnt} **TC**";

                case TopType.Posts:
                    return $"{u.MessagesCnt}";

                case TopType.PostsMonthly:
                    return $"{u.MessagesCnt - u.MessagesCntAtDate}";

                case TopType.PostsMonthlyCharacter:
                    return $"{u.CharacterCntFromDate / (u.MessagesCnt - u.MessagesCntAtDate)}";

                case TopType.Commands:
                    return $"{u.CommandsCnt}";

                case TopType.Card:
                    return u.GameDeck.Cards.OrderByDescending(x => x.GetCardPower())?.FirstOrDefault()?.GetString(false, false, true) ?? "---";

                case TopType.Cards:
                    return $"{u.GameDeck.Cards.Count}";

                case TopType.CardsPower:
                    return u.GameDeck.GetCardCountStats();

                case TopType.Karma:
                case TopType.KarmaNegative:
                    return $"{u.GameDeck.Karma}";
            }
        }

        public static string GetCardCountStats(this GameDeck deck)
        {
            string stats = "";

            foreach (Rarity rarity in (Rarity[])Enum.GetValues(typeof(Rarity)))
            {
                var count = deck.Cards.Count(x => x.Rarity == rarity);
                if (count > 0) stats += $"**{rarity.ToString().ToUpper()}**: {count} ";
            }

            return stats;
        }

        public static bool ApplySlotMachineSetting(this User user, SlotMachineSetting type, string value)
        {
            try
            {
                switch (type)
                {
                    case SlotMachineSetting.Beat:
                            var bt = (SlotMachineBeat)Enum.Parse(typeof(SlotMachineBeat), $"b{value}");
                            user.SMConfig.Beat = bt;
                        break;
                    case SlotMachineSetting.Rows:
                            var rw = (SlotMachineSelectedRows)Enum.Parse(typeof(SlotMachineSelectedRows), $"r{value}");
                            user.SMConfig.Rows = rw;
                        break;
                    case SlotMachineSetting.Multiplier:
                            var mt = (SlotMachineBeatMultiplier)Enum.Parse(typeof(SlotMachineBeatMultiplier), $"x{value}");
                            user.SMConfig.Multiplier = mt;
                        break;

                    default:
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void StoreExpIfPossible(this User user, double exp)
        {
            var maxToTransfer = user.GameDeck.ExpContainer.GetMaxExpTransferToChest();
            if (maxToTransfer != -1)
            {
                exp = Math.Floor(exp);
                var diff = maxToTransfer - user.GameDeck.ExpContainer.ExpCount;
                if (diff <= exp) exp = Math.Floor(diff);
                if (exp < 0) exp = 0;
            }
            user.GameDeck.ExpContainer.ExpCount += exp;
        }
    }
}

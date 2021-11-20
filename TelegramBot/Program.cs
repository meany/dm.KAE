using dm.KAE.Common;
using dm.KAE.Data;
using dm.KAE.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace dm.KAE.TelegramBot
{
    public class Program
    {
        private ITelegramBotClient botClient;
        private IServiceProvider services;
        private IConfigurationRoot configuration;
        private Config config;
        private AppDbContext db;
        private string cmdList;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private List<LastMessage> lastMsgs = new List<LastMessage>();

        public static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("Config.TelegramBot.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("Config.TelegramBot.Local.json", optional: true, reloadOnChange: true);

                configuration = builder.Build();

                services = new ServiceCollection()
                    .Configure<Config>(configuration)
                    .AddDatabase<AppDbContext>(configuration.GetConnectionString("Database"))
                    .BuildServiceProvider();
                config = services.GetService<IOptions<Config>>().Value;
                db = services.GetService<AppDbContext>();

                if (db.Database.GetPendingMigrations().Count() > 0)
                {
                    log.Info("Migrating database");
                    db.Database.Migrate();
                }

                cmdList = System.IO.File.ReadAllText("cmds.txt");

                await RunBot(args);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task RunBot(string[] args)
        {
            try
            {
                botClient = new TelegramBotClient(config.BotToken);
                log.Info($"Bot connected");

                if (args.Length > 0)
                {
                    await RunBotArgs(args, botClient);
                    return;
                }

                if (config.BotWatch)
                {
                    log.Info("BotWatch = true, waiting for messages");
                    botClient.OnMessage += BotClient_OnMessage;
                    botClient.StartReceiving();
                    await Task.Delay(-1).ConfigureAwait(false);
                }
                else
                {
                    log.Info("BotWatch = false, sending ad-hoc message");
                    return;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task RunBotArgs(string[] args, ITelegramBotClient botClient)
        {
            log.Info("Running with args");
            return;
        }

        private async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            if (msg.Text == null || !msg.Text.StartsWith('/'))
                return;

            string cmdAndArgs = msg.Text.Substring(1).Trim();
            string cmd = cmdAndArgs;
            string args = string.Empty;
            if (cmdAndArgs.Contains(' '))
            {
                int firstSpace = cmdAndArgs.IndexOf(' ');
                cmd = cmdAndArgs.Substring(0, firstSpace);
                args = cmdAndArgs.Substring(firstSpace + 1).Trim();
            }
            if (cmd.Contains('@'))
                cmd = cmd.Split('@')[0];

            if (msg.Date.AddMinutes(1) <= DateTime.UtcNow)
            {
                log.Info($"(old: ignoring) ChatId: {msg.Chat.Id}, Command: {cmd}, Args: {args}");
                return;
            }

            log.Info($"ChatId: {msg.Chat.Id}, Command: {cmd}, Args: {args}");

            // TODO: add request/rate limit

            string reply = await GetCmdReply(cmd, args, msg);
            if (string.IsNullOrEmpty(reply))
                return;

            var sent = await botClient.SendTextMessageAsync(
              chatId: msg.Chat,
              text: reply,
              parseMode: ParseMode.Html,
              disableNotification: true,
              disableWebPagePreview: true
            );

            if (msg.Chat.Type == ChatType.Private)
                return;

            // delete user's message
            try
            {
                await botClient.DeleteMessageAsync(
                    chatId: msg.Chat,
                    messageId: msg.MessageId
                );
            }
            catch
            {
                log.Info("could not delete user message");
            }

            // find last message and delete
            var lastMsg = await db.LastMessages.FirstOrDefaultAsync(x => x.ChatId == msg.Chat.Id);
            if (lastMsg != null)
            {
                try
                {
                    await botClient.DeleteMessageAsync(
                        chatId: lastMsg.ChatId,
                        messageId: lastMsg.MessageId
                    );
                }
                catch
                {
                    log.Info("could not delete bot's message");
                }
                db.LastMessages.Remove(lastMsg);
                db.SaveChanges();
            }

            // add new msg
            db.LastMessages.Add(new LastMessage
            {
                ChatId = msg.Chat.Id,
                MessageId = sent.MessageId
            });
            db.SaveChanges();
        }

        private async Task<string> GetCmdReply(string cmd, string args, Message msg)
        {
            //string pre = $"@{msg.From.Username} —";
            switch (cmd)
            {
                case "start":
                    return cmdList;

                case "price":
                    var price = await Data.Common.GetPrices(db);
                    return $"$ <b>{price.PriceUSD.FormatUsd()}</b>\n" +
                        $"₿ <b>{price.PriceBTC.FormatBtc()}</b>\n" +
                        $"Ξ <b>{price.PriceETH.FormatEth()}</b>";

                case "supply":
                    var supply = await Data.Common.GetStats(db);
                    return $"Supply: <b>{supply.Supply.FormatKae()}</b> $KAE\n" +
                        $"Circulation: <b>{supply.Circulation.FormatKae()}</b> $KAE";

                case "cap":
                    var data = await Data.Common.GetAllInfo(db);
                    //return $"Market Cap: $ <b>{mcap.MarketCapUSD.FormatLarge()}</b>\n" +
                    //    $"Volume (24h): $ <b>{mcap.VolumeUSD.FormatLarge()}</b>";
                    ulong full = (ulong)Math.Round(data.Price.PriceUSD * data.Stat.Supply, 0);
                    ulong circ = (ulong)Math.Round(data.Price.PriceUSD * data.Stat.Circulation, 0);
                    return $"Market Cap (Fully Diluted): $ <b>{full.FormatLarge()}</b>\n" +
                        $"Market Cap (Circulating): $ <b>{circ.FormatLarge()}</b>\n" +
                        $"Volume (24h): $ <b>{Convert.ToUInt64(data.Price.VolumeUSD).FormatLarge()}</b>";

                case "holders":
                    var holders = await Data.Common.GetTotalHolders(db);
                    return $"<b>{holders.Format()}</b> total holders";
            }

            return string.Empty;
        }
    }
}

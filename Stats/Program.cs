using dm.KAE.Common;
using dm.KAE.Data;
using dm.KAE.Data.Models;
using dm.KAE.Response;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace dm.KAE.Stats
{
    class Program
    {
        private IServiceProvider services;
        private IConfigurationRoot configuration;
        private Config config;
        private AppDbContext db;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private BigInteger supply;
        private BigInteger teamAmt;
        private List<EsTxsResult> esTxs;

        public static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("Config.Stats.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("Config.Stats.Local.json", optional: true, reloadOnChange: true);

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

                await Start();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task Start()
        {
            try
            {
                log.Info("Getting Etherscan info");
                
                await GetInfo();
                //await InsertNewTxs();

                var circulation = supply - teamAmt;

                var item = new Stat
                {
                    Circulation = circulation.ToEth(),
                    Date = DateTime.UtcNow,
                    Group = Guid.NewGuid(),
                    Supply = supply.ToEth(),
                };

                db.Add(item);

                log.Info("Saving transaction stats to database");
                db.SaveChanges();

                //

                //log.Info("Updating holders table");
                //await BuildHolders();

                log.Info("Complete");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task GetInfo()
        {
            try
            {
                var client = new RestClient("https://api.etherscan.io");
                //await GetTxs(client);
                //await Task.Delay(400);
                await GetSupply(client);
                await Task.Delay(400);
                await GetTeamAmount(client);
                await Task.Delay(400);

                while (
                    teamAmt == 0 ||
                    supply == 0
                //    esTxs == null
                )
                {
                    await Task.Delay(250);
                }

                client = null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task GetSupply(RestClient client)
        {
            var req = new RestRequest("api", Method.GET);
            req.AddParameter("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            req.AddParameter("module", "stats");
            req.AddParameter("action", "tokensupply");
            req.AddParameter("contractaddress", Statics.TOKEN_KAE);
            req.AddParameter("apikey", config.EtherscanToken);

            var res = await client.ExecuteAsync<EsToken>(req);
            supply = BigInteger.Parse(res.Data.Result);
            log.Info($"GetSupply: OK ({supply})");
        }

        private async Task GetTeamAmount(RestClient client)
        {
            var req = new RestRequest("api", Method.GET);
            req.AddParameter("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            req.AddParameter("module", "account");
            req.AddParameter("action", "tokenbalance");
            req.AddParameter("contractaddress", Statics.TOKEN_KAE);
            req.AddParameter("address", Statics.ADDRESS_TEAM);
            req.AddParameter("tag", "latest");
            req.AddParameter("apikey", config.EtherscanToken);

            var res = await client.ExecuteAsync<EsToken>(req);
            teamAmt = BigInteger.Parse(res.Data.Result);
            log.Info($"GetTeamAmount: OK ({teamAmt})");
        }

        //private async Task GetTxs(RestClient client)
        //{
        //    var lastTx = db.Transactions
        //        .AsNoTracking()
        //        .OrderByDescending(x => x.TimeStamp)
        //        .FirstOrDefault();

        //    int start = 0;
        //    if (lastTx != null)
        //        start = int.Parse(lastTx.BlockNumber) + 1;

        //    var req = new RestRequest("api", Method.GET);
        //    req.AddParameter("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        //    req.AddParameter("module", "account");
        //    req.AddParameter("action", "tokentx");
        //    req.AddParameter("contractaddress", Statics.TOKEN_YLD);
        //    req.AddParameter("startblock", start);
        //    req.AddParameter("endblock", "999999999");
        //    req.AddParameter("apikey", config.EtherscanToken);

        //    var res = await client.ExecuteAsync<EsTxs>(req);
        //    if (res.Data.Result.Count == 0)
        //    {
        //        esTxs = new List<EsTxsResult>();
        //        log.Info($"GetTxs: {res.Data.Message} (0)");
        //        return;
        //    }

        //    esTxs = res.Data.Result
        //        .OrderBy(x => x.BlockNumber)
        //        .ToList();
        //    log.Info($"GetTxs: {res.Data.Message} ({esTxs.Count()}: {start} to {esTxs.Last().BlockNumber})");
        //}

        //private async Task BuildHolders()
        //{
        //    var holders = new List<Holder>();

        //    foreach (var item in dbTxs)
        //    {
        //        if (item.From == item.To)
        //            continue;

        //        var fromHolder = holders.Where(x => x.Address == item.From).FirstOrDefault();
        //        if (fromHolder != null)
        //        {
        //            var newValue = BigInteger.Parse(fromHolder.Value) - BigInteger.Parse(item.Value);
        //            fromHolder.Value = newValue.ToString();
        //        }
        //        else
        //        {
        //            holders.Add(new Holder
        //            {
        //                Address = item.From,
        //                FirstBlockNumber = item.BlockNumber,
        //                FirstTimeStamp = item.TimeStamp,
        //                Value = $"-{item.Value}"
        //            });
        //        }

        //        var toHolder = holders.Where(x => x.Address == item.To).FirstOrDefault();
        //        if (toHolder != null)
        //        {
        //            var newValue = BigInteger.Parse(toHolder.Value) + BigInteger.Parse(item.Value);
        //            toHolder.Value = newValue.ToString();
        //        }
        //        else
        //        {
        //            holders.Add(new Holder
        //            {
        //                Address = item.To,
        //                FirstBlockNumber = item.BlockNumber,
        //                FirstTimeStamp = item.TimeStamp,
        //                Value = item.Value
        //            });
        //        }
        //    }

        //    await db.TruncateAsync<Holder>();

        //    db.AddRange(holders.Where(x => x.Value != "0" &&
        //        !x.Value.Contains('-') &&
        //        x.Address != Statics.ADDRESS_TEAM));
        //    await db.SaveChangesAsync();
        //}
    }
}
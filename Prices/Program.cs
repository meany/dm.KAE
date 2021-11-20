using CoinGecko.Clients;
using CoinGecko.Entities.Response.Coins;
using dm.KAE.Common;
using dm.KAE.Data;
using dm.KAE.Data.Models;
using dm.KAE.Response;
using Microsoft.EntityFrameworkCore;
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
using System.Numerics;
using System.Threading.Tasks;

namespace dm.KAE.Prices
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

        private CoinFullDataById data;

        public static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("Config.Prices.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("Config.Prices.Local.json", optional: true, reloadOnChange: true);

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

                await GetStatInfo();
                //await InsertNewTxs();

                var circulation = supply - teamAmt;

                var stat = new Stat
                {
                    Circulation = circulation.ToEth(),
                    Date = DateTime.UtcNow,
                    Group = Guid.NewGuid(),
                    Supply = supply.ToEth(),
                };

                db.Add(stat);

                log.Info("Saving transaction stats to database");
                db.SaveChanges();

                //

                //log.Info("Updating holders table");
                //await BuildHolders();

                log.Info("Complete");

                await GetInfo();

                // market cap
                decimal mktCapUsd = decimal.Parse(data.MarketData.MarketCap["usd"].Value.ToString());
                decimal mktCapUsdChgAmt = (data.MarketData.MarketCapChange24HInCurrency.Count == 0) ? 0 : decimal.Parse(data.MarketData.MarketCapChange24HInCurrency["usd"].ToString(), NumberStyles.Any);
                Change mktCapUsdChg = (mktCapUsdChgAmt > 0) ? Change.Up : (mktCapUsdChgAmt < 0) ? Change.Down : Change.None;
                decimal mktCapUsdChgPct = (data.MarketData.MarketCapChangePercentage24HInCurrency.Count == 0) ? 0 : decimal.Parse(data.MarketData.MarketCapChangePercentage24HInCurrency["usd"].ToString(), NumberStyles.Any);

                // volume
                int volumeUsd = (int)Math.Round(data.MarketData.TotalVolume["usd"].Value);

                // prices
                decimal priceBtc = decimal.Parse(data.MarketData.CurrentPrice["btc"].Value.ToString(), NumberStyles.Any);

                string changeBtc = "0";
                string changeEth = "0";
                string changeUsd = "0";
                string changeBtcPct = "0";
                string changeEthPct = "0";
                string changeUsdPct = "0";
                if (data.MarketData.PriceChange24HInCurrency.Count > 0 &&
                    data.MarketData.PriceChangePercentage24HInCurrency.Count > 0)
                {
                    changeBtc = data.MarketData.PriceChange24HInCurrency["btc"].ToString();
                    changeBtcPct = data.MarketData.PriceChangePercentage24HInCurrency["btc"].ToString();
                    changeEth = data.MarketData.PriceChange24HInCurrency["eth"].ToString();
                    changeEthPct = data.MarketData.PriceChangePercentage24HInCurrency["eth"].ToString();
                    changeUsd = data.MarketData.PriceChange24HInCurrency["usd"].ToString();
                    changeUsdPct = data.MarketData.PriceChangePercentage24HInCurrency["usd"].ToString();
                }

                decimal priceBtcChgAmt = decimal.Parse(changeBtc, NumberStyles.Any);
                Change priceBtcChg = (priceBtcChgAmt > 0) ? Change.Up : (priceBtcChgAmt < 0) ? Change.Down : Change.None;
                decimal priceBtcChgPct = decimal.Parse(changeBtcPct, NumberStyles.Any);

                decimal priceEth = decimal.Parse(data.MarketData.CurrentPrice["eth"].Value.ToString(), NumberStyles.Any);
                decimal priceEthChgAmt = decimal.Parse(changeEth, NumberStyles.Any);
                Change priceEthChg = (priceEthChgAmt > 0) ? Change.Up : (priceEthChgAmt < 0) ? Change.Down : Change.None;
                decimal priceEthChgPct = decimal.Parse(changeEthPct, NumberStyles.Any);

                decimal priceUsd = decimal.Parse(data.MarketData.CurrentPrice["usd"].Value.ToString(), NumberStyles.Any);
                decimal priceUsdChgAmt = decimal.Parse(changeUsd, NumberStyles.Any);
                Change priceUsdChg = (priceUsdChgAmt > 0) ? Change.Up : (priceUsdChgAmt < 0) ? Change.Down : Change.None;
                decimal priceUsdChgPct = decimal.Parse(changeUsdPct, NumberStyles.Any);

                var item = new Price
                {
                    Date = DateTime.UtcNow,
                    Group = stat.Group,
                    MarketCapUSD = int.Parse(Math.Round(mktCapUsd).ToString()),
                    MarketCapUSDChange = mktCapUsdChg,
                    MarketCapUSDChangePct = mktCapUsdChgPct,
                    PriceBTC = priceBtc,
                    PriceBTCChange = priceBtcChg,
                    PriceBTCChangePct = priceBtcChgPct,
                    PriceETH = priceEth,
                    PriceETHChange = priceEthChg,
                    PriceETHChangePct = priceEthChgPct,
                    PriceUSD = priceUsd,
                    PriceUSDChange = priceUsdChg,
                    PriceUSDChangePct = priceUsdChgPct,
                    VolumeUSD = volumeUsd,
                    Source = PriceSource.CoinGecko,
                };

                db.Add(item);

                log.Info("Saving prices to database");
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task GetStatInfo()
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
            var req1 = new RestRequest("api", Method.GET);
            req1.AddParameter("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            req1.AddParameter("module", "account");
            req1.AddParameter("action", "tokenbalance");
            req1.AddParameter("contractaddress", Statics.TOKEN_KAE);
            req1.AddParameter("address", Statics.ADDRESS_TEAM1);
            req1.AddParameter("tag", "latest");
            req1.AddParameter("apikey", config.EtherscanToken);

            var res1 = await client.ExecuteAsync<EsToken>(req1);
            teamAmt = BigInteger.Parse(res1.Data.Result);

            var req2 = new RestRequest("api", Method.GET);
            req2.AddParameter("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            req2.AddParameter("module", "account");
            req2.AddParameter("action", "tokenbalance");
            req2.AddParameter("contractaddress", Statics.TOKEN_KAE);
            req2.AddParameter("address", Statics.ADDRESS_TEAM2);
            req2.AddParameter("tag", "latest");
            req2.AddParameter("apikey", config.EtherscanToken);
            var res2 = await client.ExecuteAsync<EsToken>(req2);
            teamAmt += BigInteger.Parse(res2.Data.Result);

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

        private async Task GetInfo()
        {

            GetPrices();

            while (data == null)
                await Task.Delay(200);

        }

        private async void GetPrices()
        {
            try
            {
                var client = CoinGeckoClient.Instance;
                data = await client.CoinsClient.GetAllCoinDataWithId("kanpeki", "false", true, true, false, false, false);

                log.Info($"GetPrices: OK");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
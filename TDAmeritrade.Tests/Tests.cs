using NUnit.Framework;
using System.Threading.Tasks;

namespace TDAmeritrade.Tests
{
    public class Tests
    {
        TDAmeritradeClient client;


        [SetUp]
        public async Task Init()
        {
            // Please sign in first, following services uses the client file
            var cache = new TDUnprotectedCache();
            client = new TDAmeritradeClient(cache);
            await client.PostRefreshToken();
            Assert.IsTrue(client.IsSignedIn);
        }

        [Test]
        public async Task TestTDQuoteClient_Equity()
        {
            var data = await client.GetQuote<TDEquityQuote>("MSFT");
            Assert.IsTrue(data.symbol == "MSFT");
        }

        [Test]
        public async Task TestTDQuoteClient_Index()
        {
            var data = await client.GetQuote<TDIndexQuote>("$SPX");
            Assert.IsTrue(data.symbol == "$SPX");
        }

        [Test]
        public async Task TestTDQuoteClient_Future()
        {
            var data = await client.GetQuote<TDFundQuote>("/ES");
            Assert.IsTrue(data.symbol == "ES");
        }

        [Test]
        public async Task TestTDQuoteClient_Option()
        {
            var data = await client.GetQuote<TDOptionQuote>("SPY_231215C500");
            Assert.IsTrue(data.symbol == "SPY_231215C500");
        }

        [Test]
        public async Task TestPriceHistory()
        {
            var data = await client.GetPriceHistory(new TDPriceHistoryRequest
            {
                symbol = "SPY",
                frequencyType = TDPriceHistoryRequest.FrequencyType.minute,
                frequency = 5,
                periodType = TDPriceHistoryRequest.PeriodTypes.day,
                period = 2,
            });

            Assert.IsTrue(data.Length > 0);
        }

        [Test]
        public async Task TestOptionChain()
        {
            var data = await client.GetOptionsChain(new TDOptionChainRequest
            {
                symbol = "SPY",
            });
        }

        [Test]
        public async Task TestTDPrincipalClient()
        {
            var data = await client.GetPrincipals(TDPrincipalsFields.preferences, TDPrincipalsFields.streamerConnectionInfo, TDPrincipalsFields.streamerSubscriptionKeys);
            Assert.IsTrue(!string.IsNullOrEmpty(data.accessLevel));
        }

        [Test]
        public async Task TestRealtimeStream()
        {
            await client.PostRefreshToken();
            using (var socket = new TDAmeritradeStreamClient(client))
            {
                await socket.Connect();
                var symbol = "SPY";
                await socket.SubscribeQuote(symbol);
                await socket.SubscribeChart(symbol, TDAmeritradeClient.IsFutureSymbol(symbol) ? TDChartSubs.CHART_FUTURES : TDChartSubs.CHART_EQUITY);
                await socket.SubscribeTimeSale(symbol, TDAmeritradeClient.IsFutureSymbol(symbol) ? TDTimeSaleServices.TIMESALE_FUTURES : TDTimeSaleServices.TIMESALE_EQUITY);
                await Task.Delay(1000);
                Assert.IsTrue(socket.IsConnected);
                await socket.Disconnect();
            }
        }
    }
}
using CoffeeSlotMachine.Core;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
            {
                applicationDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.Migrate();
            }
        }


        [TestMethod()]
        public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
        {
            var controller = new OrderController(new UnitOfWork());
            var depot = controller.GetCoinDepot().ToArray();
            Assert.AreEqual(6, depot.Count(), "Sechs Münzarten im Depot");
            foreach (var coin in depot)
            {
                Assert.AreEqual(3, coin.Amount, "Je Münzart sind drei Stück im Depot");
            }
            int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
            Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
        }

        [TestMethod()]
        public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
        {
            var statisticsController = new OrderController(new UnitOfWork());
            var products = statisticsController.GetProducts().ToArray();
            Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
            Assert.AreEqual("Cappuccino", products[0].Name);
            Assert.AreEqual("Ristretto", products[8].Name);
        }

        [TestMethod()]
        public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
        {
            UnitOfWork unitOfWork = new UnitOfWork();
            OrderController controller = new OrderController(unitOfWork);
            var products = controller.GetProducts();
            var product = products.Single(p => p.Name == "Cappuccino");
            var order = controller.OrderCoffee(product);
            bool isFinished = controller.InsertCoin(order, 100);
            Assert.AreEqual(true, isFinished, "100 Cent genügen");
            Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
            Assert.AreEqual(0, order.DonationCents);
            Assert.AreEqual("20;10;5", order.ReturnCoinValues);
            // Depot überprüfen
            var coins = controller.GetCoinDepot().ToArray();
            int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
            Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
            Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());
            var orders = unitOfWork.Orders.GetAllWithProduct().ToArray();
            Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
            Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
            Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
            Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
        }

        [TestMethod()]
        public void T04_BuyOneCoffee_ExactTHrowInOneCoin_CheckCoinsAndOrders()
        {
            UnitOfWork unitOfWork = new UnitOfWork();
            OrderController controller = new OrderController(unitOfWork);
            var products = controller.GetProducts();
            var product = products.Single(p => p.Name == "Latte");
            var order = controller.OrderCoffee(product);
            bool isFinished = controller.InsertCoin(order, 50);
            Assert.AreEqual(true, isFinished, "50 Cent stimmen genau");
            Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(0, order.ReturnCents);
            Assert.AreEqual(0, order.DonationCents);
            Assert.AreEqual("", order.ReturnCoinValues);
            // Depot überprüfen
            var coins = controller.GetCoinDepot().ToArray();
            int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
            Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents für Latte");
            Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());
            var orders = unitOfWork.Orders.GetAllWithProduct().ToArray();
            Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
            Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
            Assert.AreEqual(50, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
            Assert.AreEqual("Latte", orders[0].Product.Name, "Produktname Latte");
        }

        [TestMethod()]
        public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
        {
            UnitOfWork unitOfWork = new UnitOfWork();
            OrderController controller = new OrderController(unitOfWork);
            var products = controller.GetProducts();
            var product = products.Single(p => p.Name == "Cappuccino");
            var order = controller.OrderCoffee(product);
            bool isFinished = controller.InsertCoin(order, 10);
            Assert.AreEqual(false, isFinished, "10 Cent genügen nicht");
            Assert.AreEqual(10, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual("10", order.ThrownInCoinValues);
            isFinished = controller.InsertCoin(order, 10);
            Assert.AreEqual(false, isFinished, "20 Cent genügen nicht");
            Assert.AreEqual(20, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual("10;10", order.ThrownInCoinValues);
            isFinished = controller.InsertCoin(order, 20);
            Assert.AreEqual(false, isFinished, "40 Cent genügen nicht");
            Assert.AreEqual(40, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual("10;10;20", order.ThrownInCoinValues);
            isFinished = controller.InsertCoin(order, 5);
            Assert.AreEqual(false, isFinished, "45 Cent genügen nicht");
            Assert.AreEqual(45, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual("10;10;20;5", order.ThrownInCoinValues);
            Assert.AreEqual("3*200 + 3*100 + 3*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());
            isFinished = controller.InsertCoin(order, 50);
            Assert.AreEqual(true, isFinished, "95 Cent genügen");
            Assert.AreEqual(95, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual("10;10;20;5;50", order.ThrownInCoinValues);
            Assert.AreEqual(95 - product.PriceInCents, order.ReturnCents);
            Assert.AreEqual(0, order.DonationCents);
            Assert.AreEqual("20;10", order.ReturnCoinValues);
            // Depot überprüfen
            var coins = controller.GetCoinDepot().ToArray();
            int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
            Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
            Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 4*10 + 4*5", controller.GetCoinDepotString());
        }


        [TestMethod()]
        public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
        {
            UnitOfWork unitOfWork = new UnitOfWork();
            OrderController controller = new OrderController(unitOfWork);
            var products = controller.GetProducts().ToArray();
            var product = products.Single(p => p.Name == "Cappuccino");
            var order = controller.OrderCoffee(product);
            bool isFinished = controller.InsertCoin(order, 100);
            Assert.IsTrue(isFinished);
            Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());
            product = products.Single(p => p.Name == "Latte");
            order = controller.OrderCoffee(product);
            isFinished = controller.InsertCoin(order, 100);
            Assert.IsTrue(isFinished);
            Assert.AreEqual("3*200 + 5*100 + 2*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());
            product = products.Single(p => p.Name == "Cappuccino");
            order = controller.OrderCoffee(product);
            isFinished = controller.InsertCoin(order, 200);
            Assert.IsTrue(isFinished);
            Assert.AreEqual("4*200 + 4*100 + 2*50 + 1*20 + 1*10 + 1*5", controller.GetCoinDepotString());
            Assert.AreEqual(0, order.DonationCents);
        }


        [TestMethod()]
        public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
        {
            UnitOfWork unitOfWork = new UnitOfWork();
            OrderController controller = new OrderController(unitOfWork);
            var products = controller.GetProducts().ToArray();
            var product = products.Single(p => p.Name == "Cappuccino");
            var order = controller.OrderCoffee(product);
            bool isFinished = controller.InsertCoin(order, 200);
            Assert.IsTrue(isFinished);
            Assert.AreEqual("4*200 + 2*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());
            product = products.Single(p => p.Name == "Cappuccino");
            order = controller.OrderCoffee(product);
            isFinished = controller.InsertCoin(order, 200);
            Assert.IsTrue(isFinished);
            Assert.AreEqual("5*200 + 1*100 + 3*50 + 1*20 + 1*10 + 1*5", controller.GetCoinDepotString());
            product = products.Single(p => p.Name == "Cappuccino");
            order = controller.OrderCoffee(product);
            isFinished = controller.InsertCoin(order, 200);
            Assert.IsTrue(isFinished);
            Assert.AreEqual("6*200 + 0*100 + 3*50 + 0*20 + 0*10 + 0*5", controller.GetCoinDepotString());
            Assert.AreEqual(0, order.DonationCents);
            product = products.Single(p => p.Name == "Cappuccino");
            order = controller.OrderCoffee(product);
            isFinished = controller.InsertCoin(order, 200);
            Assert.IsTrue(isFinished);
            Assert.AreEqual("7*200 + 0*100 + 1*50 + 0*20 + 0*10 + 0*5", controller.GetCoinDepotString());
            Assert.AreEqual(35, order.DonationCents);
        }



        /*
                                [TestMethod()]
                                public void T04_RunOutOfReturnCoins_CheckDonation()
                                {
                                    UnitOfWork unitOfWork = new UnitOfWork();
                                    StatisticsController statisticsController = new StatisticsController(unitOfWork);
                                    OrderController controller = new OrderController(unitOfWork);
                                    controller.OrderCoffee(TODO);
                                    bool isFinished = controller.ThrowInCoin(200, out _);
                                    Assert.AreEqual(true, isFinished, "200 Cent genügen");
                                    var coins = statisticsController.GetCoinDepot().ToArray();
                                    int sumOfCents = coins.Sum(coin => coin.CoinValue * coin.Amount);
                                    Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
                                    controller.OrderCoffee(TODO);
                                    isFinished = controller.ThrowInCoin(200, out sumOfCents);
                                    Assert.AreEqual(true, isFinished, "200 Cent genügen");
                                    coins = statisticsController.GetCoinDepot().ToArray();
                                    sumOfCents = coins.Sum(coin => coin.CoinValue * coin.Amount);
                                    Assert.AreEqual(1285, sumOfCents, "Beim Start sind 1155 Cents + 2* 65 Cents für Cappuccino");
                                    controller.OrderCoffee(TODO);
                                    isFinished = controller.ThrowInCoin(200, out sumOfCents);
                                    Assert.AreEqual(true, isFinished, "200 Cent genügen");
                                    coins = statisticsController.GetCoinDepot().ToArray();
                                    sumOfCents = coins.Sum(coin => coin.CoinValue * coin.Amount);
                                    Assert.AreEqual(1360, sumOfCents);
                                    controller.OrderCoffee(TODO);
                                    isFinished = controller.ThrowInCoin(200, out sumOfCents);
                                    Assert.AreEqual(true, isFinished, "200 Cent genügen");
                                    coins = statisticsController.GetCoinDepot().ToArray();
                                    sumOfCents = coins.Sum(coin => coin.CoinValue * coin.Amount);
                                    Assert.AreEqual(1450, sumOfCents);  // 25 Cent konnten nicht herausgegeben werden
                                    var orders = unitOfWork.Orders.GetAllWithProduct().ToArray();
                                    Assert.AreEqual(4, orders.Length, "Anzahl der Bestellungen stimmt nicht");
                                    Assert.AreEqual(25, orders[0].DonationCents, "Keine Spende");
                                    Assert.AreEqual("2*50+1*10", orders[0].ReturnCoinsString, "Text für Geldrückgabe");
                                }

                            */

    }
}

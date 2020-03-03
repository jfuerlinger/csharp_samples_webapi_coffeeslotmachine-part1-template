using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeSlotMachine.Core
{
    /// <summary>
    /// Verwaltet einen Bestellablauf. 
    /// </summary>
    public class OrderController
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gibt alle Produkte sortiert nach Namen zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Product> GetProducts()
        {
            var products = _unitOfWork.Products.Get();
            return products;
        }

        /// <summary>
        /// Eine Bestellung wird für das Produkt angelegt.
        /// </summary>
        /// <param name="product"></param>
        public Order OrderCoffee(Product product)
        {
            var order = new Order
            {
                ProductId = product.Id,
                Time = DateTime.Now
            };
            _unitOfWork.Orders.Insert(order);
            _unitOfWork.Save();
            return order;
        }

        /// <summary>
        /// Münze einwerfen. 
        /// Wurde zumindest der Produktpreis eingeworfen, Münzen in Depot übernehmen
        /// und für Order Retourgeld festlegen. Bestellug abschließen.
        /// </summary>
        /// <returns>true, wenn der Einwurf abgeschlossen ist</returns>
        public bool InsertCoin(Order order, int coinValue)
        {
            bool hasPaidEnough = order.InsertCoin(coinValue);
            if (hasPaidEnough)
            {
                order.FinishPayment(_unitOfWork.Coins.GetAll());
            }
            _unitOfWork.Save();
            return hasPaidEnough;
        }

        /// <summary>
        /// Gibt den aktuellen Inhalt der Kasse, sortiert nach Münzwert zurück
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Coin> GetCoinDepot()
        {
            var depot = _unitOfWork.Coins.GetOrderedDescendingByValue();
            return depot;
        }


        /// <summary>
        /// Gibt den Inhalt des Münzdepots als String zurück
        /// </summary>
        /// <returns></returns>
        public string GetCoinDepotString()
        {
            StringBuilder result = new StringBuilder();
            var coinDepot = GetCoinDepot().ToArray();
            for (int i = 0; i < coinDepot.Length; i++)
            {
                result.Append($"{coinDepot[i].Amount}*{coinDepot[i].CoinValue}");
                if (i+1 < coinDepot.Length)
                {
                    result.Append(" + ");
                }
            }
            return result.ToString();
        }

    }
}

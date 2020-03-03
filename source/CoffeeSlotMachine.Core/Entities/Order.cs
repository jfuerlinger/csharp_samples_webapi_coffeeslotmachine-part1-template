using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CoffeeSlotMachine.Core.Entities
{
    /// <summary>
    /// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
    /// die Münzen die zurückgegeben werden.
    /// </summary>
    public class Order : EntityObject
    {
        /// <summary>
        /// Datum und Uhrzeit der Bestellung
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Werte der eingeworfenen Münzen als Text. Die einzelnen 
        /// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
        /// </summary>
        public String ThrownInCoinValues { get; set; }

        /// <summary>
        /// Zurückgegebene Münzwerte mit ; getrennt
        /// </summary>
        public String ReturnCoinValues { get; set; }

        /// <summary>
        /// Summe der eingeworfenen Cents.
        /// </summary>
        public int ThrownInCents => ConvertNumbersTextToInt(ThrownInCoinValues);

        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents => ConvertNumbersTextToInt(ReturnCoinValues);


        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        public int DonationCents => ThrownInCents - Product.PriceInCents - ReturnCents;

        /// <summary>
        /// Münze wird eingenommen.
        /// </summary>
        /// <param name="coinValue"></param>
        /// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
        public bool InsertCoin(int coinValue)
        {
            ThrownInCoinValues = AddIntToNumbersText(ThrownInCoinValues, coinValue); // Münze einwerfen
            return ThrownInCents - Product.PriceInCents >= 0;
        }

        /// <summary>
        /// Hilfsmethode zur Erzeugung des Cent-Strings
        /// </summary>
        /// <param name="numbersText"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private string AddIntToNumbersText(string numbersText, int number)
        {
            if (String.IsNullOrEmpty(numbersText))
            {
                return number.ToString();
            }
            return numbersText + ";" + number;
        }

        /// <summary>
        /// Hilfsmethode zur Ermittlung der Summe aller Werte im Zahlenstring
        /// </summary>
        /// <param name="numbersText"></param>
        /// <returns></returns>
        private int ConvertNumbersTextToInt(String numbersText)
        {
            if (String.IsNullOrEmpty(numbersText))
            {
                return 0;
            }
            var numbersStrings = numbersText.Split(";");
            return numbersStrings.Sum(Convert.ToInt32);
        }

        /// <summary>
        /// Übernahme des Einwurfs in das Münzdepot.
        /// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
        /// hängt vom Inhalt der Kasse ab.
        /// </summary>
        /// <param name="coins">Aktueller Zustand des Münzdepots</param>
        public void FinishPayment(IEnumerable<Coin> coins)
        {
            // Münzen einnehmen
            coins = coins.OrderByDescending(c => c.CoinValue).ToArray();
            foreach (var coinValue in ThrownInCoinValues.Split(";").Select(x=>Convert.ToInt32(x)))
            {
                coins.Single(c => c.CoinValue == coinValue).Amount++;
            }
            int centsToReturn = ThrownInCents - Product.PriceInCents;
            // Rückgabemünzen ermitteln
            ReturnCoinValues = "";
            foreach (var coin in coins)
            {
                int coinsToReturn = centsToReturn / coin.CoinValue;  // gewünschte Anzahl an Münzen des Wertes
                coinsToReturn = Math.Min(coinsToReturn, coin.Amount); // wieviele Münzen sind im Depot
                coin.Amount -= coinsToReturn; // Münze zurückgeben
                centsToReturn -= coinsToReturn * coin.CoinValue;
                for (int i = 0; i < coinsToReturn; i++)
                {
                    ReturnCoinValues = AddIntToNumbersText(ReturnCoinValues, coin.CoinValue);
                }
                if (centsToReturn == 0) break;  // Alles zurückgegeben ==> kein weiterer Durchlauf notwendig
            }
        }
    }
}

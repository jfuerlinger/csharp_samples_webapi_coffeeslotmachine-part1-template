using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeSlotMachine.Persistence
{
    public class CoinRepository : ICoinRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CoinRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Coin> GetAll()
        {
            return _dbContext.Coins.OrderBy(coin=>coin.CoinValue).ToList();
        }

        public IEnumerable<Coin> GetOrderedDescendingByValue()
        {
            return _dbContext.Coins.OrderByDescending(coin => coin.CoinValue).ToList();
        }

        public Coin GetById(int id)
        {
            return _dbContext.Coins.Find(id);
        }

        public void Insert(Coin coin)
        {
            _dbContext.Coins.Add(coin);
        }

        public bool Delete(int id)
        {
            Coin coin = _dbContext.Coins.Find(id);
            if (coin == null)
            {
                return false;
            }
            _dbContext.Coins.Remove(coin);
            return true;
        }
    }
}

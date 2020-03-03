using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSlotMachine.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Order> GetAll()
        {
            return _dbContext.Orders.ToList();
        }

        public Order GetById(int id)
        {
            return _dbContext.Orders.FirstOrDefault(order => order.Id == id);
        }

        public void Insert(Order order)
        {
            _dbContext.Orders.Add(order);
        }

        public IEnumerable<Order> GetAllWithProduct()
        {
            return _dbContext.Orders.Include(order => order.Product)
                .OrderByDescending(order => order.Time).ToList();
        }

        public Order GetByIdWithProductAndCoins(int id)
        {
            return _dbContext.Orders
                .Include(o => o.Product)
                .SingleOrDefault(o => o.Id == id);

        }

        public bool Delete(int id)
        {
            Order order = _dbContext.Orders.Find(id);
            if (order == null)
            {
                return false;
            }
            _dbContext.Orders.Remove(order);
            return true;
        }
    }
}

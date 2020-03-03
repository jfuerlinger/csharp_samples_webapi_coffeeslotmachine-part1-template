using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSlotMachine.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Product GetByTypeName(string coffeeTypeName)
        {
            return _dbContext.Products.SingleOrDefault(product => product.Name == coffeeTypeName);
        }

        public IEnumerable<Product> GetWithOrders()
        {
            return _dbContext.Products.Include(product => product.Orders)
                .OrderBy(product => product.Name).ToList();

        }

        public IEnumerable<Product> Get()
        {
            return _dbContext.Products.OrderBy(product => product.Name).ToList();
        }

        public Product GetById(int id)
        {
            return _dbContext.Products.Find(id);
        }
    }
}

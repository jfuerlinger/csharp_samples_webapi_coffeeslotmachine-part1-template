using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface IProductRepository
    {
        Product GetByTypeName(string coffeeTypeName);
        IEnumerable<Product> GetWithOrders();
        IEnumerable<Product> Get();
        Product GetById(int id);
    }
}
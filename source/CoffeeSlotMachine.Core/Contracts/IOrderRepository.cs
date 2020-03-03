using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;

namespace CoffeeSlotMachine.Core.Contracts
{
    public interface IOrderRepository
    {
        void Insert(Order order);
        IEnumerable<Order> GetAllWithProduct();
        Order GetByIdWithProductAndCoins(int id);
    }
}
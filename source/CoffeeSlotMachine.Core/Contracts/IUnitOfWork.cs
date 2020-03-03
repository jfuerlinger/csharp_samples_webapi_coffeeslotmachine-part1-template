namespace CoffeeSlotMachine.Core.Contracts
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }

        ICoinRepository Coins { get; }

        void Save();

        void InitializeDatabase();
    }
}

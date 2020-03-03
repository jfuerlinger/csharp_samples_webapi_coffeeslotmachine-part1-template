using System.ComponentModel.DataAnnotations;

namespace CoffeeSlotMachine.Core.Entities
{
    public class EntityObject : IEntityObject
    {
        #region IEnityObject Members

        [Key]
        public virtual int Id { get; set; }

        [Timestamp]
        public virtual byte[] RowVersion
        {
            get;
            set;
        }

        #endregion
    }
}

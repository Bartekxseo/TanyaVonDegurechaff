using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TD.Domain.Entities.Abstract
{
    public abstract class Entity<TKey>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public TKey Id { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace MyCompanyName.AbpZeroTemplate.Storage
{
    [Table("AppBinaryObjects")]
    public class BinaryObject : Entity<Guid>
    {
        [Required]
        public byte[] Bytes { get; set; }

        public BinaryObject()
        {
            Id = Guid.NewGuid();
        }

        public BinaryObject(byte[] bytes)
            : this()
        {
            Bytes = bytes;
        }
    }
}

using System;

namespace MinimalAPI.DataAccess.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }
}
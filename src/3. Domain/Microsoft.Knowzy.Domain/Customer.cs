﻿using System.Collections.Generic;

namespace Microsoft.Knowzy.Domain
{
    public class Customer
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}

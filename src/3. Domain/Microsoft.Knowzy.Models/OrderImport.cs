using System.Collections.Generic;
using Microsoft.Knowzy.Domain;

namespace Microsoft.Knowzy.Models
{
    public class OrderImport
    {
        public List<Receiving> Receivings { get; set; }
        public List<Shipping> Shippings { get; set; }
    }
}

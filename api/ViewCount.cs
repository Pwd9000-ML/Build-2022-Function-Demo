using Microsoft.Azure.Cosmos.Table;

namespace Company.Function
{
    public class ViewCount : TableEntity
    {
        public ViewCount(string URL)
        {
            this.PartitionKey = URL; 
            this.RowKey = "visits"; 
            Count = 0; 
        }

        public int Count { get; set; }

        public ViewCount()
        {
            Count = 0;
        }
    }
}

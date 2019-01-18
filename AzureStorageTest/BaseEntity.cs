using AzureStorageTest;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.DataAccess.Base
{
   public class BaseEntity:TableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class Book:BaseEntity, IAuditTracker
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public Book() { }

        public Book(int bookId,string publisher)
        {
            this.RowKey = bookId.ToString() ;
            this.PartitionKey = publisher;
        }
    }
}

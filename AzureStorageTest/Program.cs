using ASC.DataAccess;
using ASC.DataAccess.Base;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace AzureStorageTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Task.Run(async () =>
            {
                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();

                    Book book = new Book()
                    {
                        Author = "Rami",
                        BookName = "ASP.NET Core With Azure",
                        Publisher = "APress"
                    };

                    book.BookId = new Random().Next(1,1000);
                    book.RowKey = book.BookId.ToString();
                    book.PartitionKey = book.Publisher;

                    var data = await bookRepository.AddAsync(book);

                    _unitOfWork.CommitTransaction();

                    Console.WriteLine(data);
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    var data = await bookRepository.FindAsync("APress", "1");
                    Console.WriteLine(data);
                    data.Author = "Rami Vemula";
                    var updatedData = await bookRepository.UpdateAsync(data);
                    Console.WriteLine(updatedData);
                    _unitOfWork.CommitTransaction();
                }

                using (var _unitOfWork = new UnitOfWork("UseDevelopmentStorage=true;"))
                {
                    var bookRepository = _unitOfWork.Repository<Book>();
                    await bookRepository.CreateTableAsync();
                    var data = await bookRepository.FindAsync("APress", "1");
                    Console.WriteLine(data);

                    await bookRepository.DeleteAsync(data);
                    Console.WriteLine("Deleted");

                    // Throw an exception to test rollback actions
                    //  throw new Exception();

                    _unitOfWork.CommitTransaction();
                }

            }).GetAwaiter().GetResult();

            Console.ReadLine();






        }
    }
}

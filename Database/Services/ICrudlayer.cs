
using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;
using static Database.DatabaseLogic.DbBookCrud;


namespace Database.Services;

public interface ICrudlayer 
{
    Task Testapi();

    Task ConcurTest(int delay,int qnty);




    //Task<decimal?> Getpricebooks(List<BookItemList> bookitem);


    //Task<Model.PaymentDetails?> GetInvoicebooks(List<Model.ModelsDto.Paymentmodels.BookItemList> bookitem);
    //Task<BookPaymentModel?> GetInvoicebooks(List<BookItemList> bookitem);



    //Task<bool> DeleteAccount(Guid? userid);
    Task<bool> UpdateTier(Guid userID, Subscription subtier ,HttpClient client);
    Task<bool> Pricebookset(string isbn, decimal price,CancellationToken token = default);
    Task<bool> DeleteAccount<T>(T value);
    Task<bool> ChangeRoles(Guid? UserID, string? email, UserRole role);
    Task UpdateOrderStatus(int OrderID, Status Ordertatus);
    Task<(PaymentDetails?, int?)> GetInvoicebooks(List<Model.ModelsDto.PaymentPartialmodels.BookItemList> BooksList , Guid GuidID);

    //Task<MarketDataAPIModelbyISBN?> ApiServiceGetbyisbn(string isbn, Guid userID, UserRole role, CancellationToken token = default);
    Task<(MarketDataAPIModelbyISBN?, Respostebookapi?)> ApiServiceGetbyisbn(string isbn, Guid apikey, CancellationToken token = default);
    //Task<MarketDataAPIModelbyISBN?> ApiServiceGetbyisbn(string isbn, Guid apikey, CancellationToken token = default);

    Task<List<DetailedFilterBookModel>> Filteredquery(QuerySelector selector, CancellationToken token = default);
    Task<List<BooksCatalog>> RawReturn(int page , int pagesize,CancellationToken token = default);
    Task<bool> Registration(Registration regi, CancellationToken token = default);
    Task<(string?, string?)> Login(Login login, CancellationToken token = default);

    Task<bool> Deletebyisbn(string isbn, CancellationToken token = default);

    Task<bool> AddOrOverrideStockQuantitybyISBN(string isbn, int qnty, bool Forcerewrite, CancellationToken token = default);

    Task<Respostebookapi> InsertBookItem(BookinsertModel datamodel);

}

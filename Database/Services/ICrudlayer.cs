
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;


namespace Database.Services;

public interface ICrudlayer 
{


    Task ConcurTest(int delay,int qnty);
    Task Testapi();



    //Task<decimal?> Getpricebooks(List<BookItemList> bookitem);


    //Task<Model.PaymentDetails?> GetInvoicebooks(List<Model.ModelsDto.Paymentmodels.BookItemList> bookitem);
    //Task<BookPaymentModel?> GetInvoicebooks(List<BookItemList> bookitem);

    Task<PaymentDetails?> GetInvoicebooks(List<Model.ModelsDto.PaymentPartialmodels.BookItemList> BooksList);
    Task<MarketDataAPIModelbyISBN?> Getbyisbn(string isbn, CancellationToken token = default);
    Task<List<DetailedFilterBookModel>> Filteredquery(QuerySelector selector, CancellationToken token = default);
    Task<List<BooksCatalog>> RawReturn(int page , int pagesize,CancellationToken token = default);
    Task<bool> Registration(Registration regi, CancellationToken token = default);
    Task<(string?, string?)> Login(Login login, CancellationToken token = default);

    Task<bool> Deletebyisbn(string isbn, CancellationToken token = default);

    Task<bool> AddOrOverrideStockQuantitybyISBN(string isbn, int qnty, bool Forcerewrite, CancellationToken token = default);

    Task<Respostebookapi> InsertBookItem(BookinsertModel datamodel);

}

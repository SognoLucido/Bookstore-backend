using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Services;

public interface ICrudlayer 
{


    Task ConcurTest(int delay,int qnty);
    Task Testapi();
    Task<MarketDataAPIModelbyISBN?> Getbyisbn(string isbn, CancellationToken token = default);
    Task<List<DetailedFilterBookModel>> Filteredquery(QuerySelector selector, CancellationToken token = default);
    Task<List<BooksCatalog>> RawReturn(int page , int pagesize,CancellationToken token = default);
    Task<bool> Registration(Registration regi, CancellationToken token = default);
    Task<(string?, string?)> Login(Login login, CancellationToken token = default);

    Task<bool> Deletebyisbn(string isbn, CancellationToken token = default);

    Task<bool> AddOrOverrideStockQuantitybyISBN(string isbn, int qnty, bool Forcerewrite, CancellationToken token = default);

    Task<string> InsertBookItem(BookinsertModel datamodel);

}

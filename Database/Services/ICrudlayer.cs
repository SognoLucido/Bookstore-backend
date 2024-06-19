using Database.Model;
using Database.Model.Apimodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Services;

public interface ICrudlayer 
{




    Task<MarketDataAPIModelbyISBN?> Getbyisbn(string isbn, CancellationToken token = default);
    Task<List<DetailedFilterBookModel>> Filteredquery(QuerySelector selector, CancellationToken token = default);
    Task<List<BooksCatalog>> RawReturn(int page , int pagesize,CancellationToken token = default);


}

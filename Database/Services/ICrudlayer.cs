﻿
using Database.Model;
using Database.Model.Apimodels;
using Database.Model.ModelsDto;
using Database.Model.ModelsDto.Paymentmodels;



namespace Database.Services;

public interface ICrudlayer 
{
  

    Task<List<DetailedFilterBookModel>> SearchItems(int? limit, (string? Booktitle, string? Authorname, string? Category) Tupledata,CancellationToken ctoken = default);
    Task<List<AuthorDto>> GetAuthorinfo(int? limit, string? search);
    Task<List<Category>> GetCategoriesinfo(int? limit, string? search);
    Task<UserInfo?> GetUserInfoAccount(Guid UserID);
    Task<bool> UpdateTier(Guid userID, Subscription subtier ,HttpClient client);
    Task<bool> Pricebookset(string isbn, decimal price,CancellationToken token = default);
    Task<bool> DeleteAccount<T>(T value);
    Task<bool> ChangeRoles(Guid? UserID, string? email, UserRole role);
    Task UpdateOrderStatus(int OrderID, Status Ordertatus);
    Task<(PaymentDetails?, int?)> GetInvoicebooks(List<Model.ModelsDto.PaymentPartialmodels.BookItemList> BooksList , Guid GuidID);
    Task<(MarketDataAPIModelbyISBN?, Respostebookapi?)> ApiServiceGetbyisbn(string isbn, Guid apikey, CancellationToken token = default);
    Task<List<DetailedFilterBookModel>> Filteredquery(QuerySelector selector, CancellationToken token = default);
    Task<List<BooksCatalog>> RawReturn(int page , int pagesize,CancellationToken token = default);
    Task<bool> Registration(Registration regi, CancellationToken token = default);
    Task<(string?, string?)> Login(Login login, CancellationToken token = default);
    Task<bool> Deletebyisbn(string isbn, CancellationToken token = default);
    Task<bool> AddOrOverrideStockQuantitybyISBN(string isbn, int qnty, bool Forcerewrite, CancellationToken token = default);
    Task<(List<DetailedFilterBookModel>?,Respostebookapi?)> InsertBooksItem(List<BookinsertModel> datamodel);
    Task<bool> UpinsertAuthorsxCategories(CategoryandAuthorDto data, bool AuthorUpinsert);
    Task<ItemModelGroup> SingleItemSearch(string name, Item item);
    Task<OrdersInfoDto?> GetOrdersinfo(Guid userid,DateOnly start,DateOnly end);

}

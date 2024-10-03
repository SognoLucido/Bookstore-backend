using Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Api.Integration.test.Model
{
    public record Tokenlogin(string result);
    public record Data(string firstname, string lastname ,string email);


    public record UserInfoOnlykey (Apikey apiInfo);
    public record Apikey (string apikey);

    public record Isbn (string isbn);


}

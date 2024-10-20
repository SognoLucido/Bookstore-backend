﻿using Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Api.Integration.test.Model
{
    public record Tokenlogin(string result);
    public record Data(string firstname, string lastname ,string email);
    public record Role(string role);
    public record Isbn(string isbn);
    public record FUllname ( string fullname);
    public record Categoryname(string name);  
    public record Email(string email);
    public record RolebyEmail (string email, string role);
    public record RolebyUserID ( string userID , string role);

    public record UserInfoOnlykey (Apikey apiInfo);
    public record SubtierInfo (Subscription apiInfo);
    public record Apikey (string apikey);
    public record Subscription(string subscriptionTier);


    public class AuthorNCategoryOnlyNames
    {
       public List<string> Authors { get; set; }
       public  List<string> Category { get; set; }
    }
  



}

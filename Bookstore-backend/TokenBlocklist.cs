namespace Bookstore_backend
{
    public class TokenBlocklist // redis ?
    {
        //temp block list
        public HashSet<string> TokenList { get; /*private*/ set; } = [];
        
        //<securitytoken>

        public void TokenInsert (string token) => TokenList.Add(token);     
        public bool TokenListCheck (string token) => TokenList.Contains(token);
       

    }
}

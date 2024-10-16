namespace Bookstore_backend
{
    public class TokenBlocklist // redis ?
    {
        //temp block list
        private HashSet<string> TokenList { get;  set; } = [];
        
        //<securitytoken>

        public void TokenInsert (string token) => TokenList.Add(token);     
        public bool TokenListCheck (string token) => TokenList.Contains(token);
       

    }
}

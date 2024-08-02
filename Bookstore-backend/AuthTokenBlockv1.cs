using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Bookstore_backend
{
    public class AuthTokenBlockv1 : JwtBearerEvents
    {

        //public override Task MessageReceived(MessageReceivedContext context)
        //{
        //    var x = context.Token;


        //    return base.MessageReceived(context);
        //}

        private readonly TokenBlocklist block;



        public AuthTokenBlockv1(TokenBlocklist tokenblock)
        {
            block = tokenblock;
        }


        public override  Task TokenValidated(TokenValidatedContext context)
        {


            

            string z = ((Microsoft.IdentityModel.JsonWebTokens.JsonWebToken)context.SecurityToken).EncodedSignature;

            if (block.TokenListCheck(z)) 
            {
                context.Fail("Token Expired");
            }
       


                   
            return Task.CompletedTask;


            //return base.Forbidden(context);
            //return base.TokenValidated(context);
        }

    }
}

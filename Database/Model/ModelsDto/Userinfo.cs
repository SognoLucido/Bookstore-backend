using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model.ModelsDto
{
    public record UserInfo
        (
            string Firstname,
            string Lastname,
            string Email,
            string Phone,
            string Role,
            Apiinfo ApiInfo
        );
   

    public record Apiinfo
        (
        string Apikey,
        string SubscriptionTier,
        int Apicalls

        );


    //public static class Enumconverter
    //{
    //    public static string EnumTostring(Subscription sub)
    //    {
    //        return sub switch
    //        {
    //            Subscription.Tier0 => "Tier0",
    //            Subscription.Tier1 => "Tier1",
    //            Subscription.Tier2 => "Tier2",
    //            _ => "wtf",
    //        };
    //    }
    //}
   

}

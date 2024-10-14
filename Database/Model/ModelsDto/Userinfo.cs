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


    public record UserInfoWithID
        (
             Guid UserID,
             string Firstname,
             string Lastname,
             string Email,
             string Phone,
             string Role,
             Apiinfo ApiInfo
        );


}

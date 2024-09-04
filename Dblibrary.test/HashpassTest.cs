using Database.DatabaseLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dblibrary.test
{
    public class HashpassTest
    {
        //[Fact]
        //public void Testinghash256pass()
        //{
        //    Passhasher hasher = new();

        //    string expected = "51231b21a8c9215629fb3effabf5afbe7b7ce4af9425f5e694883baee43ab4f8";

        //    string actual = hasher.HashpasstoDb("home");

        //    Assert.Equal(expected, actual);

        //}



        [Theory]
        [InlineData("home", "dYVDYcoG", "51231b21a8c9215629fb3effabf5afbe7b7ce4af9425f5e694883baee43ab4f8")]
        public async Task Testinghash256pass2(string pass,string salt,string hashedpassplussalt)
        {
            Passhasher hasher = new();

            string actual = await hasher.HashAlgorithm(pass,salt);

            Assert.Equal(hashedpassplussalt, actual);

        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Services
{
    public interface IpassHash
    {
        public Task<(string,string)> HashpasstoDb(string pass);

        Task<string> HashAlgorithm(string pass, string salt);
    }
}

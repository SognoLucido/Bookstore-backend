using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace Database.DatabaseLogic
{
    //using sha256 for a fast implementation
    public class Passhasher
    {


        public string HashpasstoDb(string pass)
        {
            Guid guid = Guid.NewGuid();
            string salt = guid.ToString().Substring(0, 8);
   

            return HashAlgoritm(pass,salt);
        }


        public string HashAlgoritm(string pass ,string salt)
        {
           

            using (SHA256 sha256Hash = SHA256.Create())
            {
                
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(pass+salt));

               
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return  builder.ToString();
            }

        }


    }
}

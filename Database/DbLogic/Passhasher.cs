using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Database.DatabaseLogic
{

    public class Passhasher
    {


        public void HashpasstoDb(string pass)
        {
            Guid guid = Guid.NewGuid();
            
            string salt = guid.ToString().Substring(0, 8);

           
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute the hash from the raw data
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(salt));

                // Convert the byte array to a string of hexadecimal characters
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                var x =  builder.ToString();
            }

        }

      




    }
}

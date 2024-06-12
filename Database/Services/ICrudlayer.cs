using Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Services;

public interface ICrudlayer 
{

    Task<Person> Getbyid(int id);
    Task Insert(Person person);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Models
{
    public class MapperCreation
    {
        public void CreateMaps()
        {
            AutoMapper.Mapper.CreateMap<User, User>();
        }
    }    
}

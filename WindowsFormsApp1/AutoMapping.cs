using AutoMapper;
using DAL;
using DAL.DTO;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertToolApp
{
  
        public static class AutoMapping
        {
            public static void RegisterMapping()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<ExportConstract, ExportContractExcel>().ReverseMap();

                });

            }
  
    }
}

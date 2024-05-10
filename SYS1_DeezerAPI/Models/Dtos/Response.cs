using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Models.Dtos
{
    public class Response
    {
        public List<Track> Data { get; set; } = null!;
        public int Total { get; set; }
    }
}

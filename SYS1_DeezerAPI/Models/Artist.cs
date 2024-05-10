using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Models
{
    public class Artist
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Link { get; set; } //url
        public string? Picture { get; set; } //url
        public string? Picture_small { get; set; } //url
        public string? Picture_medium { get; set; } //url
        public string? Picture_big { get; set; } //url
        public string? Picture_xl { get; set; } //url
        public string? Tracklist { get; set; }
    }
}

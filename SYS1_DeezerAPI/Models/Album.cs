using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Models
{
    public class Album
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public string? Cover { get; set; } //url
        public string? Cover_small { get; set; } //url
        public string? Cover_medium { get; set; } //url
        public string? Cover_big { get; set; } //url
        public string? Cover_xl { get; set; } //url
        public string? Md5_image { get; set; }
        public string? Tracklist { get; set; } //url
    }
}

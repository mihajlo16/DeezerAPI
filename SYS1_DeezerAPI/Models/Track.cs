using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Models
{
    public class Track
    {
        public long? Id { get; set; }
        public bool? Readable { get; set; }
        public string? Title { get; set; }
        public string? Title_short { get; set; }
        public string? Title_version { get; set; }
        public string? Link { get; set; } //url
        public int? Duration { get; set; }
        public int? Rank { get; set; }
        public bool? Explicit_lyrics { get; set; }
        public int? Explicit_content_lyrics { get; set; }
        public int? Explicit_content_cover { get; set; }
        public string? Preview { get; set; } //url
        public string? Md5_image { get; set; }
        public Artist? Artist { get; set; }
        public Album? Album { get; set; }
    }
}

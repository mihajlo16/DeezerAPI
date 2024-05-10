using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYS1_DeezerAPI.Models.Dtos
{
    public class TrackQueryParameters(NameValueCollection queryParams)
    {
        public string? Artist { get; set; } = queryParams["artist"];
        public string? Album { get; set; } = queryParams["album"];
        public string? Track { get; set; } = queryParams["track"];
        public string? Label { get; set; } = queryParams["label"];
        public string? Dur_min { get; set; } = queryParams["dur_min"];
        public string? Dur_max { get; set; } = queryParams["dur_max"];
        public string? Bpm_min { get; set; } = queryParams["bpm_min"];
        public string? Bpm_max { get; set; } = queryParams["bpm_max"];

        public override string ToString()
        {
            var properties = this.GetType().GetProperties();
            var stringBuilder = new StringBuilder();

            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (value != null)
                {
                    stringBuilder.Append(property.Name)
                                 .Append(": ")
                                 .Append(value)
                                 .Append(", ");
                }
            }

            stringBuilder.Length -= 2;

            return stringBuilder.ToString();
        }
    }
}
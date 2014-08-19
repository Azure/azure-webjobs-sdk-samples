using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhluffyShuffyWeb.Models
{
    public class ImageModel
    {
        public string Id { get; set; }

        public bool IsReadonly { get; set; }

        public IEnumerable<Uri> ImageParts { get; set; }
    }
}
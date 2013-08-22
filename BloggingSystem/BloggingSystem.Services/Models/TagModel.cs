using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BloggingSystem.Services.Models
{
    [DataContract]
    public class TagModel
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
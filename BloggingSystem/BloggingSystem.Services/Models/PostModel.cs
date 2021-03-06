﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BloggingSystem.Services.Models
{
    // trqbva da dobavqm vsi4ki dumi ot imeto kato tagove
    [DataContract]
    public class PostModel
    {
        //        { "title": "NEW POST",
        //"tags": ["post"],
        //"text": "this is just a test post" }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "postDate")]
        public DateTime PostDate { get; set; }

        [DataMember(Name = "postedBy")]
        public string PostedBy { get; set; }
        

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "tags")]
        public IEnumerable<string> Tags { get; set; }

        [DataMember(Name = "comments")]
        public IEnumerable<CommentModel> Comments { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MALClient.Models
{
    public class AnimeReviewData
    {
        public string Review { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string AuthorAvatar { get; set; }
        public string OverallRating { get; set; }
        public string EpisodesSeen { get; set; }
        public string HelpfulCount { get; set; }

        //For UI purpose
        public string Header => Author;
    }
}

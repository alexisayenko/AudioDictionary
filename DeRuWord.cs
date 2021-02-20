using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDictionary
{
    class DeRuWord : IWord
    {
        public string GermanArticle { get; set; }
        public string German { get; set; }
        public string Russian { get; set; }
        public bool HasAudio { get; set; }
    }
}

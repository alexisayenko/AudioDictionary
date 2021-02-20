using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDictionary
{
    class EnRuWord : IWord
    {
        public string English { get; set; }
        public string Russian { get; set; }
        public bool HasAudio { get; set; }
    }
}

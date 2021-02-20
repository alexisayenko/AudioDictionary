using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDictionary
{
    class DeWord : IWord
    {
        public string SingularArticle { get; set; }
        public string SingularForm { get; set; }
        public string PluralArticle { get; set; }
        public string PluralForm { get; set; }
    }
}

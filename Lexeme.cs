using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioDictionary
{
    class Lexeme
    {
        public Lexeme(ILanguage language, string word)
        {
            Language = language;
            Word = language.TryToSeparateArticle(word.Trim(), out string article);
            Article = article;
        }

        public ILanguage Language { get; private set; }
        public string Article { get; private set; }
        public string Word { get; private set; }
        public bool HasArticle => string.IsNullOrEmpty(Article);
        public bool HasArticleAudio { get; set; }
        public string WordNormalized => Word.Replace(' ', '-');

        public bool HasWordAudio { get; set; }

        public override string ToString()
        {
            return Word;
        }
    }
}

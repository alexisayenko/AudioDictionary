using System;
using System.Collections.Generic;

namespace AudioDictionary
{
    internal class LanguageDe : ILanguage
    {
        private readonly Dictionary<string, string> articlesUrlsDictionary;

        public LanguageDe()
        {
            articlesUrlsDictionary = new Dictionary<string, string>
            {
                //{"der", "https://upload.wikimedia.org/wikipedia/commons/5/53/De-der.ogg" },
                //{"die", "https://upload.wikimedia.org/wikipedia/commons/9/9b/De-die.ogg" },
                //{"das", "https://upload.wikimedia.org/wikipedia/commons/e/e6/De-das.ogg" }
                {"der", "https://upload.wikimedia.org/wikipedia/commons/0/01/De-der2.ogg" },
                {"die", "https://upload.wikimedia.org/wikipedia/commons/7/76/De-die2.ogg" },
                {"das", "https://upload.wikimedia.org/wikipedia/commons/e/e6/De-das.ogg" }
            };
        }

        public string[] WikiBaseUrls => new[]
        {
            "https://de.wiktionary.org/wiki",
            "https://en.wiktionary.org/wiki"
        };

        public string GetArticleUrl(string article)
        {
            if (string.IsNullOrEmpty(article))
                return default;

            if (articlesUrlsDictionary.TryGetValue(article, out var url))
                return url;

            return default;
        }

        public string GetLanguageSpecificDcitionaryUrl(string word) => default;

        public string TryToSeparateArticle(string word, out string article)
        {
            foreach (var key in this.articlesUrlsDictionary.Keys)
            {
                if (!word.StartsWith(key))
                    continue;

                article = key;
                return word.Substring(key.Length + 1);
            }

            //if (word.StartsWith("der", StringComparison.OrdinalIgnoreCase))
            //{
            //    article = "der";
            //    return word.Substring(4);
            //}

            //if (word.StartsWith("die", StringComparison.OrdinalIgnoreCase))
            //{
            //    article = "die";
            //    return word.Substring(4);
            //}

            //if (word.StartsWith("das", StringComparison.OrdinalIgnoreCase))
            //{
            //    article = "das";
            //    return word.Substring(4);
            //}

            article = default;
            return word;
        }
    }
}
namespace AudioDictionary
{
    class WordPair
    {
        public WordPair(ILanguage language1, string word1, ILanguage language2, string word2)
        {
            Language1 = language1;
            Word1 = language1.TryToSeparateArticle(word1.Trim(), out string article1);
            Article1 = article1;

            Language2 = language2;
            Word2 = language2.TryToSeparateArticle(word2.Trim(), out string article2);
            Article2 = article2;
        }

        public ILanguage Language1 { get; private set; }
        public ILanguage Language2 { get; private set; }

        public bool HasAudio { get; set; }
        public string Article1 { get; private set; }
        public string Word1 { get; private set; }
        public string Article2 { get; private set; }
        public string Word2 { get; private set; }
        public bool HasArticle1 => string.IsNullOrEmpty(Article1);
        public bool HasArticle2 => string.IsNullOrEmpty(Article2);
        public bool HasArticle1Audio { get; set; }
        public bool HasArticle2Audio { get; set; }
    }
}

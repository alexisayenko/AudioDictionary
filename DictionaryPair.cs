namespace AudioDictionary
{
    class DictionaryPair
    {
        public DictionaryPair(ILanguage language1, string word1, ILanguage language2, string word2)
        {
            Lexeme1 = new Lexeme(language1, word1);
            Lexeme2 = new Lexeme(language2, word2);
        }

        public Lexeme Lexeme1 { get; set; }
        public Lexeme Lexeme2 { get; set; }

        public bool HasAudio => 
            Lexeme1.HasWordAudio && Lexeme2.HasWordAudio;
    }
}

﻿namespace AudioDictionary
{
    interface ILanguage
    {
        string[] WikiBaseUrls { get; }

        /// <summary>
        /// Use this method to get URL of online dictionary specifc to Word1.
        /// You should use the dictionary that suits the language of the word better than wikitionary. 
        /// For example, Oxford dictionary for English words.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        string GetLanguageSpecificDcitionaryUrl(string word);
    }
}
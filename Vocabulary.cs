using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AudioDictionary
{
    abstract class Vocabulary
    {
        public VocabularyType Type { get; private set; }

        //public Vocabulary(VocabularyType type)
        //{
        //    this.Type = type;
        //}

        public string WorkingDirectory => Environment.WorkingDirectory;
        public string Silence05sec { get; set; }

        public static Vocabulary Create(VocabularyType type, string pauseFileName)
        {
            switch (type)
            {
                case VocabularyType.RuEnTranslation:
                    return new EnRuVocabulary(pauseFileName);
                case VocabularyType.EnRuTranslation:
                    break;
                case VocabularyType.DeRuTranslation:
                    break;
                case VocabularyType.RuDeTranslation:
                    break;
                case VocabularyType.DeSingularPlural:
                    break;
                default:
                    break;
            }

            return null;
        }

        internal abstract void GenerateAudioFile();
    }
}

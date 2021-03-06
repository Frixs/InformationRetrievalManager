﻿using InformationRetrievalManager.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace InformationRetrievalManager.NLP
{
    /// <summary>
    /// Basic index processing
    /// </summary>
    public sealed class IndexProcessing
    {
        #region Constants

        /// <summary>
        /// Alphabet with diacritics
        /// </summary>
        private const string _withDiacritics = "áÁäÄčČćĆďĎéÉěĚíÍňŇńŃóÓöÖřŘŕŔšŠśŚťŤúÚůŮüÜýÝžŽźŹ";

        /// <summary>
        /// Alphabet without diacritics
        /// </summary>
        private const string _withoutDiacritics = "aAaAcCcCdDeEeEiInNnNoOoOrRrRsSsStTuUuUuUyYzZzZ";

        #endregion

        #region Private Members (Injects)

        private readonly ILogger _logger;

        #endregion

        #region Private Members

        /// <summary>
        /// Tokenizer of this processing
        /// </summary>
        private Tokenizer _tokenizer;

        /// <summary>
        /// Stemmer of this processing
        /// </summary>
        private Stemmer _stemmer;

        /// <summary>
        /// StopWord remover of this processing
        /// </summary>
        private StopWordRemover _stopWordRemover;

        /// <summary>
        /// Document inverted indexation
        /// </summary>
        private IInvertedIndex _invertedIndex;

        #endregion

        #region Public Properties

        /// <summary>
        /// Name that identifies the index processing instance
        /// </summary>
        /// <remarks>
        ///     Should not be <see cref="null"/>.
        /// </remarks>
        private string Name { get; }

        /// <summary>
        /// Read-only reference to <see cref="_invertedIndex"/>
        /// </summary>
        public IReadOnlyInvertedIndex InvertedIndex => _invertedIndex;

        /// <summary>
        /// Indicates if this processing puts the document into the lower case
        /// </summary>
        public bool ToLowerCase { get; } //; ctor

        /// <summary>
        /// Indicates if the processing removes accents before stemming or not
        /// </summary>
        public bool RemoveAccentsBeforeStemming { get; } //; ctor

        /// <summary>
        /// Indicates if the processing removes accents after stemming or not
        /// </summary>
        public bool RemoveAccentsAfterStemming { get; } //; ctor

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of the processing</param>
        /// <param name="timestamp">The timestamp for this instance that is used as an additional index identifier.</param>
        /// <param name="tokenizer">The tokenizer for this processing</param>
        /// <param name="stemmer">The stemmer for this processing</param>
        /// <param name="stopWordRemover">The stopword remover for this processing (leave <see langword="null"/> to ignore removing stopwords)</param>
        /// <param name="fileManager">File manager used used for storing the processed index (the index will be stored in-memory only if the manager is not set)</param>
        /// <param name="logger">Connect logger from the rest of the system (if not set, the logger will not log anything)</param>
        /// <param name="toLowerCase">Should the toknes be lowercased?</param>
        /// <param name="removeAccentsBeforeStemming">Should we remove accents from tokens before stemming?</param>
        /// <param name="removeAccentsAfterStemming">Should we remove accents from tokens after stemming?</param>
        public IndexProcessing(string name, DateTime timestamp, Tokenizer tokenizer, Stemmer stemmer, StopWordRemover stopWordRemover = null, IFileManager fileManager = null, ILogger logger = null, bool toLowerCase = false, bool removeAccentsBeforeStemming = false, bool removeAccentsAfterStemming = false)
        {
            Name = name;

            _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
            _stemmer = stemmer ?? throw new ArgumentNullException(nameof(stemmer));
            _stopWordRemover = stopWordRemover;
            _invertedIndex = new InvertedIndex(name, timestamp, fileManager, logger);

            _logger = logger;

            ToLowerCase = toLowerCase;
            RemoveAccentsBeforeStemming = removeAccentsBeforeStemming;
            RemoveAccentsAfterStemming = removeAccentsAfterStemming;
        }

        /// <summary>
        /// Constructor with configuration as a parameter
        /// </summary>
        /// <param name="name">Name of the processing</param>
        /// <param name="timestamp">The timestamp for this instance that is used as an additional index identifier.</param>
        /// <param name="configuration">The configuration for the processing</param>
        /// <param name="fileManager">File manager used used for storing the processed index (the index will be stored in-memory only if the manager is not set)</param>
        /// <param name="logger">Connect logger from the rest of the system (if not set, the logger will not log anything)</param>
        public IndexProcessing(string name, DateTime timestamp, IndexProcessingConfiguration configuration, IFileManager fileManager = null, ILogger logger = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            Name = name;

            _tokenizer = new Tokenizer(configuration.CustomRegex);
            _stemmer = new Stemmer(configuration.Language);
            _stopWordRemover = new StopWordRemover(configuration.Language, configuration.CustomStopWords);
            _invertedIndex = new InvertedIndex(name, timestamp, fileManager, logger);

            _logger = logger;

            ToLowerCase = configuration.ToLowerCase;
            RemoveAccentsBeforeStemming = configuration.RemoveAccentsBeforeStemming;
            RemoveAccentsAfterStemming = configuration.RemoveAccentsAfterStemming;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Go through the documents and run process for each of the documents according to the processing settings set initially onto this instance.
        /// Additionally, the method calls for inverted index to save data (if file manager was defined in constructor for this instance).
        /// </summary>
        /// <param name="documents">Array of the documents</param>
        /// <param name="save">Once the indexation will be done, the indexed data will be saved into file storage, and working in-memory storage will be cleared.</param>
        /// <param name="load">Loads already indexed data of <see cref="_invertedIndex"/> under the same <see cref="IReadOnlyInvertedIndex.Name"/> into working in-memory storage and <paramref name="documents"/> will be indexed into the loaded data as a addition.</param>
        /// <param name="setProgressMessage">Action to retrieve progress data ("what is going on during processing").</param>
        /// <param name="cancellationToken">Cancellation token for interrupting the process.</param>
        /// <exception cref="ArgumentNullException">If the array is null</exception>
        public void IndexDocuments(IndexDocument[] documents, bool save = false, bool load = false, Action<string> setProgressMessage = null, CancellationToken cancellationToken = default)
        {
            if (documents == null)
                throw new ArgumentNullException("Array of documents not specified!");

            // Load indexed data
            if (load && !cancellationToken.IsCancellationRequested)
            {
                setProgressMessage?.Invoke("Loading index...");
                _invertedIndex.Load();
            }

            // Indexate documents
            for (long i = 0; i < documents.Length; ++i)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                IndexDocument(documents[i], false, false);
                setProgressMessage?.Invoke($"Indexing... ({i}/{documents.Length})");
            }

            // Save indexed data
            if (save && !cancellationToken.IsCancellationRequested)
            {
                setProgressMessage?.Invoke("Saving index...");
                Action<string> savingProgressMessage = null;
                if (setProgressMessage != null)
                    savingProgressMessage = (value) => setProgressMessage?.Invoke($"Saving index... ({value})");

                _invertedIndex.Save(savingProgressMessage);
            }
        }

        /// <summary>
        /// Run process for the document according to the processing settings set initially onto this instance.
        /// </summary>
        /// <param name="document">The document</param>
        /// <param name="save">Once the indexation will be done, the indexed data will be saved into file storage, and working in-memory storage will be cleared.</param>
        /// <param name="load">Loads already indexed data of <see cref="_invertedIndex"/> under the same <see cref="IReadOnlyInvertedIndex.Name"/> into working in-memory storage and <paramref name="documents"/> will be indexed into the loaded data as a addition.</param>
        /// <exception cref="ArgumentNullException">If the document is null</exception>
        public void IndexDocument(IndexDocument document, bool save = false, bool load = false)
        {
            if (document == null)
                throw new ArgumentNullException("Document not specified!");

            // TODO: Create better approach for this
            string docContent = document.Category + " " + document.Title + " " + document.Content;

            // Load indexed data
            if (load)
                _invertedIndex.Load();

            // Process the data
            _ = Process(docContent, document, _invertedIndex);

            // Save indexed data
            if (save)
                _invertedIndex.Save();
        }

        /// <summary>
        /// Run process for the text independently of the rest of this instance (documents) and get the indexed vocabulary instantly.
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>Indexed (generates ReadOnly!) data based on the <paramref name="text"/>. The structure is the same as <see cref="InvertedIndex._data"/> with the only document here (ID=0).</returns>
        public InvertedIndex.ReadOnlyData IndexText(string text)
        {
            if (text == null)
                throw new ArgumentNullException("Text not specified!");

            // Define inverted index instance special for separate query indexing...
            var ii = new InvertedIndex(Name + "(query)", default, null, _logger);

            // Process the text data
            _ = Process(text, new IndexDocument(0, string.Empty, string.Empty, string.Empty, default, string.Empty), ii); // Create mock index document -we do not care about indexed document data

            // Return the vocabulary straight back
            return ii.GetReadOnlyData();
        }

        /// <summary>
        /// Process the <paramref name="text"/> by the processing settings. 
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>Processed text into terms.</returns>
        public string[] ProcessText(string text)
        {
            if (text == null)
                throw new ArgumentNullException("Text not specified!");

            return Process(text, null);
        }

        /// <summary>
        /// Process the <paramref name="word"/> (no sentence/text is expected - just a word) by the processing settings. 
        /// The process follows <see cref="Process"/>.
        /// </summary>
        /// <param name="word">The word to be processed.</param>
        /// <returns>Processed word.</returns>
        public string ProcessWord(string word)
        {
            if (word == null)
                throw new ArgumentNullException("Word not specified!");

            // To lower
            if (ToLowerCase)
                word = word.ToLower();

            // Remove accents before stemming
            if (RemoveAccentsBeforeStemming)
                word = RemoveAccents(word);

            // Stemming
            word = _stemmer.Stem(word);

            // Remove accents after stemming
            if (RemoveAccentsAfterStemming)
                word = RemoveAccents(word);

            return word;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Process specific text by the processing settings and indexate it according to <paramref name="documentId"/> (only if <paramref name="invertedIndex"/> is defined).
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="document">The document for indexation (If <paramref name="invertedIndex"/> is not defined, indexation will not proceed and this parameter will be ignored).</param>
        /// <param name="invertedIndex">Inverted index instance used for the indexation.</param>
        /// <returns>Processed terms</returns>
        private string[] Process(string text, IndexDocument document, IInvertedIndex invertedIndex = null)
        {
            // To lower
            if (ToLowerCase)
                text = text.ToLower();

            // Remove newlines from the document to prepare the doc for tokenization
            text = StringHelpers.ReplaceNewLines(text, " ");

            // Remove accents before stemming
            if (RemoveAccentsBeforeStemming)
                text = RemoveAccents(text);

            // Tokenize
            var terms = _tokenizer.Tokenize(text);

            // Remove stopwords
            if (_stopWordRemover != null)
                terms = _stopWordRemover.Process(terms);

            // Go through all terms
            for (int i = 0; i < terms.Length; ++i)
            {
                // Stemming
                terms[i] = _stemmer.Stem(terms[i]);

                // Remove accents after stemming
                if (RemoveAccentsAfterStemming)
                    terms[i] = RemoveAccents(terms[i]);

                // Indexate it
                if (invertedIndex != null)
                    invertedIndex.Put(terms[i], document);
            }

            return terms;
        }

        /// <summary>
        /// Remove accents from text
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>Text without accents</returns>
        private string RemoveAccents(string text)
        {
            for (int i = 0; i < _withDiacritics.Length; ++i)
                text = text.Replace(_withDiacritics[i], _withoutDiacritics[i]);
            return text;
        }

        #endregion
    }
}

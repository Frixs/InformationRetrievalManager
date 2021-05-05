﻿using InformationRetrievalManager.Core;
using Ixs.DNA;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace InformationRetrievalManager.NLP
{
    /// <summary>
    /// Definition of vector space model TF-IDF
    /// </summary>
    internal sealed class TfIdfModel : IQueryModel
    {
        #region Private Members (Injects)

        private readonly ILogger _logger;

        #endregion

        #region Private Members

        /// <summary>
        /// Calculated IDF for each term based on the last run of <see cref="CalculateData"/>.
        /// </summary>
        private Dictionary<string, double> _termIdf = null;

        /// <summary>
        /// Calculated document vectors based on the last run of <see cref="CalculateData"/>.
        /// </summary>
        private Dictionary<long, double[]> _documentVectors = null;

        /// <summary>
        /// Calculated query vector based on the last run of <see cref="CalculateQuery"/>.
        /// </summary>
        private double[] _queryVector = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger instance - not required, if not defined, the logger does log nothing</param>
        public TfIdfModel(ILogger logger = null)
        {
            _logger = logger;
        }

        #endregion

        #region Interface Methods

        /// <inheritdoc/>
        public void CalculateData(IReadOnlyDictionary<string, IReadOnlyDictionary<long, IReadOnlyTermInfo>> data, out long totalDocuments, Action<string> setProgressMessage = null, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException("Data not specified!");

            // (Re)Initialize the values
            _termIdf = new Dictionary<string, double>();
            _documentVectors = null;

            // Set of all document IDs found among terms
            var documents = new HashSet<long>();

            // Calculate term IDF
            long i = 0;
            foreach (var term in data)
            {
                i++;
                var termDocCount = 0;
                // Get no. of documents the term is located in.
                foreach (var termDocument in term.Value)
                {
                    // Check for cancelation
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (termDocument.Key < 0)
                        continue;

                    termDocCount++;
                    documents.Add(termDocument.Key);
                }

                // Check for cancelation
                if (cancellationToken.IsCancellationRequested)
                    break;

                // The IDF calculation
                _termIdf[term.Key] = Math.Log(documents.Count / termDocCount, 10);
                setProgressMessage?.Invoke($"data IDF calculation: {i}/{data.Count}");
            }
            totalDocuments = documents.Count;

            // Create array for document vectors
            var docVectors = new Dictionary<long, double[]>();

            // Go through documents and create document vectors...
            i = 0;
            foreach (var documentId in documents)
            {
                i++;
                // Check for cancelation
                if (cancellationToken.IsCancellationRequested)
                    break;

                docVectors[documentId] = CalculateDocumentVector(data, documentId, cancellationToken);
                setProgressMessage?.Invoke($"creating data document vectors: {i}/{documents.Count}");
            }

            // Save the vectors
            _documentVectors = docVectors;

            // Log it
            _logger?.LogDebugSource("Data has been successfully calculated into vectors.");
            setProgressMessage?.Invoke("data done");
        }

        /// <inheritdoc/>
        public void CalculateQuery(string query, IReadOnlyDictionary<string, IReadOnlyDictionary<long, IReadOnlyTermInfo>> data, IndexProcessingConfiguration processingConfiguration, Action<string> setProgressMessage = null, CancellationToken cancellationToken = default)
        {
            if (query == null || data == null)
                throw new ArgumentNullException("Data not specified!");

            if (_termIdf == null)
                throw new InvalidOperationException("Term IDF map is not defined!");
            if (_documentVectors == null)
                throw new InvalidOperationException("Document vectors are not defined!");

            // (Re)Initialize the values
            _queryVector = null;

            setProgressMessage?.Invoke("creating query document vector");

            // Process the query and indexate it for vocabulary
            var processing = new IndexProcessing("__tfidf",
                timestamp: default,
                new Tokenizer(processingConfiguration.CustomRegex),
                new Stemmer(processingConfiguration.Language),
                new StopWordRemover(processingConfiguration.Language, processingConfiguration.CustomStopWords),
                fileManager: null,
                logger: null,
                processingConfiguration.ToLowerCase,
                processingConfiguration.RemoveAccentsBeforeStemming,
                processingConfiguration.RemoveAccentsAfterStemming
                );
            var queryData = processing.IndexText(query);

            _queryVector = CalculateDocumentVector(CreateQueryDataVocabulary(queryData, data), 0, cancellationToken);

            // Log it
            _logger?.LogDebugSource("Query has been successfully calculated into vector.");
            setProgressMessage?.Invoke("query done");
        }

        /// <inheritdoc/>
        public long[] CalculateBestMatch(int select, out long foundDocuments, Action<string> setProgressMessage = null, CancellationToken cancellationToken = default)
        {
            var documentVectors = _documentVectors;
            var queryVector = _queryVector;

            if (select < 0)
                throw new InvalidCastException($"Parameter '{nameof(select)}' cannot be negative number!");

            if (documentVectors == null)
                throw new InvalidOperationException("Document vectors are not defined!");
            if (queryVector == null)
                throw new InvalidOperationException("Query vector is not defined!");

            // Calculate cosine similarity for each document...
            var results = new List<(long, double)>();
            long i = 0;
            foreach (var document in documentVectors)
            {
                i++;
                // Check for cancelation
                if (cancellationToken.IsCancellationRequested)
                    break;

                results.Add(
                    (document.Key, CalculateCosSimilarity(queryVector, document.Value))
                    );
                setProgressMessage?.Invoke($"calculating cosine similarity: {i}/{documentVectors.Count}");
            }

            // Sort and return result
            setProgressMessage?.Invoke("retrieving results");
            //results.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            results = results.OrderByDescending(o => o.Item2).ThenBy(o => o.Item1).ToList();
            foundDocuments = results.Count;
            if (select > 0)
                return results.Select(o => o.Item1).Take(select).ToArray();
            return results.Select(o => o.Item1).ToArray();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Calculate cosine similarity for 2 specific vectors.
        /// </summary>
        /// <param name="queryVector">Query vector</param>
        /// <param name="documentVector">Document vector</param>
        /// <returns>Cosine similarity result</returns>
        /// <exception cref="ArgumentNullException">Missing query or document vector reference.</exception>
        private double CalculateCosSimilarity(double[] queryVector, double[] documentVector)
        {
            if (queryVector == null || documentVector == null)
                throw new ArgumentNullException("Vectors not specified!");

            double queryPowSum = 0;
            foreach (var value in queryVector)
                queryPowSum += value * value;
            double querySum = Math.Sqrt(queryPowSum);

            double documentPowSum = 0;
            foreach (var value in documentVector)
                documentPowSum += value * value;
            double documentSum = Math.Sqrt(documentPowSum);

            double x = 0;
            for (int i = 0; i < queryVector.Length; i++)
                x += queryVector[i] * documentVector[i];

            return x / (documentSum * querySum);
        }

        /// <summary>
        /// Calculate space vector for the inputing data based on the <paramref name="documentId"/>.
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="documentId">The ID</param>
        /// <param name="cancellationToken">Cancellation token for interrupting the process.</param>
        /// <returns>Document vector from <paramref name="data"/> based on <paramref name="documentId"/>.</returns>
        private double[] CalculateDocumentVector(IReadOnlyDictionary<string, IReadOnlyDictionary<long, IReadOnlyTermInfo>> data, long documentId, CancellationToken cancellationToken = default)
        {
            List<double> docVector = new List<double>();
            foreach (var term in data)
            {
                // Check for cancelation
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Take only the terms located in the document...
                if (term.Value.ContainsKey(documentId))
                {
                    // Calculate term TF-IDF
                    double tf = term.Value[documentId].Frequency; // Term Frequency
                    double ntf = tf > 0 ? 1 + Math.Log(tf, 10) : 0; // 1 + log(TF) --- if TF == 0 => 0
                    double tfidf = _termIdf.ContainsKey(term.Key) ? ntf * _termIdf[term.Key] : 0; // If the term does not exist (query call), TF-IDF of the term is 0
                    docVector.Add(tfidf);
                }
                // Otherwise, it is 0...
                else
                {
                    docVector.Add(0);
                }
            }

            return docVector.ToArray();
        }

        /// <summary>
        /// Map <paramref name="query"/> vocabulary into <paramref name="data"/> representing documents vocabulary.
        /// </summary>
        /// <param name="query">The query vocabulary</param>
        /// <param name="data">The document vocabulary</param>
        /// <returns>Query vocabulary in scope of document vocabulary terms.</returns>
        private IReadOnlyDictionary<string, IReadOnlyDictionary<long, IReadOnlyTermInfo>> CreateQueryDataVocabulary(IReadOnlyDictionary<string, IReadOnlyDictionary<long, IReadOnlyTermInfo>> query, IReadOnlyDictionary<string, IReadOnlyDictionary<long, IReadOnlyTermInfo>> data)
        {
            var result = new SortedDictionary<string, Dictionary<long, TermInfo>>();

            foreach (var term in data)
            {
                var postingList = new Dictionary<long, TermInfo>();

                if (query.ContainsKey(term.Key))
                    foreach (var document in query[term.Key])
                        postingList.Add(document.Key, (TermInfo)document.Value);

                result.Add(term.Key, postingList);
            }

            return result.ToDictionary(o => o.Key, o => (IReadOnlyDictionary<long, IReadOnlyTermInfo>)o.Value.ToDictionary(x => x.Key, x => (IReadOnlyTermInfo)x.Value)); ;
        }

        #endregion
    }
}

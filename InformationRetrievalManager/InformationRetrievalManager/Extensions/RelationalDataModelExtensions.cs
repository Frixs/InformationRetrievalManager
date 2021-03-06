﻿using InformationRetrievalManager.NLP;
using InformationRetrievalManager.Relational;

namespace InformationRetrievalManager
{
    public static class RelationalDataModelExtensions
    {
        /// <summary>
        /// Transform relational data model <see cref="IndexedDocumentDataModel"/> into <see cref="IndexDocument"/>
        /// </summary>
        /// <param name="model">The relational data model <see cref="IndexedDocumentDataModel"/></param>
        /// <returns>Index data model with initialized ID</returns>
        public static IndexDocument ToIndexDocument(this IndexedDocumentDataModel model)
        {
            return new IndexDocument(model.Id, model.Title, model.SourceUrl, model.Category, model.Timestamp, model.Content);
        }
    }
}

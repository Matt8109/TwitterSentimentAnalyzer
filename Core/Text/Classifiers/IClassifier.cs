using System;
using System.Collections.Generic;
using System.Data.Entity;
using Oclumen.Core.DataContexts;
using Oclumen.Core.Entities;

namespace Oclumen.Core.Text.Classifiers
{
    internal interface IClassifier
    {
        Sentiment GetTextSentiment<TEntity>(string text, bool isRetweet, int ngramCardinality, decimal smoothingFactor,
                                            bool isStemmed, IDictionary<String, String> dictionary,
                                            DbSet<TEntity> ngramDbSet, IOclumenContext oclumenContext,
                                            Dictionary<string, List<KeyValuePair<Sentiment, decimal>>> ngramDictionary)
            where TEntity : NgramBase;
    }
}
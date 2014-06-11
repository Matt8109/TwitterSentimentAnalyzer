using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oclumen.Web.Models;

namespace Oclumen.Web.Controllers
{
    public class StatisticsController : BaseController
    {
        //
        // GET: /Statistics/

        public ActionResult Index()
        {
            var model = new StatisticsModel()
                {
                    TotalTweets = OclumenContext.RawTweets.Count(),
                    TweetsAwaitingProcessing = OclumenContext.RawTweets.Count(x => x.AutoSentimentTimestamp == DateTime.MinValue),
                    TotalNgrams = OclumenContext.BasicNgrams.Count(),
                    Unigrams = OclumenContext.BasicNgrams.Count(x => x.Cardinality == 1),
                    Bigrams = OclumenContext.BasicNgrams.Count(x => x.Cardinality == 2),
                    TotalNgramsStemmed = OclumenContext.StemmedNgrams.Count(),
                    StemmedUnigrams = OclumenContext.StemmedNgrams.Count(x => x.Cardinality == 1),
                    StemmedBigrams = OclumenContext.StemmedNgrams.Count(x => x.Cardinality == 2),
                    TotalHashtagNgrams = OclumenContext.Hashtags.Count(),
                };

            return View(model);
        }

    }
}

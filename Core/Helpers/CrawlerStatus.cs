namespace Oclumen.Core.Helpers
{
    public class CrawlerStatus
    {
        private volatile bool _keepRunning;

        public bool KeepRunning { get { return _keepRunning; } set { _keepRunning = value; } }
    }
}

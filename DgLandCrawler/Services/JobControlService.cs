namespace DgLandCrawler.Services
{
    public class JobControlService
    {
        private bool _should_run = false;
        public bool Should_Run => _should_run;

        public void StartJob()
        {
            _should_run = true;
        }

        public void StopJob()
        {
            _should_run = false;
        }   
    }
}

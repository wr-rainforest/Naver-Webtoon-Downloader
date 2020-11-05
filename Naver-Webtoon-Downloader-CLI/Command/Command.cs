using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wr_rainforest.Naver_Webtoon_Downloader_CLI.Command
{
    abstract class Command
    {
        protected IProgress<string> progress;
        protected Config config;
        public Command(Config config)
        {
            this.config = config;
            progress = new CommandProgress();
        }
        public abstract void Start(params string[] args);
    }
}

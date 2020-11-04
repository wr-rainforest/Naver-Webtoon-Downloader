using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WRforest.NWD
{
    public class EpisodeNotFoundException : Exception
    {
        public string TitleId { get; set; }
        public int EpisodeNo { get; set; }

        public EpisodeNotFoundException()
        {

        }

        public EpisodeNotFoundException(string message) : base(message)
        {
        }

        public EpisodeNotFoundException(string titleId, int episodeNo)
        {
            TitleId = titleId;
            EpisodeNo = episodeNo;
        }

        public EpisodeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EpisodeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

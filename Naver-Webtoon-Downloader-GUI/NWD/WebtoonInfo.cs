using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace wr_rainforest.NWD
{
    class WebtoonInfo
    {
        /// <summary>
        /// 웹툰 제목입니다.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 웹툰 id입니다.
        /// </summary>
        public string TitleId { get; private set; }

        /// <summary>
        /// 웹툰작가 닉네임입니다.
        /// </summary>
        public string Writer { get; set; }

        /// <summary>
        /// 웹툰 장르입니다.
        /// </summary>
        public string Genre { get; private set; }

        /// <summary>
        /// 웹툰 설명입니다.
        /// </summary>
        public string Description { get; private set; }


        /// <summary>
        /// <seealso cref="WebtoonInfo"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="title"></param>
        public WebtoonInfo(string titleId, string title, string writer, string genre, string description)
        {
            TitleId = titleId;
            Title = title;
            Writer = writer;
            Genre = genre;
            Description = description;
        }

    }
}

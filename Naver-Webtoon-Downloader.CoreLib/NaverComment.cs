using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NaverWebtoonDownloader.CoreLib
{
    public class NaverComment
    {
        [JsonProperty(PropertyName = "ticket")]
        public string Ticket { get; set; }

        [JsonProperty(PropertyName = "objectId")]
        public string ObjectId { get; set; }

        [JsonProperty(PropertyName = "categoryId")]
        public string CategoryId { get; set; }

        [JsonProperty(PropertyName = "templateId")]
        public string TemplateId { get; set; }

        [JsonProperty(PropertyName = "commentNo")]
        public int CommentNo { get; set; }

        [JsonProperty(PropertyName = "parentCommentNo")]
        public int ParentCommentNo { get; set; }

        [JsonProperty(PropertyName = "replyLevel")]
        public int ReplyLevel { get; set; }

        [JsonProperty(PropertyName = "replyCount")]
        public int ReplyCount { get; set; }

        [JsonProperty(PropertyName = "replyAllCount")]
        public int ReplyAllCount { get; set; }

        [JsonProperty(PropertyName = "replyPreviewNo")]
        public object ReplyPreviewNo { get; set; }

        [JsonProperty(PropertyName = "replyList")]
        public object ReplyList { get; set; }

        [JsonProperty(PropertyName = "imageCount")]
        public int ImageCount { get; set; }

        [JsonProperty(PropertyName = "imageList")]
        public object ImageList { get; set; }

        [JsonProperty(PropertyName = "imagePathList")]
        public object ImagePathList { get; set; }

        [JsonProperty(PropertyName = "imageWidthList")]
        public object ImageWidthList { get; set; }

        [JsonProperty(PropertyName = "imageHeightList")]
        public object ImageHeightList { get; set; }

        [JsonProperty(PropertyName = "commentType")]
        public string CommentType { get; set; }

        [JsonProperty(PropertyName = "stickerId")]
        public object StickerId { get; set; }

        [JsonProperty(PropertyName = "sticker")]
        public object Sticker { get; set; }

        [JsonProperty(PropertyName = "sortValue")]
        public long SortValue { get; set; }

        [JsonProperty(PropertyName = "contents")]
        public string Contents { get; set; }

        [JsonProperty(PropertyName = "userIdNo")]
        public string UserIdNo { get; set; }

        [JsonProperty(PropertyName = "exposedUserIp")]
        public object ExposedUserIp { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Lang { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "idType")]
        public string IdType { get; set; }

        [JsonProperty(PropertyName = "idProvider")]
        public string IdProvider { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "userProfileImage")]
        public string UserProfileImage { get; set; }

        [JsonProperty(PropertyName = "profileType")]
        public string ProfileType { get; set; }

        [JsonProperty(PropertyName = "modTime")]
        public DateTime ModTime { get; set; }

        [JsonProperty(PropertyName = "modTimeGmt")]
        public DateTime ModTimeGmt { get; set; }

        [JsonProperty(PropertyName = "regTime")]
        public DateTime RegTime { get; set; }

        [JsonProperty(PropertyName = "regTimeGmt")]
        public DateTime RegTimeGmt { get; set; }

        [JsonProperty(PropertyName = "sympathyCount")]
        public int SympathyCount { get; set; }

        [JsonProperty(PropertyName = "antipathyCount")]
        public int AntipathyCount { get; set; }

        [JsonProperty(PropertyName = "hideReplyButton")]
        public bool HideReplyButton { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "mine")]
        public bool Mine { get; set; }

        [JsonProperty(PropertyName = "best")]
        public bool Best { get; set; }

        [JsonProperty(PropertyName = "mentions")]
        public object Mentions { get; set; }

        [JsonProperty(PropertyName = "toUser")]
        public object ToUser { get; set; }

        [JsonProperty(PropertyName = "userStatus")]
        public int UserStatus { get; set; }

        [JsonProperty(PropertyName = "categoryImage")]
        public object CategoryImage { get; set; }

        [JsonProperty(PropertyName = "open")]
        public bool Open { get; set; }

        [JsonProperty(PropertyName = "levelCode")]
        public object LevelCode { get; set; }

        [JsonProperty(PropertyName = "grades")]
        public object Grades { get; set; }

        [JsonProperty(PropertyName = "sympathy")]
        public bool Sympathy { get; set; }

        [JsonProperty(PropertyName = "antipathy")]
        public bool Antipathy { get; set; }

        [JsonProperty(PropertyName = "snsList")]
        public object SnsList { get; set; }

        [JsonProperty(PropertyName = "metaInfo")]
        public object MetaInfo { get; set; }

        [JsonProperty(PropertyName = "extension")]
        public object Extension { get; set; }

        [JsonProperty(PropertyName = "audioInfoList")]
        public object AudioInfoList { get; set; }

        [JsonProperty(PropertyName = "translation")]
        public object Translation { get; set; }

        [JsonProperty(PropertyName = "report")]
        public object Report { get; set; }

        [JsonProperty(PropertyName = "middleBlindReport")]
        public bool MiddleBlindReport { get; set; }

        [JsonProperty(PropertyName = "spamInfo")]
        public object SpamInfo { get; set; }

        [JsonProperty(PropertyName = "userHomepageUrl")]
        public object UserHomepageUrl { get; set; }

        [JsonProperty(PropertyName = "defamation")]
        public bool Defamation { get; set; }

        [JsonProperty(PropertyName = "hiddenByCleanbot")]
        public bool HiddenByCleanbot { get; set; }

        [JsonProperty(PropertyName = "evalScore")]
        public object EvalScore { get; set; }

        [JsonProperty(PropertyName = "visible")]
        public bool Visible { get; set; }

        [JsonProperty(PropertyName = "serviceId")]
        public object ServiceId { get; set; }

        [JsonProperty(PropertyName = "idNo")]
        public string IdNo { get; set; }

        [JsonProperty(PropertyName = "deleted")]
        public bool Deleted { get; set; }

        [JsonProperty(PropertyName = "anonymous")]
        public bool Anonymous { get; set; }

        [JsonProperty(PropertyName = "expose")]
        public bool Expose { get; set; }

        [JsonProperty(PropertyName = "manager")]
        public bool Manager { get; set; }

        [JsonProperty(PropertyName = "blindReport")]
        public bool BlindReport { get; set; }

        [JsonProperty(PropertyName = "secret")]
        public bool Secret { get; set; }

        [JsonProperty(PropertyName = "blind")]
        public bool Blind { get; set; }

        [JsonProperty(PropertyName = "containText")]
        public bool ContainText { get; set; }

        [JsonProperty(PropertyName = "exposeByCountry")]
        public bool ExposeByCountry { get; set; }

        [JsonProperty(PropertyName = "userBlocked")]
        public bool UserBlocked { get; set; }

        [JsonProperty(PropertyName = "validateBanWords")]
        public bool ValidateBanWords { get; set; }

        [JsonProperty(PropertyName = "profileUserId")]
        public object ProfileUserId { get; set; }

        [JsonProperty(PropertyName = "virtual")]
        public bool Virtual { get; set; }

        [JsonProperty(PropertyName = "maskedUserId")]
        public string MaskedUserId { get; set; }

        [JsonProperty(PropertyName = "maskedUserName")]
        public string MaskedUserName { get; set; }
    }
}

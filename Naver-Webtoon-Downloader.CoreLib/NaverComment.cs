using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NaverWebtoonDownloader.CoreLib
{
    public class NaverComment
    {
        [JsonPropertyName("ticket")]
        public string Ticket { get; set; }

        [JsonPropertyName("objectId")]
        public string ObjectId { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        [JsonPropertyName("templateId")]
        public string TemplateId { get; set; }

        [JsonPropertyName("commentNo")]
        public int CommentNo { get; set; }

        [JsonPropertyName("parentCommentNo")]
        public int ParentCommentNo { get; set; }

        [JsonPropertyName("replyLevel")]
        public int ReplyLevel { get; set; }

        [JsonPropertyName("replyCount")]
        public int ReplyCount { get; set; }

        [JsonPropertyName("replyAllCount")]
        public int ReplyAllCount { get; set; }

        [JsonPropertyName("replyPreviewNo")]
        public object ReplyPreviewNo { get; set; }

        [JsonPropertyName("replyList")]
        public object ReplyList { get; set; }

        [JsonPropertyName("imageCount")]
        public int ImageCount { get; set; }

        [JsonPropertyName("imageList")]
        public object ImageList { get; set; }

        [JsonPropertyName("imagePathList")]
        public object ImagePathList { get; set; }

        [JsonPropertyName("imageWidthList")]
        public object ImageWidthList { get; set; }

        [JsonPropertyName("imageHeightList")]
        public object ImageHeightList { get; set; }

        [JsonPropertyName("commentType")]
        public string CommentType { get; set; }

        [JsonPropertyName("stickerId")]
        public object StickerId { get; set; }

        [JsonPropertyName("sticker")]
        public object Sticker { get; set; }

        [JsonPropertyName("sortValue")]
        public long SortValue { get; set; }

        [JsonPropertyName("contents")]
        public string Contents { get; set; }

        [JsonPropertyName("userIdNo")]
        public string UserIdNo { get; set; }

        [JsonPropertyName("exposedUserIp")]
        public object ExposedUserIp { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("idType")]
        public string IdType { get; set; }

        [JsonPropertyName("idProvider")]
        public string IdProvider { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("userProfileImage")]
        public string UserProfileImage { get; set; }

        [JsonPropertyName("profileType")]
        public string ProfileType { get; set; }

        [JsonPropertyName("modTime")]
        public DateTime ModTime { get; set; }

        [JsonPropertyName("modTimeGmt")]
        public DateTime ModTimeGmt { get; set; }

        [JsonPropertyName("regTime")]
        public DateTime RegTime { get; set; }

        [JsonPropertyName("regTimeGmt")]
        public DateTime RegTimeGmt { get; set; }

        [JsonPropertyName("sympathyCount")]
        public int SympathyCount { get; set; }

        [JsonPropertyName("antipathyCount")]
        public int AntipathyCount { get; set; }

        [JsonPropertyName("hideReplyButton")]
        public bool HideReplyButton { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("mine")]
        public bool Mine { get; set; }

        [JsonPropertyName("best")]
        public bool Best { get; set; }

        [JsonPropertyName("mentions")]
        public object Mentions { get; set; }

        [JsonPropertyName("toUser")]
        public object ToUser { get; set; }

        [JsonPropertyName("userStatus")]
        public int UserStatus { get; set; }

        [JsonPropertyName("categoryImage")]
        public object CategoryImage { get; set; }

        [JsonPropertyName("open")]
        public bool Open { get; set; }

        [JsonPropertyName("levelCode")]
        public object LevelCode { get; set; }

        [JsonPropertyName("grades")]
        public object Grades { get; set; }

        [JsonPropertyName("sympathy")]
        public bool Sympathy { get; set; }

        [JsonPropertyName("antipathy")]
        public bool Antipathy { get; set; }

        [JsonPropertyName("snsList")]
        public object SnsList { get; set; }

        [JsonPropertyName("metaInfo")]
        public object MetaInfo { get; set; }

        [JsonPropertyName("extension")]
        public object Extension { get; set; }

        [JsonPropertyName("audioInfoList")]
        public object AudioInfoList { get; set; }

        [JsonPropertyName("translation")]
        public object Translation { get; set; }

        [JsonPropertyName("report")]
        public object Report { get; set; }

        [JsonPropertyName("middleBlindReport")]
        public bool MiddleBlindReport { get; set; }

        [JsonPropertyName("spamInfo")]
        public object SpamInfo { get; set; }

        [JsonPropertyName("userHomepageUrl")]
        public object UserHomepageUrl { get; set; }

        [JsonPropertyName("defamation")]
        public bool Defamation { get; set; }

        [JsonPropertyName("hiddenByCleanbot")]
        public bool HiddenByCleanbot { get; set; }

        [JsonPropertyName("evalScore")]
        public object EvalScore { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("serviceId")]
        public object ServiceId { get; set; }

        [JsonPropertyName("idNo")]
        public string IdNo { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        [JsonPropertyName("anonymous")]
        public bool Anonymous { get; set; }

        [JsonPropertyName("expose")]
        public bool Expose { get; set; }

        [JsonPropertyName("manager")]
        public bool Manager { get; set; }

        [JsonPropertyName("blindReport")]
        public bool BlindReport { get; set; }

        [JsonPropertyName("secret")]
        public bool Secret { get; set; }

        [JsonPropertyName("blind")]
        public bool Blind { get; set; }

        [JsonPropertyName("containText")]
        public bool ContainText { get; set; }

        [JsonPropertyName("exposeByCountry")]
        public bool ExposeByCountry { get; set; }

        [JsonPropertyName("userBlocked")]
        public bool UserBlocked { get; set; }

        [JsonPropertyName("validateBanWords")]
        public bool ValidateBanWords { get; set; }

        [JsonPropertyName("profileUserId")]
        public object ProfileUserId { get; set; }

        [JsonPropertyName("virtual")]
        public bool Virtual { get; set; }

        [JsonPropertyName("maskedUserId")]
        public string MaskedUserId { get; set; }

        [JsonPropertyName("maskedUserName")]
        public string MaskedUserName { get; set; }
    }
}

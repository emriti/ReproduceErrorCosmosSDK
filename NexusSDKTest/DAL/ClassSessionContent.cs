using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;

namespace Nexus.Base.CosmosDBRepositoryTests.DAL
{
    //@Kh
    public class ClassSessionContent : ModelBase
    {

        [JsonProperty(PropertyName = "classSessionId")]
        public string ClassSessionId { get; set; }

        [JsonProperty(PropertyName = "classTopicId")]
        public string ClassTopicId { get; set; }

        // 30 April 2020 @sintongsimon
        [JsonProperty(PropertyName = "courseTopicId")]
        public string CourseTopicId { get; set; }

        // butuh link secara symbolic mengacu ke resourceId
        // @ss 2021-04-26 : resourceId also refer assignment/assessment and quiz unique id from existing system (BM5/BOL)
        [JsonProperty(PropertyName = "resourceId")]
        public string ResourceId { get; set; }

        private string _resourceStatus;
        [JsonProperty(PropertyName = "resourceStatus")]
        public string ResourceStatus
        {
            get { return _resourceStatus; }
            set
            {
                if (value != null && value != "draft" && 
                    value != "encoding" && value != "publish" && 
                    value != "error")
                {
                    throw new InvalidOperationException("ResourceStatus should be 'draft','encoding','error','publish'");
                }
                _resourceStatus = value;
            }
        }

        [JsonProperty(PropertyName = "contentSubject")]
        public string ContentSubject { get; set; }

        [JsonProperty(PropertyName = "contentUrl")]
        public string ContentUrl { get; set; }

        // pada assessment : digunakan untuk assessmentType (authentic/objective/lti)
        [JsonProperty(PropertyName = "contentAddress")]
        public string ContentAddress { get; set; }

        // @ss 2021-04-27
        [JsonProperty(PropertyName = "contentTypeId")]
        public string ContentTypeId { get; set; }
        // @ss 2021-04-26 : new contentType for assigment (
        //contentCategory: main => assessment bm5/bol, 
        //contentCategory: additional => assessment bm7 ) 
        //and quiz (bm5/bol)
        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; }

        [JsonProperty(PropertyName = "mode")]
        public string Mode { get; set; }

        [JsonProperty(PropertyName = "groupSetId")]
        public string GroupSetId { get; set; }

        // durasi video / durasi estimasi membaca artikel (default article: 10m ("600"), video conference: 100m ("6000"))
        [JsonProperty(PropertyName = "contentDuration")]
        public string ContentDuration { get; set; }


        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }


        [JsonProperty(PropertyName = "contentCategory")]
        public string ContentCategory { get; set; }

        [JsonProperty(PropertyName = "contentOwner")]
        public Instructor ContentOwner { get; set; }

        // saat ini dipakai assessment
        [JsonProperty(PropertyName = "contentEndDate")]
        public DateTime ContentEndDate { get; set; }

        [JsonProperty(PropertyName = "contentEndDateUtc")]
        public DateTime ContentEndDateUtc { get; set; }

        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; }


        [JsonProperty(PropertyName = "createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty(PropertyName = "createdDateUtc")]
        public DateTime CreatedDateUtc { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "modifiedDate")]
        public DateTime ModifiedDate { get; set; }

        [JsonProperty(PropertyName = "modifiedDateUtc")]
        public DateTime ModifiedDateUtc { get; set; }

        [JsonProperty(PropertyName = "modifiedBy")]
        public string ModifiedBy { get; set; }

        // @ds : untuk kebutuhan vicon

        [JsonProperty(PropertyName = "contentStartDate")]
        public DateTime ContentStartDate { get; set; }

        [JsonProperty(PropertyName = "contentStartDateUtc")]
        public DateTime ContentStartDateUtc { get; set; }

        [JsonProperty(PropertyName = "startUrl")]
        public string StartUrl { get; set; }

        [JsonProperty(PropertyName = "joinUrl")]
        public string JoinUrl { get; set; }

        [JsonProperty(PropertyName = "additionalContent")]
        public object AdditionalContent { get; set; }

        // untuk kepentingan DueDate Assessment 26 Mei

        //[JsonProperty(PropertyName = "dueDateUtc")]
        //public DateTime DueDateUtc { get; set; }


        // 2020-10-15 @ss additional from migration (consider to remove not used attribute)

        [JsonProperty(PropertyName = "courseId")]
        public string CourseId { get; set; }

        [JsonProperty(PropertyName = "institution")]
        public string Institution { get; set; }

        [JsonProperty(PropertyName = "academicCareer")]
        public string AcademicCareer { get; set; }

        [JsonProperty(PropertyName = "academicGroup")]
        public string AcademicGroup { get; set; }

        [JsonProperty(PropertyName = "academicOrganization")]
        public string AcademicOrganization { get; set; }

        [JsonProperty(PropertyName = "crseId")]
        public string CrseId { get; set; }

        [JsonProperty(PropertyName = "courseOfferNumber")]
        public int CourseOfferNumber { get; set; }

        [JsonProperty(PropertyName = "academicPeriod")]
        public string AcademicPeriod { get; set; }

        [JsonProperty(PropertyName = "academicPeriodDesc")]
        public string AcademicPeriodDesc { get; set; }

        [JsonProperty(PropertyName = "sessionCode")]
        public string SessionCode { get; set; }

        [JsonProperty(PropertyName = "classCode")]
        public string ClassCode { get; set; }

        [JsonProperty(PropertyName = "ssrComponent")]
        public string SsrComponent { get; set; }

        [JsonProperty(PropertyName = "classNumber")]
        public int? ClassNumber { get; set; }

        [JsonProperty(PropertyName = "courseCode")]
        public string CourseCode { get; set; }

        [JsonProperty(PropertyName = "courseTitleEn")]
        public string CourseTitleEn { get; set; }

        [JsonProperty(PropertyName = "courseTitleId")]
        public string CourseTitleId { get; set; }

        [JsonProperty(PropertyName = "topicName")]
        public string TopicName { get; set; }

        [JsonProperty(PropertyName = "termBeginDate")]
        public DateTime TermBeginDate { get; set; }

        [JsonProperty(PropertyName = "termEndDate")]
        public DateTime TermEndDate { get; set; }

        [JsonProperty(PropertyName = "termBeginDateUtc")]
        public DateTime TermBeginDateUtc { get; set; }

        [JsonProperty(PropertyName = "termEndDateUtc")]
        public DateTime TermEndDateUtc { get; set; }

        [JsonProperty(PropertyName = "academicYear")]
        public string AcademicYear { get; set; }

        [JsonProperty(PropertyName = "classSessionNumber")]
        public int? ClassSessionNumber { get; set; }

        [JsonProperty(PropertyName = "weekNumber")]
        public int? WeekNumber { get; set; }

        [JsonProperty(PropertyName = "sessionNumber")]
        public int? SessionNumber { get; set; }

        [JsonProperty(PropertyName = "weekStartDate")]
        public DateTime WeekStartDate { get; set; }

        [JsonProperty(PropertyName = "weekEndDate")]
        public DateTime WeekEndDate { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty(PropertyName = "startDateUtc")]
        public DateTime StartDateUtc { get; set; }

        [JsonProperty(PropertyName = "endDateUtc")]
        public DateTime EndDateUtc { get; set; }

        [JsonProperty(PropertyName = "classDeliveryMode")]
        public string ClassDeliveryMode { get; set; }

        /*[JsonProperty(PropertyName = "facilityId")]
        public string FacilityId { get; set; }

        [JsonProperty(PropertyName = "buildingCode")]
        public string BuildingCode { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "room")]
        public string Room { get; set; }

        [JsonProperty(PropertyName = "roomDescription")]
        public string RoomDescription { get; set; }*/

        //

        // 2020-10-17 @ss additional from migration

        [JsonProperty(PropertyName = "academicPeriodId")]
        public string AcademicPeriodId { get; set; }

        [JsonProperty(PropertyName = "academicCareerId")]
        public string AcademicCareerId { get; set; }

        [JsonProperty(PropertyName = "contentLocation")]
        public string ContentLocation { get; set; }

        [JsonProperty(PropertyName = "contentPath")]
        public string ContentPath { get; set; }

        [JsonProperty(PropertyName = "contentFileName")]
        public string ContentFileName { get; set; }

        /// <summary>Indicate expiration token url. <br />Setted at SessionContentItem</summary>
        [JsonProperty(PropertyName = "tokenExpiredDate")]
        public DateTime? TokenExpiredDate { get; set; }


        [JsonProperty(PropertyName = "revision")]
        public int Revision { get; set; }

        /// <summary>
        /// Y | N
        /// </summary>
        [JsonProperty(PropertyName = "activeFlag")]
        public string ActiveFlag { get; set; }

        // 2021-04-16 @ds backlog sprint 13
        [JsonProperty(PropertyName = "deliveryModeId")]
        public string DeliveryModeId { get; set; }

        [JsonProperty(PropertyName = "deliveryMode")]
        public string DeliveryMode { get; set; }

        [JsonProperty(PropertyName = "deliveryModeDesc")]
        public string DeliveryModeDesc { get; set; }

        [JsonProperty(PropertyName = "isMandatory")]
        public bool IsMandatory { get; set; }


    }

    /*public class ContentAssessment : ClassSessionContent
    {
        [JsonProperty(PropertyName = "assessmentType")]
        public string AssessmentType { get; set; }

        [JsonProperty(PropertyName = "mode")]
        public string Mode { get; set; }

        [JsonProperty(PropertyName = "groupSetId")]
        public string GroupSetId { get; set; }
    }*/
}

// Content Category, "Main" "Additional"

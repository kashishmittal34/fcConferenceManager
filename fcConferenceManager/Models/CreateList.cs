using System;
using System.Collections.Generic;

namespace Models
{
    public class GetSegmentationField
    {
        public int segmentationFieldId { get; set; }
        public string segmentationFieldName { get; set; }
        public int segmentationFieldGroupId { get; set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }
        public int Position { get; set; }
    }
    public class SegmentationField
    {
        public string SegmentationFieldName { get; set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }        
        public int Position { get; set; }
    }

    public class GetSegmentationFieldGroup
    {
        public int segmentationFieldGroupId { get; set; }
        public string segmentationFieldGroupName { get; set; }
        public int position { get; set; }
    }

    public class CreateSegmentationFieldGroup
    {
        public string SegmentationFieldGroupName { get; set; }
        public int Position { get; set; }
    }
    public class GetListImport
    {
        public int importFileId { get; set; }
        public string importFileName { get; set; }
        public DateTime importDate { get; set; }
    }
    public class ErrorMessage
    {
        public int status { get; set; }
        public string error { get; set; }
        public string message { get; set; }
    }

    public class SegmentationFields
    {
        public int segmentationFieldId { get; set; }
        public string value { get; set; }
    }
    public class ContactCreateUpdate
    {
        public string emailAddress { get; set; }
        public string subscriptionState { get; set; }
        public List<SegmentationFields> segmentationFieldValues { get; set; }
    }

    public class FileMappings
    {
        public int segmentationFieldId { get; set; }
        public string defaultValue { get; set; }
        public int fileColumn { get; set; }
        public string fileColumnType { get; set; }
    }
    public class StartListImport
    {
        public string FileDelimiter { get; set; }
        public FileMappings[] Mappings { get; set; }
        public string FileName { get; set; }
        public byte[] FileStream { get; set; }
        public bool hasColumnNames { get; set; }
        public string ImportType { get; set; }
        public string SegmentationImportType { get; set; }
        public bool SuppressEmailNotifications { get; set; }
        public string TextQualifier { get; set; }
    }

    public class CreateEvent
    {
        public string EventName { get; set; }
        public int EventGroupId { get; set; }
        public string Status { get; set; }
    }
    public class GetEventGroup
    {
        public int eventGroupId { get; set; }
        public string eventGroupName { get; set; }
    }
    public class EventGroup
    {
        public string EventGroupName { get; set; }
    }

    public class CreateList
    {
        public string listName { get; set; }
        public int folderId { get; set; }
        public int ipPoolId { get; set; }
        public string bounceDomainAlias { get; set; }
        public string bounceHandling { get; set; }
        public int bounceUnsubscribeCount { get; set; }
        public DateTime createDate { get; set; }
        public bool enableBrowserLink { get; set; }
        public bool enableDoubleOptIn { get; set; }
        public bool enableDynamicContent { get; set; }
        public bool enableGoogleAnalytics { get; set; }
        public bool enableInternationalization { get; set; }
        public bool enableListHygiene { get; set; }
        public bool enableListRemovalHeader { get; set; }
        public bool enableListRemovalLink { get; set; }
        public bool enableListrakAnalytics { get; set; }
        public bool enableSpamScorePersonalization { get; set; }
        public bool enableToNamePersonalization { get; set; }
        public string fromEmail { get; set; }
        public string fromName { get; set; }
        public string googleTrackingDomains { get; set; }
        public string linkDomainAlias { get; set; }
        public string mediaDomainAlias { get; set; }
    }
}

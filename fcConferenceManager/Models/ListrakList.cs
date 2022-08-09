using System;
using System.Collections.Generic;

namespace Models
{
    public class CommonRESTResponse
    {
        public int status { get; set; }
        public string resourceId { get; set; }
    }

    public class ContactsResponse
    {
        public string status { get; set; }
        public Contact data { get; set; }
    }

    public class ListImportsResponse
    {
        public string status { get; set; }
        public List<GetListImport> data { get; set; }
    }

    public class SegmentationFieldResponse
    {
        public string status { get; set; }
        public List<GetSegmentationField> data { get; set; }
    }

    public class SegmentationFieldGroupResponse
    {
        public string status { get; set; }
        public List<GetSegmentationFieldGroup> data { get; set; }
    }

    public class ListImportResponse
    {
        public string status { get; set; }
        public GetListImport data { get; set; }
    }
    public class MessagesResponse
    {
        public string status { get; set; }
        public string nextPageCursor { get; set; }
        public List<Message> data { get; set; }
    }

    public class EventGroupsResponse
    {
        public string status { get; set; }
        public List<GetEventGroup> data { get; set; }
    }
    public class ListContactsResponse
    {
        public string status { get; set; }
        public string nextPageCursor { get; set; }
        public List<Contact> data { get; set; }
    }
    
    public class CampaignsResponse
    {
        public string status { get; set; }
        public List<Campaign> data { get; set; }
    }

    public class FoldersResponse
    {
        public string status { get; set; }
        public List<Folder> data { get; set; }
    }

    public class ListResponse
    {
        public string status { get; set; }
        public ListrakList data { get; set; }
    }
    public class ListsResponse
    {
        public string status { get; set; }
        public List<ListrakList> data { get; set; }
    }
    public class ListrakList
    {        
        public int listId { get; set; }
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
    }
}

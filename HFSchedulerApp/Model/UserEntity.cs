namespace SendSMSReminder.Model
{
    class UserEntity
    {
        public string id { get; set; }
        public string name { get; set; }
        public string gender { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string dateOfBirth { get; set; }
        public string address { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public string presentIssues { get; set; }
        public string pastIssues { get; set; }
        public string familyHistoryIssues { get; set; }
        public string foodChoices { get; set; }
        public string allergy { get; set; }
        public string medication { get; set; }
        public string sleepTime { get; set; }
        public string foodTime { get; set; }
        public string dayToDayActivities { get; set; }
        public string createdBy { get; set; }
        public string modifiedBy { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
        public bool? isActive { get; set; }
    }
}

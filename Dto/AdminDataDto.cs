namespace FastFingerTest.Dto
{
    public class AdminDataDto
    {
        public int SumRegisterUsers { get; set; }
        public int SumTestsResult { get; set; }
        public int TodayFinishTests {  get; set; }
        public int TodayCreatedTests { get; set; }
        public List<AdminAttendsInMonth> TestsAttends { get; set; }

    }
}

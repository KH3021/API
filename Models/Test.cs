namespace API.Models
{
    public class Test
    {
        public string TestId { get; set; }
        public string SkillId { get; set; }
        public List<Question> Questions { get; set; }
    }
}
namespace API.Models;

public class Question
{
    public string QuestionText { get; set; }
    public List<string> Options { get; set; }
    public string Answer { get; set; }
}
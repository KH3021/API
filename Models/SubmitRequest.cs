namespace API.Models;

public class AnswerModel
{
    public string Question { get; set; }
    public string SelectedAnswer { get; set; }
    public string CorrectAnswer { get; set; }
}

public class SubmitRequest
{
    public string UserId { get; set; }     // 🔥 U001
    public string SkillId { get; set; }    // 🔥 S001 instead of name
    public string Level { get; set; }      // Beginner / Mid / Expert

    public List<AnswerModel> Answers { get; set; } = new();
}
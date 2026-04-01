namespace API.Models;

public class AnswerModel
{
    public string Question { get; set; }
    public string SelectedAnswer { get; set; }
    public string CorrectAnswer { get; set; }
}

public class SubmitRequest
{
    public string UserId { get; set; }     
    public string SkillId { get; set; }    
    public string Level { get; set; }      

    public List<AnswerModel> Answers { get; set; } = new();
}
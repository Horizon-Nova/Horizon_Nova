namespace HNB.Areas.WebTest.Models;

/// <summary>
/// 對應 <form> 送出的欄位 name
/// </summary>
public class WebTestDto
{
    public string? Foo { get; set; }     // <input name="Foo">
    public int? Bar { get; set; }     // <input name="Bar">
    // …依你的表單欄位自行增加
}

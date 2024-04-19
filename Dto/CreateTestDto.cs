using System.ComponentModel.DataAnnotations;

namespace FastFingerTest.Dto
{
    public enum SeparatorType
    {
        Space = ' ',
        Comma = ',',
        Semicolon = ';',
        VerticalBar = '|'

    }
    public class CreateTestDto
    {
        [Required]
        [MaxLength(40)]
        public string Name { get; set; }
        [Required]
        public int Length { get; set; }
        [Required]
        public SeparatorType Separator { get; set; }
        [Required]
        public IFormFile File { get; set; }



    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace FastFingerTest.Models
{
    public class WordSequence
    {
        public int Id { get; set; }
        public string WritingTestId { get; set; }
        public int Position { get; set; }

        [ForeignKey("WritingTestId")]
        public WritingTest WritingTest { get; set; }

        public int UserWordId { get; set; }
        [ForeignKey("UserWordId")]
        public UserWorld Word { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;
namespace API.Entities
{
    public class Student
    {

        [Key]
        public int Id { get; set; }
        public string StudentName { get; set; }

    }
}
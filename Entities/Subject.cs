
using System;
using System.ComponentModel.DataAnnotations;
namespace API.Entities
{
    public class Subject
    {

        [Key]
        public int Id { get; set; }
        public string SubjectName { get; set; }
        public int Units { get; set; }
        public DateTime Schedule { get; set; }

    }
}
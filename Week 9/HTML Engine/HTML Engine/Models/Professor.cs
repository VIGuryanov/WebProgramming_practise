namespace HTML_Engine.Models
{
    public class Professor
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Lastname { get; set; }
        public string[] Disciplines { get; set; }
        public string[] GroupNumber { get; set; }

        public List<Student> Students {get;set; } = new List<Student>();

        public Professor(string name, string surname, string lastname, string[] disciplines, string[] groupNumber)
        {
            Name = name;
            Surname = surname;
            Lastname = lastname;
            Disciplines = disciplines;
            GroupNumber = groupNumber;
        }
    }

    public class Student
    {
        public string StudId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Lastname { get; set; }
        public object[] Op {get;set; } = new object[] {new string[] {"ff", "LL" },new int[] {0,1 } };

        public Student(string studId, string name, string surname, string lastname)
        {
            StudId = studId;
            Name = name;
            Surname = surname;
            Lastname = lastname;
        }
    }
}

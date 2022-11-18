using HTML_Engine.Models;
using HTML_Engine_Library;
using System.Text.RegularExpressions;

namespace HTML_Engine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string template = "<html>\r\n\t<head></head>\r\n\t<body>\r\n\t\t<p>Surname:\t\t{{surname}}</p>\r\n\t\t<p>Name:\t\t{{name}}</p>\r\n\t\t<p>Lastname:\t{{lastname}}</p>\r\n\t\t<p>Discipline:\t{{discipline[0]}}</p>\r\n\t\t<p>#group:\t\t{{groupnumber}}</p>\r\n\t\t{{foreach in professor.students~\r\n\t\t\t\t<p>{{studid}} {{surname}} {{name}} {{lastname}}</p>\r\n\t\t}}\r\n\t</body>\r\n</html>";
            string template = File.ReadAllText("C:\\Users\\vadimgur\\source\\repos\\HTML Engine\\HTML Engine\\templates\\index.template");
            var professor = new Professor("namePH", "surnamePH", "lastnamePH", new string[] {"d1", "d2", "d3" }, new string[] {"1", "2" });

            professor.Students.Add(new Student("0","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("1","Anon2", "Anonym2", "Anonymous2"));
            professor.Students.Add(new Student("2","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("3","Anon2", "Anonym2", "Anonymous2"));
            professor.Students.Add(new Student("4","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("5","Anon2", "Anonym2", "Anonymous2"));
            professor.Students.Add(new Student("6","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("7","Anon2", "Anonym2", "Anonymous2"));
            professor.Students.Add(new Student("8","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("9","Anon2", "Anonym2", "Anonymous2"));
            professor.Students.Add(new Student("10","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("11","Anon2", "Anonym2", "Anonymous2"));
            professor.Students.Add(new Student("12","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("13","Anon2", "Anonym2", "Anonymous2"));
            professor.Students.Add(new Student("14","Anon1", "Anonym1", "Anonymous1"));
            professor.Students.Add(new Student("15","Anon2", "Anonym2", "Anonymous2"));
            string result = new EngineHTMLService().GetHTML(template, professor);
            /*var template = "{{Length}}";
            var professor = new Professor("", "", "", new string[0], new string[0]);
            professor.Students.Add( new Student("","Name1", "", ""));
            professor.Students.Add(new Student("","Name2", "", ""));

            var j = new object[] { new string[] { "qwerty", "asdfg" }, new int[] { 0, 1 } };
            var l = j[1];

            string result = new EngineHTMLService().GetHTML("{{foreach in Students\r\n {{Name}} {{foreach in Students\r\n {{Name}} }}}}",professor);*/
            Console.WriteLine(result);
            //Console.WriteLine(TemplateParser.ProcessCondition("2!=3&true"));
        }
    }
}
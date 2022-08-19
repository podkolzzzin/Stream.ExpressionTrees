using System.Collections.Generic;
using System.Linq;

namespace ExpressionBuilding;


public class Person
{
    public string Name { get; init; }
    public string Surname { get; init; }
    
    public int Age { get; init; }
}

public class Band
{
    public string Name { get; init; }
    
    public IEnumerable<Person> Members { get; set; }
}

public class FakeContext
{
    public IQueryable<Person> People { get; }
    
    public IQueryable<Band> Bands { get; }

    public FakeContext()
    {
        People = new List<Person>()
        {
            new () { Name = "Andrii", Surname = "Podkolzin", Age = 26},
            new () { Name = "Iurii", Surname = "Podkolzin", Age = 64},
            new () { Name = "Keith", Surname = "Richards", Age = 78},
            new () { Name = "Mike", Surname = "Jagger", Age = 79 },
            new () { Name = "Ronnie", Surname = "Wood", Age = 75},
            new () { Name = "Charlie", Surname = "Watts", Age = 80},
        }.AsQueryable();

        Bands = new List<Band>()
        {
            new() { Name = "The Rolling Stones", Members = People.Skip(2).ToList() }
        }.AsQueryable();
    }
}

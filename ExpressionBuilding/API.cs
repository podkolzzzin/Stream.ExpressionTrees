using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ExpressionBuilding;

public abstract class Filter
{
    
}

public class ComplexFilter : Filter
{
    public IEnumerable<Filter> Filters { get; init; }
}

public class SimpleFilter : Filter
{
    public string Operator { get; init; }
    public string Value { get; init; }
    public string Property { get; init; }
}

public class PersonController
{
    private readonly QueryBuilder _queryBuilder;
    private readonly FakeContext _context;
    private readonly FilterBuilder _filterBuilder;
    
    public PersonController(FakeContext context, QueryBuilder queryBuilder, FilterBuilder filterBuilder)
    {
        _context = context;
        _queryBuilder = queryBuilder;
        _filterBuilder = filterBuilder;
    }
    
    public async Task<IEnumerable<Person>> Get(JToken filter)
    {
        return _queryBuilder.BuildQuery(_context.People, _filterBuilder.BuildFilter(filter)).ToList();
    }
}
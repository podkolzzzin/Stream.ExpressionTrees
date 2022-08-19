using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ExpressionBuilding;

public static class RequestExecutor
{
    public static async Task<IEnumerable<Person>> GetPerson(string json)
    {
        var controller = new PersonController(new FakeContext(), new QueryBuilder(), new FilterBuilder());
        return await controller.Get(JToken.Parse(json));
    }
    
    public static async Task<T[]> ExecuteRequest<T>(string request)
    {
        var ctx = new FakeContext();
        var collectionType = ctx.GetType().GetProperties().Single(x => x.PropertyType == typeof(IQueryable<T>));
        var query = (IQueryable<T>)collectionType.GetValue(ctx)!;

        var filterBuilder = new FilterBuilder();
        var queryBuilder = new QueryBuilder();
        var filter = filterBuilder.BuildFilter(JToken.Parse(request));
        var resultQuery = queryBuilder.BuildQuery(query, filter);

        return resultQuery.ToArray();
    }
}

public class UnitTest1
{
    [Fact]
    public async Task SimpleFilter_ReturnsSingleElement()
    {
        var result = await RequestExecutor.GetPerson("{'Name': 'Andrii'}");
        Assert.Single(result);
        Assert.All(result, person =>
        {
            Assert.Equal("Andrii", person.Name);
            Assert.Equal("Podkolzin", person.Surname);
        });
    }
    
    [Fact]
    public async Task TwoFiledFilter_ReturnsSingleElement()
    {
        var result = await RequestExecutor.GetPerson("{'Name': 'Iurii', 'Surname': 'Podkolzin'}");
        Assert.Single(result);
        Assert.All(result, person =>
        {
            Assert.Equal("Iurii", person.Name);
            Assert.Equal("Podkolzin", person.Surname);
        });
    }
    
    [Fact]
    public async Task BandFilter_RollingStones_ReturnsSingleElement()
    {
        var band = await RequestExecutor.ExecuteRequest<Band>("{'Name': 'The Rolling Stones'}");
        Assert.Single(band);
    }
    
    [Fact]
    public async Task BandFilter_Beatles_ReturnsNothing()
    {
        var band = await RequestExecutor.ExecuteRequest<Band>("{'Name': 'The Beatles'}");
        Assert.Empty(band);
    }

    [Fact]
    public async Task IntFilter_ReturnsRollingStonesMembers()
    {
        var members = await RequestExecutor.ExecuteRequest<Person>("{'Age': {'$gt': 70}}");
        Assert.Equal(4, members.Length);
    }
}
using Nest;

namespace Test;


public class ElasticSearch
{
    private readonly IElasticClient _client;
    public ElasticSearch(IElasticClient client)
    {
        _client = client;
    }
    public async Task<string> Add(Person document)
    {
        var indexResponse = await _client.IndexDocumentAsync(document);
        return indexResponse.Id;
    }
    public async Task<Person> Get(string id)
    {
        var response = await _client.GetAsync<Person>(id);
        return response.Source;
    }
}

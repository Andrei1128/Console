using Nest;
using Test;

var connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("people");
var client = new ElasticClient(connectionSettings);
var elasticsearch = new ElasticSearch(client);

var person = new Person {Name = "Martijn" };

var id = await elasticsearch.Add(person);

await elasticsearch.Get(id);
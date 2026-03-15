using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using dotnetest.Models;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace dotnetest.Tests;

public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UnitTest1(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts()
    {
        var response = await _client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_Valid()
    {
        var product = new Product { Name = "Product Name", Price = 99.99m };
        var productJSON = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

        var response = await _client.PostAsync("/api/products", productJSON);
        var createdProduct = await response.Content.ReadAsStringAsync();
        var ProductDoc = JsonDocument.Parse(createdProduct);
        var createdId = ProductDoc.RootElement.GetProperty("id").GetInt32();
        var createdName = ProductDoc.RootElement.GetProperty("name").GetString();
     
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(createdId > 0);
        Assert.Equal("Product Name", createdName);
    }

    [Fact]
    public async Task CreateProduct_Invalid()
    {
        var product = new Product { Name = "", Price = 0 };
        var productJSON = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

        var response = await _client.PostAsync("/api/products", productJSON);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_Valid()
    {
        // Create product
        var product = new Product { Name = "Product Name", Price = 50.00m };
        var productJSON = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        var productResponse = await _client.PostAsync("/api/products", productJSON);
        var createdProduct = await productResponse.Content.ReadAsStringAsync();
        var createdProductId = JsonDocument.Parse(createdProduct).RootElement.GetProperty("id").GetInt32();
        Assert.True(createdProductId > 0);

        // Create order
        var order = new Order { ProductId = createdProductId, Quantity = 3 };
        var orderJSON = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        var orderResponse = await _client.PostAsync("/api/orders", orderJSON);
        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        var createdOrder = await orderResponse.Content.ReadAsStringAsync();
        var orderDoc = JsonDocument.Parse(createdOrder);
        var createdId = orderDoc.RootElement.GetProperty("id").GetInt32();
        var createdQuantity = orderDoc.RootElement.GetProperty("quantity").GetInt32();
        Assert.True(createdId > 0);
        Assert.Equal(3, createdQuantity);
    }

    [Fact]
    public async Task CreateOrder_Invalid()
    {
        // Create order
        var order = new Order { ProductId = 0, Quantity = 0 };
        var orderJSON = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        var response = await _client.PostAsync("/api/orders", orderJSON);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

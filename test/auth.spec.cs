using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace test;

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterUser_Successfully()
    {
        var user = new
        {
            userId = Guid.NewGuid().ToString(),
            firstName = "John",
            lastName = "Doe",
            email = "west1@gmail.com",
            password = "Password",
            phone = "02345678900"
        };
        var response = await _client.PostAsync("/auth/register", new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
        Assert.Equal("success", (string)responseData.status);
        Assert.Equal("Registration successful", (string)responseData.message);
    }

    [Fact]
    public async Task LoginUser_Successfully()
    {
        var login = new
        {
            email = "west1@gmail.com",
            password = "Password"
        };
        var response = await _client.PostAsync("/auth/login", new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
        Assert.Equal("success", (string)responseData.status);
        Assert.Equal("Login successful", (string)responseData.message);
    }
}


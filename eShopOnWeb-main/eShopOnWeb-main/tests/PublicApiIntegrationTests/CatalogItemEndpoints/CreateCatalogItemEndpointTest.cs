using BlazorAdmin.Models;
using BlazorShared.Models;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.AuthEndpoints;

[TestClass]
public class CreateCatalogItemEndpointTest
{
    private int _testBrandId = 1;
    private int _testTypeId = 2;
    private string _testDescription = "test description";
    private string _testName = "test name";
    private decimal _testPrice = 1.23m;


    [TestMethod]
    public async Task ReturnsNotAuthorizedGivenNormalUserToken()
    {
        var jsonContent = GetValidNewItemJson();
        var token = ApiTokenHelper.GetNormalUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsync("api/catalog-items", jsonContent);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsSuccessGivenValidNewItemAndAdminUserToken()
    {
        var jsonContent = GetValidNewItemJson();
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.PostAsync("api/catalog-items", jsonContent);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<CatalogItemDTO>();

        Assert.AreEqual(_testBrandId, model!.CatalogBrandId);
        Assert.AreEqual(_testTypeId, model.CatalogTypeId);
        Assert.AreEqual(_testDescription, model.Description);
        Assert.AreEqual(_testName, model.Name);
        Assert.AreEqual(_testPrice, model.Price);
    }

    private StringContent GetValidNewItemJson()
    {
        var request = new CreateCatalogItemRequest()
        {
            CatalogBrandId = _testBrandId,
            CatalogTypeId = _testTypeId,
            Description = _testDescription,
            Name = _testName,
            Price = _testPrice
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        return jsonContent;
    }
}

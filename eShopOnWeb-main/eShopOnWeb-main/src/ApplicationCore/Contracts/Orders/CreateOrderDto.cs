using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;
public class CreateOrderDto
{
    [JsonPropertyName("buyer_id")]
    public string BuyerId { get; set; }

    [JsonPropertyName("shiptoaddress_street")]
    public string ShipToAddress_Street { get; set; }

    [JsonPropertyName("shiptoaddress_city")]
    public string ShipToAddress_City { get; set; }

    [JsonPropertyName("shiptoaddress_state")]
    public string ShipToAddress_State { get; set; }

    [JsonPropertyName("shiptoaddress_country")]
    public string ShipToAddress_Country { get; set; }

    [JsonPropertyName("shiptoaddress_zipcode")]
    public string ShipToAddress_ZipCode { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItemDto> Items { get; set; }
}

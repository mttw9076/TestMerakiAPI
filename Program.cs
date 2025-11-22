using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;

string apiKey = "YOUR_API_KEY";
string organizationId = "YOUR_ORG_ID";

var client = new RestClient("https://api.meraki.com/api/v1");
client.AddDefaultHeader("X-Cisco-Meraki-API-Key", apiKey);
client.AddDefaultHeader("Accept", "application/json");

var networks = await GetNetworksAsync(organizationId);
foreach (var network in networks)
{
    Console.WriteLine($"\n=== Sieć: {network.Name} ===");

    var devices = await GetDevicesAsync(network.Id);
    var mxDevices = devices.Where(d => d.Model.StartsWith("MX")).ToList();

    if (!mxDevices.Any())
    {
        Console.WriteLine("Brak urządzeń MX.");
        continue;
    }

    foreach (var device in mxDevices)
    {
        Console.WriteLine($"Urządzenie: {device.Model}, SN: {device.Serial}");

        var uplinks = await GetUplinkStatusAsync(device.Serial);
        foreach (var uplink in uplinks)
        {
            Console.WriteLine($"  {uplink.Interface}: {uplink.Ip} ({uplink.Status})");
        }
    }
}

// === Funkcje pomocnicze ===

async Task<List<Network>> GetNetworksAsync(string orgId)
{
    var request = new RestRequest($"/organizations/{orgId}/networks", Method.Get);
    var response = await client.ExecuteAsync(request);
    return JsonSerializer.Deserialize<List<Network>>(response.Content) ?? new();
}

async Task<List<Device>> GetDevicesAsync(string networkId)
{
    var request = new RestRequest($"/networks/{networkId}/devices", Method.Get);
    var response = await client.ExecuteAsync(request);
    return JsonSerializer.Deserialize<List<Device>>(response.Content) ?? new();
}

async Task<List<Uplink>> GetUplinkStatusAsync(string serial)
{
    var request = new RestRequest($"/devices/{serial}/uplink", Method.Get);
    var response = await client.ExecuteAsync(request);
    return JsonSerializer.Deserialize<List<Uplink>>(response.Content) ?? new();
}

// === Modele danych ===

record Network(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

record Device(
    [property: JsonPropertyName("serial")] string Serial,
    [property: JsonPropertyName("model")] string Model
);

record Uplink(
    [property: JsonPropertyName("interface")] string Interface,
    [property: JsonPropertyName("ip")] string Ip,
    [property: JsonPropertyName("status")] string Status
);
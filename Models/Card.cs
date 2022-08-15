using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text.Json;
namespace LigaMagicAPI.Models;

public class Card{

    [JsonPropertyName("name")]
    public string? Name{get;set;}
    public Dictionary<string, double>? Precos{get; private set;}

    [JsonConstructor]
    public Card(string name){
        Name = name;
        Precos = GetPrices(Name);
    }
    
    public static Card CardFactory(string name){
        Card card = CardFromName(name);
        return card;
    }

    public static Card CardFromName(string name) {
        string? parsedName = ParseName(name);
        var page = GetWebPage($"https://api.scryfall.com/cards/named?fuzzy={parsedName}").Result;
        var card = JsonSerializer.DeserializeAsync<Card>(page).Result;
        if(card!=null){
            return card;
        }
        return new Card("Colossal Dreadmaw");
    }

    public Dictionary<string, double> GetPrices(string name){   
        Dictionary<string, double> prices = new Dictionary<string, double>();

        if(name is null){
            name = "Colossal Dreadmaw";
        }
        var parsedName = ParseName(name);

        var stream = GetWebPage($"https://www.ligamagic.com.br/?view=cards/card&card={parsedName}").Result;
        if(stream is null){
            throw new Exception("Stream error while getting prices.");
        }
        var reader = new StreamReader(stream);
        string data = reader.ReadToEnd();
        if(data==null){
            return new Dictionary<string, double>{};
        }

        var pos1 = data.IndexOf("var g_avgprice=");
        var pos2 = data.IndexOf(";", pos1);
        var range = data.Substring(pos1+16,pos2-pos1-17);
        var prices_json = JObject.Parse(range);
        var sets = prices_json.Children();
        foreach(var set in prices_json){
            pos1 = data.IndexOf($"<option value='{set.Key}'>");
            pos2 = data.IndexOf("<", pos1+1);
            var setName = data.Substring(pos1+16+set.Key.Length+1, pos2-pos1-16-set.Key.Length-1);
            setName = HttpUtility.HtmlDecode(setName);
            var preco = set.Value?.Value<double>("precoMenor");
            
            if(preco!=null && preco!=0){
                prices[setName] = preco.Value;
            }
        }

        return prices;

    }

    private static readonly HttpClient client = new HttpClient();
    public static async Task<Stream> GetWebPage(string url){
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json") { CharSet = "utf-8" });
        var streamTask = await client.GetStreamAsync(url);
        return streamTask;

    }

    private static string? ParseName(string name)
    {
        string parsedName = name.Trim().Replace("%20", " ").Replace("+", " ");
        return parsedName?.Replace(" ", "+").Replace("/", "%2F");
    }
}

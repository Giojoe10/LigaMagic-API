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
    public string? Name{get;private set;}
    public Dictionary<string, double>? Precos{get; private set;}

    [OnDeserialized()]
    internal void OnDecerializedMethod(StreamingContext context){
        Precos = GetPrices();
    }

    [JsonConstructor]
    public Card(string name){
        Name = name;
        
        Precos = GetPrices();
    }
    
    public static Card CardFactory(string name){
        Card card = CardFromName(name).Result;
        return card;
    }


    public async static Task<Card> CardFromName(string name) {
        string? parsedName = ParseName(name);
        var page = await GetWebPage($"https://api.scryfall.com/cards/named?fuzzy={parsedName}");
        var card = await JsonSerializer.DeserializeAsync<Card>(page);
        if(card!=null){
            return card;
        }
        return new Card("Colossal Dreadmaw");
    }


    public void PrintPrecos(){
        if(Precos is null){
            return;
        }

        Console.WriteLine($"Preços para {Name}");
        foreach(var preco in Precos){
                Console.WriteLine(preco.Key+": R$"+preco.Value);
        }
    }
    public double GetMenorPreco(){
        if(Precos is null){
            return -1;
        }
        return Precos.OrderBy(precoSet => precoSet.Value).First().Value;
    }

    public Dictionary<string, double> GetPrices(){
        Dictionary<string, double> prices = new Dictionary<string, double>();

        if(Name is null){
            Name = "Colossal Dreadmaw";
        }
        var parsedName = ParseName(Name);
        //parsedName = Uri.EscapeDataString(parsedName);
        //Console.WriteLine(Name + ": " + parsedName);
        //parsedName = parsedName.Replace(" ", "%20");
        //parsedName = parsedName.Replace("/", "%2F");

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
            
            // var preco = setObj.Value<double>("precomenor");
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
        //client.DefaultRequestHeaders.Add(new NameValueHeaderValue(""))
        var streamTask = await client.GetStreamAsync(url);
        //string page = await stringTask;
        return streamTask;

    }

    private static string? ParseName(string name)
    {
        return name?.Split('/')[0].Trim().Replace(" ", "%20").Replace("/", "%2F");
    }



    // public string GetWebPage(string url){
    //     WebRequest request = WebRequest.Create(url);
    //     request.Method="GET";
    //     HttpWebResponse response = (HttpWebResponse)request.GetResponse();
    //     using(Stream stream = response.GetResponseStream()){
    //         StreamReader reader = new StreamReader(stream);
    //         string? result = reader.ReadToEnd();
    //         reader.Close();
    //         return result;
    //     }
    // }
    
    // private async Task<string>? ScryfallName(string name){
    //     var parsedName = name.Replace(" ", "+");
    //     var url = $"https://api.scryfall.com/cards/named?fuzzy={parsedName}";
    //     var result = GetWebPage(url);
    //     return JObject.Parse(result).Value<string>("name");
    // }
}

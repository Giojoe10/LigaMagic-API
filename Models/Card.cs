using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Text.Json.Serialization;
using System.Text.Json;
namespace LigaMagicAPI.Models;

public class Card{

    [JsonPropertyName("name")]
    public string? Name{get;set;}
    //public Dictionary<string, double>? Precos{get; private set;}
    public Dictionary<string, Dictionary<string, double>>? Precos{get;private set;}
    private Dictionary<string, string> EXTRAS_KEYS{get;} = new Dictionary<string, string>{
				{"2", "Foil"},
				{"3", "Promo"},
				{"5", "Pre-Release"},
				{"7", "FNM"},
				{"11", "DCI"},
				{"13", "Textless"},
				{"17", "Assinada"},
				{"19", "Buy-a-Box"},
				{"23", "Oversize"},
				{"29", "Alterada"},
				{"31", "Foil Etched"},
				{"37", "Misprint"},
				{"41", "Miscut"}				
			};

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

    public Dictionary<string, Dictionary<string,double>> GetPrices(string name){   
        Dictionary<string, Dictionary<string,double>> prices = new Dictionary<string, Dictionary<string,double>>();

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
            throw new Exception("Error loading LigaMagic's data");
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
            
            var set_extras = (JObject?) set.Value?["extras"];

            if(preco!=null && (preco!=0 || set_extras!=null)){
                prices[setName] = new Dictionary<string, double>{{"Normal",preco.Value}};
            }
            
            

            if(set_extras!=null){
                foreach(var extra in set_extras){
                    var extra_object = (JObject?)extra.Value; 
                    if(extra_object!=null){
                       prices[setName][EXTRAS_KEYS[extra.Key]] = extra_object.Value<double>("precoMenor");
                    }
                }
            }else{
                Console.WriteLine("no extras?!");
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
        string parsedName = name.Replace(' ', '+').Split('/')[0];
        return parsedName;
    }
}

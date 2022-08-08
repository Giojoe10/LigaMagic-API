using System.Net;
using Newtonsoft.Json.Linq;
using System.Web;
namespace LigaMagicAPI.Models;

public class Card{

    public string? Name{get;}
    public Dictionary<string, double> Precos{get;}


    public Card(string name){
        Name = ScryfallName(name);
        Precos = GetPrices();
    }


    public void PrintPrecos(){
    Console.WriteLine($"Preços para {Name}");
    foreach(var preco in Precos){
            Console.WriteLine(preco.Key+": R$"+preco.Value);
    }
    }
    public double GetMenorPreco(){
        return Precos.OrderBy(precoSet => precoSet.Value).First().Value;
    }

    public Dictionary<string, double> GetPrices(){
        Dictionary<string, double> prices = new Dictionary<string, double>();
        var parsedName = Name.Replace(" ", "+");

        var data = GetWebPage($"https://www.ligamagic.com.br/?view=cards/card&card={parsedName}");
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
            if(preco!=null){
                prices[setName] = preco.Value;
            }
        }

        return prices;

    }

    public string GetWebPage(string url){
        WebRequest request = WebRequest.Create(url);
        request.Method="GET";
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        using(Stream stream = response.GetResponseStream()){
            StreamReader reader = new StreamReader(stream);
            string? result = reader.ReadToEnd();
            reader.Close();
            return result;
        }
        
    }
    
    private string? ScryfallName(string name){
        var parsedName = name.Replace(" ", "+");
        var url = $"https://api.scryfall.com/cards/named?fuzzy={parsedName}";
        var result = GetWebPage(url);
        return JObject.Parse(result).Value<string>("name");
    }
}

using LigaMagicAPI.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;

namespace LigaMagicAPI.Services;


    
public class LigaMagicAPIService{

    static List<Card> Cards{get;}
    static int nextId = 1;

    static LigaMagicAPIService(){
        Cards = new List<Card>{};
    }


    public static List<Card> GetCards()=>Cards;
    
    public static Card Get(string name){
        return Card.CardFactory(name);
    }

}
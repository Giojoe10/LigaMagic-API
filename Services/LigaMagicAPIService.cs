using LigaMagicAPI.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;

namespace LigaMagicAPI.Services;


    
public class LigaMagicAPIService{
   
    public static Card Get(string name){
        return Card.CardFactory(name);
    }

}
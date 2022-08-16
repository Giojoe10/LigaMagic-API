using Microsoft.AspNetCore.Mvc;
using LigaMagicAPI.Models;
using LigaMagicAPI.Services;
using System.Diagnostics; 

namespace LigaMagicAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LigaMagicAPIController : ControllerBase{

    public LigaMagicAPIController(){
    }

    [HttpGet("{name}")]
    public ActionResult<Card> Get(string name){
        try{
            var card = LigaMagicAPIService.Get(name);
            return card;
        }catch(Exception e){
            Console.WriteLine(e);
            return NotFound();
        }
    }

}
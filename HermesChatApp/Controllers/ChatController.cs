using HermesChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer;
using DataLayer;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace HermesChatApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly ChatOperations _chatOperator;

        public ChatController(ILogger<HomeController> logger, IMapper map, ChatOperations chatOperator)
        {
            _logger = logger;
            _mapper = map;
            _chatOperator = chatOperator;
        }

        public IActionResult Index()
        {
            {
                return View();
            }
        }
    }
}


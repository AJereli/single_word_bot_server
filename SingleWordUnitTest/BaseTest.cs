using SigneWordBotAspCore.Services;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SigneWordBotAspCore.Controllers;
using Telegram.Bot.Types;
using Xunit;
using Context = SigneWordBotAspCore.Services;

namespace SingleWordUnitTest
{
    public abstract class BaseTest
    {
        protected readonly DataBaseService DataBaseService;
        protected readonly Context.AppContext AppContext;
        protected readonly IBotService BotService;
        protected readonly ICommandsService CommandsService;
        protected readonly IUpdateService UpdateService;
        protected readonly UpdateController UpdateController;
        
        
        protected BaseTest()
        {
            AppContext = new Context.AppContext();
            DataBaseService = new DataBaseService(AppContext);
            CommandsService = new CommandsService(DataBaseService);
            BotService = new BotService(AppContext);
            UpdateService = new UpdateService(BotService, null, CommandsService);
            UpdateController = new UpdateController(UpdateService);
            
        }

        protected async Task<Update> GetUpdateFromJson(UpdateJsonType updateJsonType)
        {
            string jsonName = "";
            
            switch (updateJsonType)
            {
                case UpdateJsonType.Default:
                    jsonName = "default_update.json";
                    break;
                case UpdateJsonType.Start:
                    jsonName = "start_request.json";
                    break;
            }
            
            
            var jsonString = await System.IO.File.ReadAllTextAsync(jsonName);

            return JsonConvert.DeserializeObject<Update>(jsonString);

        }

    }
}
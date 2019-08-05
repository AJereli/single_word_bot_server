using System;
using System.Linq;

using Newtonsoft.Json;
using SigneWordBotAspCore.Services;
using Telegram.Bot.Types;
using Xunit;
using Context = SigneWordBotAspCore.Services;

namespace SingleWordUnitTest
{
    public sealed class DataBaseServiceTest: BaseTest
    {
        
        
        
        
        [Fact]
        public async void CreateUserTest()
        {
            var update = await GetUpdateFromJson(UpdateJsonType.Start);

            var firstResult = DataBaseService.CreateUser(update.Message.From, "qwerty");
            
            Assert.NotEqual(expected: -1, actual: firstResult);


            var defaultBasketResult = DataBaseService.CreateBasket(firstResult, "default");
            
            Assert.NotEqual(-1, defaultBasketResult);

            
            var seconResult = DataBaseService.CreateUser(update.Message.From, "qwerty");
            Assert.Equal(expected: -1, actual: seconResult);

            
            
            update.Message.From.Id++;
            update.Message.Chat.Id++;
            update.Message.MessageId++;

            var jsonString = JsonConvert.SerializeObject(update);
            await System.IO.File.WriteAllTextAsync(path: "start_request.json", contents: jsonString);


        }

        [Fact]
        public async void CreateBasket()
        {
            var update = await GetUpdateFromJson(UpdateJsonType.Default);

            var result = DataBaseService.CreateBasket(update.Message.From, "qwerty2");
            
            Assert.NotEqual(-1, result);

            var sameNameResult = DataBaseService.CreateBasket(update.Message.From, "qwerty");
            
            Assert.Equal(-1, sameNameResult);

            var withPass = DataBaseService.CreateBasket(update.Message.From, "WithPass2");
            Assert.NotEqual(-1, withPass);

            var withDesc = DataBaseService.CreateBasket(update.Message.From, "WithDescr2", "pass", "DESCRIPTION HAHAH");
            Assert.NotEqual(-1, withDesc);
        }
    }
}
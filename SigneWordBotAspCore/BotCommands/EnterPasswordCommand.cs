﻿using System;
using System.Threading.Tasks;
using SigneWordBotAspCore.Services;
using SigneWordBotAspCore.States;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class EnterPasswordCommand: IBotCommand
    {

        public string Name => "EnterPasswordCommand";

        public UserStartState AfterState => UserStartState.None;

        public Task Execute(Message message, TelegramBotClient client)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteSql(Message message, TelegramBotClient client, IDataBaseService dbService)
        {
            var userId = dbService.CreateUser(message.Text, message.Chat.Id);
            
            if (userId != -1)
            {
                dbService.CreateBasket(userId, "default", null, "This is default basket for passwords");
                
                await client.SendTextMessageAsync(message.Chat.Id,
                    "Your password created successful\n"+
                    "Now you can user /createCredentials command now" +
                    "Use this template for credentials sending:" +
                    "<login>" +
                    "<password>");
            }

        }
    }
}

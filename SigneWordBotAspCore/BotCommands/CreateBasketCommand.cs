using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.EntityFrameworkCore.Internal;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.EntitiesToTgResponse;
using SigneWordBotAspCore.Exceptions;
using SigneWordBotAspCore.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.BotCommands
{
    public class CreateBasketCommand : AbstractBotCommand
    {
        private readonly IDataBaseService _dataBaseService;

        public CreateBasketCommand(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;

            _name = "/createBasket";
        }

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            var commandPart = message.Text.Split(' ');

            async void SuccessParsedAction(CreateBasketOptions options)
            {
                try
                {
                    var result = _dataBaseService.CreateBasket(message.From, options.Name, options.Password, options.Description);

                    if (result == -1) return;

                    var response = $"Basket with name '{options.Name}' created";
                    await client.SendTextMessageAsync(message.Chat.Id, response);
                }
                catch (BasketAlreadyExistException e)
                {
                    await client.SendTextMessageAsync(message.Chat.Id, e.Message);
                }
            }

            async void FaliedParsedAction(IEnumerable<Error> errors)
            {
                var missingError = errors.Select(e => e as MissingRequiredOptionError)
                    .Where(e => e != null);

                // ReSharper disable once PossibleMultipleEnumeration
                if (!missingError.Any()) return;

                var reqParamsMissing = ResponseFormatter.Default.ErrorResponse(missingError);
                
                if (!string.IsNullOrEmpty(reqParamsMissing))
                    await client.SendTextMessageAsync(message.Chat.Id, reqParamsMissing);
            }

            var res = Parser.Default.ParseArguments<Options.CreateBasketOptions>(commandPart)
                .WithParsed(SuccessParsedAction)
                .WithNotParsed(FaliedParsedAction);
        }
    }
}
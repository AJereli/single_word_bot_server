using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SigneWordBotAspCore.Models;
using SigneWordBotAspCore.Utils;
using SigneWordBotAspCore.BotCommands;
using SigneWordBotAspCore.BotCommands.Options;
using SigneWordBotAspCore.Exceptions;
using Telegram.Bot.Types;
using TgUser = Telegram.Bot.Types.User;

namespace SigneWordBotAspCore.Services
{
    public class DataBaseService: IDataBaseService
    {
        private readonly IAppContext _appContext;
        
        public DataBaseService(IAppContext appContext)
        {
            GetUsers();
        }

        public void GetUsers()
        {
            using (var db = new SwDbContext())
            {

            }
        }
        
        public int CreateUser(User tgUser, string password)
        {
            using (var db = new SwDbContext())
            {
                if (db.User.Any(u => u.TgId == tgUser.Id))
                    return -1;
                
                var res = db.User.Add(new UserModel
                {
                    TgId = tgUser.Id,
                    Password = password,
                    FirstName = tgUser.FirstName,
                    SecondName = tgUser.LastName,
                    TgUsername = tgUser.Username
                });
                
                db.SaveChanges();
                return res.Entity.Id;
            }
        }

        public int CreateBasket(User user, string name, string basketPass = null, string description = null)
        {
            using (var context = new SwDbContext())
            {
                var currentUser = context.User.FirstOrDefault(u => u.TgId == user.Id);

                if (currentUser == null)
                    throw new UserNotFoundException();

                var isBasketExist = context.User.Include(u => u.UserBasket).ThenInclude(u => u.BasketModel)
                    .Any(u => u.UserBasket.Select(ub => ub.BasketModel).Any(b => b.Name == name));
                
                if (isBasketExist)
                    throw new BasketAlreadyExistException();

                var basket = context.PasswordsBasket.Add(new PasswordsBasketModel
                {
                    BasketPass = basketPass,
                    Description = description,
                    Name = name
                    
                });

                var userBasket = context.UserBasket.Add(new UserBasket
                {
                    BasketId = basket.Entity.Id,
                    UserId = currentUser.Id,
                    AccessTypeId = 1
                });
                context.SaveChanges();

                return userBasket.Entity.Id;
            }

        }

        public int CreateBasket(long userId, string name, string basketPass = null, string description = null)
        {
            using (var context = new SwDbContext())
            {
                var currentUser = context.User.FirstOrDefault(u => u.Id == userId);

                if (currentUser == null)
                    throw new UserNotFoundException();

                var isBasketExist = context.User.Include(u => u.UserBasket).ThenInclude(u => u.BasketModel)
                    .Any(u => u.UserBasket.Select(ub => ub.BasketModel).Any(b => b.Name == name));
                
                if (isBasketExist)
                    throw new BasketAlreadyExistException();

                var basket = context.PasswordsBasket.Add(new PasswordsBasketModel
                {
                    BasketPass = basketPass,
                    Description = description,
                    Name = name
                    
                });

                var userBasket = context.UserBasket.Add(new UserBasket
                {
                    BasketId = basket.Entity.Id,
                    UserId = currentUser.Id,
                    AccessTypeId = 1
                });
                context.SaveChanges();

                return userBasket.Entity.Id;
            }
        }

        public int CreateCredentials(User tgUser, AddCredsOption credsOption)
        {
            throw new NotImplementedException();
        }

        public bool UnShareBasket(User user, ShareOptions shareOptions)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CredentialsModel> GetCredentials(User user, ShowCredentialOptions credentialOptions)
        {
            throw new NotImplementedException();
        }

        public ShareResult ShareBasket(User user, ShareOptions shareOptions)
        {
            throw new NotImplementedException();
        }
    }
}
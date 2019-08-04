using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using SigneWordBotAspCore.Models;

namespace SigneWordBotAspCore.EntitiesToTgResponse
{
    public sealed class ResponseFormatter
    {
        private static ResponseFormatter _instance;
        private static readonly object SyncRoot = new object();
 
        private ResponseFormatter() {}


        public string FormatResponse(IEnumerable<CredentialsModel> c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            const string offset = "---";
            
            var praperedStrings = new List<string>();
            
            var title = $"{offset}|Title|{offset}|Login|{offset}|Password|{offset}";

            var grouped = c.GroupBy(cred => cred.BasketName);
            foreach (var g in grouped)
            {
                praperedStrings.Add($"Basket: <b>{g.Key}</b>\n");
                praperedStrings.Add(title);
                praperedStrings.Add(g.Select(s => $"{offset}{s.Name}{offset}{s.Login}{offset}{s.UnitPassword}{offset}")
                                    .Join(Environment.NewLine));
            }

            return praperedStrings.Join(Environment.NewLine);

        }
        
        public static ResponseFormatter Default 
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new ResponseFormatter();
                    }
                }
                return _instance;
            }
            
        }
        
        
        
    }
}
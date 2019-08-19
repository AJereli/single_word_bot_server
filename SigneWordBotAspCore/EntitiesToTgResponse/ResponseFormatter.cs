using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Microsoft.EntityFrameworkCore.Internal;
using SigneWordBotAspCore.Models;
using CLIError = CommandLine.Error;

namespace SigneWordBotAspCore.EntitiesToTgResponse
{
    public sealed class ResponseFormatter
    {
        private static ResponseFormatter _instance;
        private static readonly object SyncRoot = new object();
 
        private ResponseFormatter() {}


        public string ErrorResponse(IEnumerable<CLIError> errors)
        {
            var missingError = errors.Select(e => e as MissingRequiredOptionError)
                .Where(e => e != null);

            // ReSharper disable once PossibleMultipleEnumeration
            if (!missingError.Any()) return null;

            var reqParamsMissing = $"ERROR(S):{Environment.NewLine}";

            reqParamsMissing += missingError.Select(e => $"Required option {e.NameInfo.NameText} is missing.")
                .Join(Environment.NewLine);
            return reqParamsMissing;

        }
        
        public string FormatResponse(IEnumerable<CredentialsModel> c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            const string offset = "---";
            
            var praperedStrings = new List<string>();
            
//            var title = $"{offset}|Title|{offset}|Login|{offset}|Password|{offset}";

            var title = @"<pre>    Title   |  Login  |  Password
  -------- | -------- | ---------|";

            var grouped = c.GroupBy(cred => cred.BasketModelPass.Name);
            
            foreach (var g in grouped)
            {
                praperedStrings.Add($"Basket: <b>{g.Key}</b>\n");
                praperedStrings.Add(title);
                praperedStrings.Add(g.Select(s => $"  {s.Name}  | {s.Login} | {s.UnitPassword}")
                                    .Join(Environment.NewLine));
                praperedStrings.Add("</pre>");

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
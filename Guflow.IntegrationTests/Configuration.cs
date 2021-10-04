// Copyright (c) Gurmit Teotia. Please see the LICENSE file in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;

namespace Guflow.IntegrationTests
{
    public class Configuration
    {
        private readonly Func<string, string> _valueName;
        private Configuration(Func<string, string> valueName)
        {
            _valueName = valueName;
        }

        public static Configuration Build()
        {
            var configurationBuilder = new ConfigurationBuilder();
           
            var builder = configurationBuilder
                                .AddJsonFile("appSettings.json", true, true)
                                .AddJsonFile("appSettings-secrets.json", true, true)
                .SetBasePath(System.AppContext.BaseDirectory)
                .Build();
            
            return new Configuration(n => builder[n]);
        }

        public string this[string name] => _valueName(name);
    }
}
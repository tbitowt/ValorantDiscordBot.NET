using System;
using dotenv.net.Utilities;
using Emzi0767.Utilities;

namespace DiscordBot.Services
{
    public class EnvCheckerService
    {
        private readonly EnvReader _reader;

        public EnvCheckerService(EnvReader reader)
        {
            _reader = reader;
        }

        public string Valorant_UserName { get; private set; }
        public string Valorant_Password { get; private set; }
        public string Discord_Token { get; private set; }
        public string Bot_Prefix { get; private set; }

        public bool CheckEnvironmentValues()
        {
            var readOnlyDictionary = _reader.ToDictionary();

            var result = true;

            try
            {
                Valorant_UserName = GetValue("VALORANT_USERNAME");
            }
            catch (ArgumentException)
            {
                result = false;
            }

            try
            {
                Valorant_Password = GetValue("VALORANT_PASSWORD");
            }
            catch (ArgumentException)
            {
                result = false;
            }

            try
            {
                Discord_Token = GetValue("DISCORD_TOKEN");
            }
            catch (ArgumentException)
            {
                result = false;
            }

            try
            {
                Bot_Prefix = GetValue("BOT_PREFIX");
            }
            catch (ArgumentException)
            {
                result = false;
            }

            return result;
        }

        private string GetValue(string variable)
        {
            string value;
            if (_reader.TryGetStringValue(variable, out value))
            {
                if (value == variable)
                {
                    Console.WriteLine($"Found default value of {variable} data in .env file");
                    throw new ArgumentException($"Found default value of {variable} data in .env file");
                }

                return value;
            }

            Console.WriteLine($"Cannot read {variable} data from .env file");
            throw new ArgumentException($"Found default value of {variable} data in .env file");
        }
    }
}
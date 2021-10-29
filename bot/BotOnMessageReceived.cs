using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using bot.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace bot
{
    public partial class Handlers
    {
        private async Task BotOnMessageReceived(ITelegramBotClient client, Message message)
        {
            if (!await _storage.ExistsAsync(message.Chat.Id))
            {
                var user = new BotUser(
                    chatId: message.Chat.Id,
                    username: message.From.Username,
                    fullname: $"{message.From.FirstName} {message.From.LastName}",
                    latitude: 0,
                    longitude: 0,
                    address: string.Empty,
                    language: string.Empty);

                var result = await _storage.InsertUserAsync(user);

                if (result.IsSuccess)
                {
                    _logger.LogInformation($"New user added: {message.Chat.Id}");
                }
            }
            else
            {
                _logger.LogInformation($"User exists");
            }

            var language = "";
            // var User = new BotUser(message.Chat.Id, message.From.Username, message.From.FirstName + message.From.LastName, message.Location.Longitude, message.Location.Latitude, string.Empty);
            if (message.Type == MessageType.Location && message.Location != null)
            {
                if (await _storage.ExistsAsync(message.Chat.Id))
                {
                    var user = new BotUser(
                            chatId: message.Chat.Id,
                            username: message.From.Username,
                            fullname: $"{message.From.FirstName} {message.From.LastName}",
                            latitude: message.Location.Latitude,
                            longitude: message.Location.Longitude,
                            address: string.Empty,
                            language: _storage.GetUserAsync(message.Chat.Id).Result.Language);
                    var result = await _storage.UpdateUserAsync(user);

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation($"User exists");
                    }
                    else
                    {
                        _logger.LogInformation($"User added");
                    }

                }
                if (_storage.GetUserAsync(message.Chat.Id).Result.Language == "English")
                {
                    await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        "Select day",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: MessageBuilder.MenuShow(_storage.GetUserAsync(message.Chat.Id).Result.Language));
                }
                else if (_storage.GetUserAsync(message.Chat.Id).Result.Language == "Русский")
                {
                    await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        "Выберите день",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: MessageBuilder.MenuShow(_storage.GetUserAsync(message.Chat.Id).Result.Language));
                }
                else if (_storage.GetUserAsync(message.Chat.Id).Result.Language == "O'zbek")
                {
                    await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        "Kunni tanlang",
                        parseMode: ParseMode.Markdown,
                        replyMarkup: MessageBuilder.MenuShow(_storage.GetUserAsync(message.Chat.Id).Result.Language));
                }
            }
            switch (message.Text)
            {
                case "/start":
                    await client.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            parseMode: ParseMode.Markdown,
                            text: "English | O'zbek | Русский?",
                            replyMarkup: MessageBuilder.LanguagesButton());

                    await client.DeleteMessageAsync(
                        chatId: message.Chat.Id,
                        messageId: message.MessageId); break;
                case "English":

                    var userEnglish = await _storage.GetUserAsync(message.Chat.Id);
                    userEnglish.Language = "English";
                    await _storage.UpdateUserAsync(userEnglish);

                    await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        parseMode: ParseMode.Markdown,
                        text: "Assalomu aleykum, Can you share your location?",
                        replyMarkup: MessageBuilder.LocationRequestButton(
                            _storage.GetUserAsync(message.Chat.Id).Result.Language));
                    break;

                case "Русский":
                    var userRussian = await _storage.GetUserAsync(message.Chat.Id);
                    userRussian.Language = "Русский";
                    await _storage.UpdateUserAsync(userRussian);
                    await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        parseMode: ParseMode.Markdown,
                        text: "Ассалому алейкум, Можете отправить вашу локацию?",
                        replyMarkup: MessageBuilder.LocationRequestButton(
                            _storage.GetUserAsync(message.Chat.Id).Result.Language)); break;
                case "O'zbek":
                    var userOzbek = await _storage.GetUserAsync(message.Chat.Id);
                    userOzbek.Language = "O'zbek";
                    await _storage.UpdateUserAsync(userOzbek);
                    await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        parseMode: ParseMode.Markdown,
                        text: "Assalomu aleykum, Lokatsiyangizni jo'nata olasizmi?",
                        replyMarkup: MessageBuilder.LocationRequestButton(
                            _storage.GetUserAsync(message.Chat.Id).Result.Language)); break;
                case "Back":
                    await client.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                parseMode: ParseMode.Markdown,
                                text: "Select language?",
                                replyMarkup: MessageBuilder.LanguagesButton()); break;
                case "Назад":
                    await client.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                parseMode: ParseMode.Markdown,
                                text: "Выберите язык?",
                                replyMarkup: MessageBuilder.LanguagesButton()); break;
                case "Orqaga":
                    await client.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                parseMode: ParseMode.Markdown,
                                text: "Tilni tanglang?",
                                replyMarkup: MessageBuilder.LanguagesButton()); break;
                case "Today":
                    {
                        var result = await _cache.GetOrUpdatePrayerTimeAsync(
                            message.Chat.Id, _storage.GetUserAsync(message.Chat.Id).Result.Latitude,
                            _storage.GetUserAsync(message.Chat.Id).Result.Longitude);
                        var times = result.prayerTime;
                        await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: getTimeString(times, message),
                        parseMode: ParseMode.Markdown,
                        replyMarkup: MessageBuilder.LocationRequestButton(
                            _storage.GetUserAsync(message.Chat.Id).Result.Language));
                    }
                    break;
                case "Сегодняшний":
                    {
                        var result = await _cache.GetOrUpdatePrayerTimeAsync(
                            message.Chat.Id, _storage.GetUserAsync(message.Chat.Id).Result.Latitude,
                             _storage.GetUserAsync(message.Chat.Id).Result.Longitude);
                        var times = result.prayerTime;
                        await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: getTimeString(times, message),
                        parseMode: ParseMode.Markdown,
                        replyMarkup: MessageBuilder.LocationRequestButton(
                            _storage.GetUserAsync(message.Chat.Id).Result.Language));
                    }
                    break;
                case "Bugungi":
                    {
                        var result = await _cache.GetOrUpdatePrayerTimeAsync(
                            message.Chat.Id, _storage.GetUserAsync(message.Chat.Id).Result.Latitude,
                            _storage.GetUserAsync(message.Chat.Id).Result.Longitude);
                        var times = result.prayerTime;
                        await client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: getTimeString(times, message),
                        parseMode: ParseMode.Markdown,
                        replyMarkup: MessageBuilder.LocationRequestButton(
                            _storage.GetUserAsync(message.Chat.Id).Result.Language)
                        );
                    }
                    break;
            }
        }

        public string getTimeString(Models.PrayerTime times, Message message)
        {
            if (_storage.GetUserAsync(message.Chat.Id).Result.Language == "English")
            {
                return $" *Fajr*: {times.Fajr}\n*Sunrise*: {times.Sunrise}\n*Dhuhr*: {times.Dhuhr}\n*Asr*: {times.Asr}\n*Maghrib*: {times.Maghrib}\n*Isha*: {times.Isha}\n\n*Method*: {times.CalculationMethod}";
            }
            else if (_storage.GetUserAsync(message.Chat.Id).Result.Language == "Русский")
            {
                return $" *Фажр*: {times.Fajr}\n*Восход*: {times.Sunrise}\n*Зухр*: {times.Dhuhr}\n*Аср*: {times.Asr}\n*Магриб*: {times.Maghrib}\n*Иша*: {times.Isha}\n\n*Method*: {times.CalculationMethod}";
            }
            return $" *Bomdod*: {times.Fajr}\n*Quyosh chiqishi*: {times.Sunrise}\n*Peshin*: {times.Dhuhr}\n*Asr*: {times.Asr}\n*Shom*: {times.Maghrib}\n*Xufton*: {times.Isha}\n\n*Method*: {times.CalculationMethod}";
        }
    }
}
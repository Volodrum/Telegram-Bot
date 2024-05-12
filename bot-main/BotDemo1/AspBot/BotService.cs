using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AspBot
{
    public class BotService : BackgroundService
    {
        private readonly TelegramBotClient _botClient;
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };
        public BotService()
        {
            _botClient = new TelegramBotClient("6826682038:AAE7bVibHTMxI1iP33JXnOaRT4hbj6NRjPA");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                stoppingToken
            );

            var me = await _botClient.GetMeAsync(stoppingToken);
            Console.WriteLine($"Start listening for @{me.Username}");

            // Wait for the bot to stop
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Process message updates
            if (update.Message is { } message)
            {
                await ProcessMessageAsync(botClient, message, cancellationToken);
            }

            // Process callback queries
            if (update.CallbackQuery is { } callbackQuery)
            {
                await ProcessCallbackQueryAsync(botClient, callbackQuery, cancellationToken);
            }
        }

        async Task ProcessMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            // Only process Message updates: https://core.telegram.org/bots/api#message
            var chatId = message.Chat.Id;
            var user = message.From.FirstName;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} username {user}.");

            switch (messageText)
            {
                case "/start":
                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Hello {user}! Welcome to Shark Envirinment. You can use command /help to see all available commands",
                        cancellationToken: cancellationToken);
                    MenuButtonCommands menu = new MenuButtonCommands();
                    break;
                case "/help":
                    sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Available commands:\n/time\n/keyboard\n/inline\nany message",
                        cancellationToken: cancellationToken);
                    break;
                case "/time":
                    sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Time:\n" + DateTime.Now.ToString("hh:mm:ss"),
                        cancellationToken: cancellationToken);
                    break;
                case "/keyboard":
                    ReplyKeyboardMarkup kyboard = new(new[]{
                new KeyboardButton[]{"/help", "/time", "/inline"}
            })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(chatId, "Choose", replyMarkup: kyboard, cancellationToken: cancellationToken);
                    break;
                case "/inline":
                    InlineKeyboardMarkup keyboard = new(new[] {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Buy 1 BTC", "buy_1_btc"),
                    InlineKeyboardButton.WithCallbackData("Buy 1 ETH", "buy_1_eth"),
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Sell 1 BTC", "sell_1_btc"),
                    InlineKeyboardButton.WithCallbackData("Sell 1 ETH", "sell_1_eth"),
                }
            });

                    await botClient.SendTextMessageAsync(chatId, "Choose inline", replyMarkup: keyboard, cancellationToken: cancellationToken);

                    break;
                default:
                    // Echo received message text
                    sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "You said:\n" + messageText + "\ntime:\n" + DateTime.Now.ToString("hh:mm:ss") + "\nUser:\n" + user,
                        cancellationToken: cancellationToken);
                    break;
            }
        }

        async Task ProcessCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;
            var user = callbackQuery.From.FirstName;

            Console.WriteLine($"Received callback query from chat {chatId} with data '{data}' username {user}.");

            // Respond to the callback query to update the user interaction
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"You selected: {data}", showAlert: false, cancellationToken: cancellationToken);

            // Additional actions based on callback data
            switch (data)
            {
                case "buy_1_btc":
                    await botClient.SendTextMessageAsync(chatId, "Buying 1 BTC...", cancellationToken: cancellationToken);
                    break;
                case "buy_1_eth":
                    await botClient.SendTextMessageAsync(chatId, "Buying 1 ETH...", cancellationToken: cancellationToken);
                    break;
                case "sell_1_btc":
                    await botClient.SendTextMessageAsync(chatId, "Selling 1 BTC...", cancellationToken: cancellationToken);
                    break;
                case "sell_1_eth":
                    await botClient.SendTextMessageAsync(chatId, "Selling 1 ETH...", cancellationToken: cancellationToken);
                    break;
                default:
                    // Handle other callback data or unexpected values
                    break;
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}

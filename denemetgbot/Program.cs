using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace denemetgbot {
    internal class Program {
        static async Task Main(string[] args) {
            string API = "";

            var botClient = new TelegramBotClient(API);

            using CancellationTokenSource cts = new();

            // StartReceiving arayan iş parçacığını engellemez. Alma işlemi ThreadPool'da yapılır.
            ReceiverOptions receiverOptions = new() {
                AllowedUpdates = Array.Empty<UpdateType>() // ChatMember ile ilgili olanlar dışındaki tüm güncelleme türlerini al
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Botu durdurmak için iptal isteği gönder
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
                // Yalnızca Mesaj güncellemelerini işleyin: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;
                // Yalnızca kısa mesajları işleyin
                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                // Echo alınan mesaj metnini aldı
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "You said:\n" + messageText,
                    cancellationToken: cancellationToken);
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
                var ErrorMessage = exception switch {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }
    }
}

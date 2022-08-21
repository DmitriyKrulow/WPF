using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace WPFDis
{
    public partial class MainWindow : Window
    {
        public List<UsersMassage> um = new List<UsersMassage>();
        public List<FileInfoPath> fileList = new List<FileInfoPath>();
        public string TextLine = "";
        public bool AddFlag = false;
        DispatcherTimer timer1 = new DispatcherTimer();
        public static string token = System.IO.File.ReadAllText(@"D:\tokentel.txt");
        public TelegramBotClient bot = new TelegramBotClient(token);
        public MainWindow()
        {
            InitializeComponent();
            string FileJson = System.IO.File.ReadAllText(@"mess.txt");
            if (FileJson == "")
            {
                um.Add(new UsersMassage
                {
                    UsersID = "0",
                    UsersName = "Тестовый пользователь",
                    UsersTextMassage = "Тестовое сообщение"
                });
                TextLine = JsonConvert.SerializeObject(um);
                System.IO.File.WriteAllTextAsync(@"mess.txt", TextLine);
                FileJson = System.IO.File.ReadAllText(@"mess.txt");
            }
            um = JsonConvert.DeserializeObject<List<UsersMassage>>(FileJson);
            PrintFile(@"File");
            ListUsersMassage.ItemsSource = um;
            if (um != null)
            {
                Thread task = new Thread(BackBotAsync);
                task.Start();
                task.IsBackground = true;
            }
            timer1.Tick += new EventHandler(timer_Tick);
            timer1.Interval = new TimeSpan(0, 0, 1);
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            ListUsersMassage.Items.Refresh();
            PrintFile(@"File");
            FileList.Items.Refresh();
            timer1.Stop();
        }
        /// <summary>
        /// Запуск бота в потоке
        /// </summary>
        private void BackBotAsync()
        {
            using var cts = new CancellationTokenSource();
            Debug.WriteLine($"Начало работы бота.");
            bot.StartReceiving(updateHandler: HandleUpdateAsync,
                               pollingErrorHandler: HandlePollingErrorAsync,
                               cancellationToken: cts.Token);
            var me = bot.GetMeAsync();
        }
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            if (update.Message is not { } message)
                return;
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                Debug.WriteLine("Закачивание файла");
                DownloadFile(message.Document.FileId, message.Document.FileName);
                timer1.Start();
            }
            if (message.Text is not { } messageText)
                return;
            var chatId = message.Chat.Id;
            Debug.WriteLine($"Ответ на сообщение '{messageText}' в чат {chatId}.");
            await botClient.SendTextMessageAsync(
                  chatId: chatId,
                  text: "Твое сообщение учтено:\n" + messageText
                  );
            um.Add(new UsersMassage
            {
                UsersID = message.Chat.Id.ToString(),
                UsersName = message.From.FirstName,
                UsersTextMassage = messageText
            });
            TextLine = JsonConvert.SerializeObject(um);
            await System.IO.File.WriteAllTextAsync(@"mess.txt", TextLine);
            timer1.Start();
        }
        async void DownloadFile(string fileID, string path)
        {
            var file = await bot.GetFileAsync(fileID);
            FileStream stream = new FileStream("File/" + path, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, stream);
            stream.Close();
            stream.Dispose();
        }
        /// <summary>
        /// Обработка ошибок бот клиента API
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Debug.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Нажата кнопка в программе!");
            ListUsersMassage.Items.Refresh();
            SendMassage(TargetText.Text, TargetID.Text);
        }
        private void SendMassage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            bot.SendTextMessageAsync(
                    chatId: id,
                    text: Text
                    );
        }
        /// <summary>
        /// Отображение скачанных файлов в таблице
        /// </summary>
        /// <param name="targetDirectory"></param>
        public void PrintFile(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            fileList.Clear();
            foreach (string fileName in fileEntries)
            {
                FileInfo fileInf = new FileInfo(fileName);
                fileList.Add(new FileInfoPath(fileInf.Name, fileInf.Extension, fileInf.Length.ToString()));
            };
            FileList.ItemsSource = fileList;
        }
    }
}

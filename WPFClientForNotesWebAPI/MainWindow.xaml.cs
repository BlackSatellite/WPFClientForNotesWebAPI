using IdentityModel.Client;
using IdentityModel.OidcClient;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WPFClientForNotesWebAPI.Models;

namespace WPFClientForNotesWebAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private HttpClient clientLogin = new HttpClient();

        private HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeOidcClientOptions();
            

            //ConfigureClientLogin();
            //ConfigureClient();

            InitializeComponent();
            InitializeWebBrowser();
        }


        //private void ConfigureClientLogin() 
        //{
        //    clientLogin.BaseAddress = new Uri("http://localhost:44308");
        //    clientLogin.DefaultRequestHeaders.Accept.Clear();
        //    clientLogin.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //}

        //private void ConfigureClient() 
        //{
        //    client.BaseAddress = new Uri("http://localhost:44301");
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //}

        private void LoadNote_Click(object sender, RoutedEventArgs e)
        {
            GetNotes();
        }

        private async void GetNotes() 
        {
            //lblMessage.Content = "";

            string response = await client.GetStringAsync("note");
            List<CreateNoteDto> notes = JsonConvert.DeserializeObject<List<CreateNoteDto>>(response);

            DataGridNote.DataContext = notes;
        }

        //private void SaveNote_Click(object sender, RoutedEventArgs e)
        //{
        //    var note = new Note()
        //    {
        //        Title = TextNoteTitle.Text,
        //        Details = TextNoteDetails.Text
        //    };

        //    SaveNoteAsync(note);

        //    LabelMessage.Content = "Note Saved";
        //}

        //private async void SaveNoteAsync(Note note) 
        //{
        //    await client.PostAsJsonAsync("note", note);
        //}


        private async void  SaveNote_Click(object sender, RoutedEventArgs e)
        {
            string title = TextNoteTitle.Text;
            string details = TextNoteDetails.Text;


            await CreateNoteAsync(title, details);
        }

        //private async Task CreateNoteAsync(string title, string details)
        //{
        //    // Создайте объект для передачи данных
        //    var createNoteDto = new CreateNoteDto
        //    {
        //        Title = title,
        //        Details = details
        //    };

        //    // Преобразуйте объект в JSON
        //    var json = JsonConvert.SerializeObject(createNoteDto);

        //    // Создайте HTTP-клиент
        //    using (var client = new HttpClient())
        //    {
        //        // Установите базовый адрес вашего Web API
        //        client.BaseAddress = new Uri("http://localhost:2309/");

        //        // Определите заголовки, если они необходимы
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // Создайте контент запроса
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        // Отправьте POST-запрос на метод Create
        //        HttpResponseMessage response = await client.PostAsJsonAsync("api/1.0/note", content);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Запись успешно создана
        //            string responseContent = await response.Content.ReadAsStringAsync();
        //            Guid noteId = JsonConvert.DeserializeObject<Guid>(responseContent);
        //            MessageBox.Show($"Запись успешно создана с ID: {noteId}");
        //        }
        //        else
        //        {
        //            // Возникла ошибка при создании записи
        //            MessageBox.Show("Ошибка при создании записи.");
        //        }
        //    }
        //}





        //private async void Login()
        //{
        //    string response = await clientLogin.GetStringAsync("Login");
        //}
        private OidcClientOptions _oidcOptions;
        private OidcClient _oidcClient;
        private OidcClientOptions _oidcOptionsLogin;


        private void InitializeOidcClientOptions()
        {
            _oidcOptions = new OidcClientOptions
            {
                Authority = "https://localhost:44308",
                ClientId = "notes-web-api",
                RedirectUri = "http://localhost:3000/signin-oidc",
                Scope = "openid profile NotesWebAPI",
            };

            _oidcClient = new OidcClient(_oidcOptions);
        }

        private void InitializeWebBrowser()
        {
            webBrowser.Navigate(new Uri(_oidcOptions.Authority));
            webBrowser.Navigated += WebBrowser_Navigated;
        }


        private async void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.StartsWith(_oidcOptions.RedirectUri))
            {
                var client = new HttpClient();
                var disco = await client.GetDiscoveryDocumentAsync("https://localhost:44308");
                if (disco.IsError)
                {
                    // Обработка ошибки
                }
                else
                {
                    var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = "notes-web-api",
                        UserName = "Peter",
                        Password = "qwerty"
                    });

                    if (tokenResponse.IsError)
                    {
                        // Обработка ошибки
                    }
                    else
                    {
                        var accessToken = tokenResponse.AccessToken;
                        // Сохраните токен в вашем WPF приложении для дальнейшего использования.
                    }
                }
            }
        }


        private string returnUrl;
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Login();
            returnUrl = "http://localhost:3000/signin-oidc";
            webBrowser.Navigate(new Uri("https://localhost:44308/Auth/Login?returnUrl=" + returnUrl));


        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            returnUrl = "http://localhost:3000/signin-oidc";
            webBrowser.Navigate(new Uri("https://localhost:44308/Auth/Register?returnUrl=" + returnUrl));
        }

        //-----------------------------------------------------------------------

        private string accessToken; // Добавляем переменную для хранения accessToken


        //private async void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        //{
        //    if (e.Uri.AbsoluteUri.StartsWith(_oidcOptions.RedirectUri))
        //    {
        //        var result = await _oidcClient.ProcessResponseAsync(e.Uri.AbsoluteUri, null);
        //        if (result.IsError)
        //        {
        //            MessageBox.Show($"Ошибка обработки аутентификации: {result.Error}");
        //        }
        //        else
        //        {
        //            MessageBox.Show("Аутентификация успешна!");
        //            // Сохраняем полученный токен
        //            accessToken = result.AccessToken;

        //            webBrowser.Navigate(new Uri(returnUrl));
        //        }
        //    }
        //}



        private async Task CreateNoteAsync(string title, string details)
        {
            // Проверяем, есть ли токен доступа
            if (string.IsNullOrEmpty(accessToken))
            {
                MessageBox.Show("Не выполнена аутентификация. Выполните вход перед созданием записи.");
                return;
            }

            // Создайте объект для передачи данных
            var createNoteDto = new CreateNoteDto
            {
                Title = title,
                Details = details
            };

            // Преобразуйте объект в JSON
            var json = JsonConvert.SerializeObject(createNoteDto);

            // Создайте HTTP-клиент
            using (var client = new HttpClient())
            {
                // Установите базовый адрес вашего Web API
                client.BaseAddress = new Uri("http://localhost:2309/");

                // Определите заголовки, если они необходимы
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Создайте контент запроса
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправьте POST-запрос на метод Create
                HttpResponseMessage response = await client.PostAsJsonAsync("api/1.0/note", createNoteDto);

                if (response.IsSuccessStatusCode)
                {
                    // Запись успешно создана
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Guid noteId = JsonConvert.DeserializeObject<Guid>(responseContent);
                    MessageBox.Show($"Запись успешно создана с ID: {noteId}");
                }
                else
                {
                    // Возникла ошибка при создании записи
                    MessageBox.Show("Ошибка при создании записи.");
                }
            }
        }

    }
}

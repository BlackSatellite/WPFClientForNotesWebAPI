using IdentityModel.Client;
using IdentityModel.OidcClient;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

using WPFClientForNotesWebAPI.Models;

namespace WPFClientForNotesWebAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient client = new HttpClient();

        private OidcClientOptions _oidcOptions;
        private OidcClient _oidcClient;
        private OidcClientOptions _oidcOptionsLogin;
        private string returnUrl;
        private string accessToken; // Добавляем переменную для хранения accessToken
        public MainWindow()
        {
            InitializeOidcClientOptions();

            InitializeComponent();
            InitializeWebBrowser();
        }

        private void InitializeOidcClientOptions()
        {
            _oidcOptions = new OidcClientOptions
            {
                Authority = "https://localhost:44308",
                ClientId = "notes-web-api",
                RedirectUri = "http://localhost:3000/signin-oidc",
                Scope = "openid profile NotesWebAPI",

            };
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

                var httpClient = new HttpClient();
                var discoveryResponse = await httpClient.GetDiscoveryDocumentAsync("https://localhost:44308");

                if (discoveryResponse.IsError)
                {
                    // Обработка ошибки при получении информации о discovery
                }
                else
                {
                    var tokenClient = new TokenClient(
                        httpClient, // HttpMessageInvoker
                        new TokenClientOptions
                        {
                            Address = discoveryResponse.TokenEndpoint,
                            ClientId = "notes-web-api",
                        }
                    );

                    // Запрос на получение токена
                    var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync("NotesWebAPI");

                    if (tokenResponse.IsError)
                    {
                        // Обработка ошибки при получении токена
                    }
                    else
                    {
                        // Токен успешно получен, можно использовать tokenResponse.AccessToken для доступа к защищенным ресурсам.
                        accessToken = tokenResponse.AccessToken;

                        // Теперь вы можете использовать accessToken для доступа к защищенным ресурсам.

                        // Если нужно, вы можете также сохранить refreshToken и другие данные из tokenResponse для будущего использования.
                        var refreshToken = tokenResponse.RefreshToken;
                        var expiresIn = tokenResponse.ExpiresIn;
                    }
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            returnUrl = "http://localhost:3000/signin-oidc";
            webBrowser.Navigate(new Uri("https://localhost:44308/Auth/Login?returnUrl=" + returnUrl));
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            returnUrl = "http://localhost:3000/signin-oidc";
            webBrowser.Navigate(new Uri("https://localhost:44308/Auth/Register?returnUrl=" + returnUrl));
        }

        private async void  SaveNote_Click(object sender, RoutedEventArgs e)
        {
            string title = TextNoteTitle.Text;
            string details = TextNoteDetails.Text;


            await CreateNoteAsync(title, details);
        }


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
                client.BaseAddress = new Uri("https://localhost:44301");
                
                // Определите заголовки, если они необходимы
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Add("ClientId", "NotesWebAPI");

                // Создайте контент запроса
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправьте POST-запрос на метод Create
                HttpResponseMessage response = await client.PostAsJsonAsync("api/1.0/note", createNoteDto);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Guid noteId = JsonConvert.DeserializeObject<Guid>(responseContent);
                    MessageBox.Show($"Запись успешно создана с ID: {noteId}");
                }
                else
                {
                    MessageBox.Show("Ошибка при создании записи.");
                }
            }
        }

        private void LoadNote_Click(object sender, RoutedEventArgs e)
        {
            GetNotes();
        }

        private async void GetNotes()
        {
            string response = await client.GetStringAsync("note");
            List<CreateNoteDto> notes = JsonConvert.DeserializeObject<List<CreateNoteDto>>(response);

            DataGridNote.DataContext = notes;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bc_games
{
    public partial class Form1 : Form
    {
        private const string API_BASE_URL = "https://8xhpiqceodjn.manus.space/api";
        private static readonly HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            string key = Username.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                status.Text = "Por favor, insira a chave de acesso.";
                status.Visible = true;
                return;
            }

            status.Text = "Verificando chave...";
            status.Visible = true;

            guna2Button1.Enabled = false;
            guna2Button1.Text = "Validando...";

            try
            {
                bool isValid = await ValidateKeyAsync(key);

                if (isValid)
                {
                    status.Text = "Chave válida! Acessando sistema...";
                    Form2 main = new Form2();
                    main.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                status.Visible = true;
            }
            finally
            {
                guna2Button1.Enabled = true;
                guna2Button1.Text = "Entrar";
            }
        }

        private async Task<bool> ValidateKeyAsync(string key)
        {
            try
            {
                string hwid = GetHwid();
                string ip = await GetPublicIpAddress();

                var payload = new
                {
                    key = key,
                    hwid = hwid,
                    ip_address = ip
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync($"{API_BASE_URL}/validate-key", content);

                string responseBody = await response.Content.ReadAsStringAsync();
                var validationResponse = JsonConvert.DeserializeObject<KeyValidationResponse>(responseBody);

                if (!validationResponse.success)
                {
                    throw new Exception(validationResponse.error ?? "Erro desconhecido na validação.");
                }

                return validationResponse.valid;
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível conectar ao servidor de validação: {ex.Message}");
            }
        }

        private string GetHwid()
        {
            try
            {
                string cpuInfo = string.Empty;
                string boardInfo = string.Empty;
                string diskInfo = string.Empty;

                System.Management.ManagementClass mc = new System.Management.ManagementClass("win32_processor");
                System.Management.ManagementObjectCollection moc = mc.GetInstances();

                foreach (System.Management.ManagementObject mo in moc)
                {
                    if (cpuInfo == "")
                    {
                        cpuInfo = mo.Properties["processorID"].Value.ToString();
                        break;
                    }
                }

                mc = new System.Management.ManagementClass("Win32_BaseBoard");
                moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    boardInfo = mo.Properties["SerialNumber"].Value.ToString();
                    break;
                }

                mc = new System.Management.ManagementClass("Win32_DiskDrive");
                moc = mc.GetInstances();
                foreach (System.Management.ManagementObject mo in moc)
                {
                    diskInfo = mo.Properties["SerialNumber"].Value.ToString();
                    break;
                }

                string hwidString = cpuInfo + boardInfo + diskInfo;
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(hwidString);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                return "unsupported";
            }
        }

        private async Task<string> GetPublicIpAddress()
        {
            try
            {
                return await httpClient.GetStringAsync("https://api.ipify.org");
            }
            catch
            {
                return "unknown";
            }
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            status.Visible = false;
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Username_TextChanged_1(object sender, EventArgs e)
        {

        }
    }

    public class KeyValidationResponse
    {
        public bool success { get; set; }
        public bool valid { get; set; }
        public string message { get; set; }
        public string error { get; set; }
    }

    public class UserCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}



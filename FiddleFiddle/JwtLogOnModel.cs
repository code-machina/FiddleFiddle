using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static FiddleFiddle.JwtRestClient;

namespace FiddleFiddle
{
    public class JwtLogOnModel : INotifyPropertyChanged
    {

        public class InvalidPasswordException : Exception { }
        public interface IHavePassword
        {
            System.Security.SecureString Password { get; }
        }

        private string _statusMessage = "반갑습니다. Fiddle Fiddle 입니다.";
        public string StatusMessage { get { return _statusMessage; } set { _statusMessage = value; OnPropertyChanged(); } }

        private int _progress = 0;
        public int Progress {
            get { return _progress; }
            set
            {
                _progress = value;
                //OnPropertyChanged();
                OnPropertyChanged("Progress");
            }
        }

        private string _username = "yes24";
        public string Username
        {
            get { return _username; }
            set {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _plainPassword;
        public string PlainPassword
        {
            get { return _plainPassword; }
            set
            {
                _plainPassword = value;
                OnPropertyChanged();
            }
        }

        private string _servername = "yes24.cert.com";
        public string ServerName
        {
            get { return _servername; }
            set
            {
                _servername = value;
                OnPropertyChanged();
            }
        }

        private int _port = 8000;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged();
            }
        }

        private HttpScheme _httpScheme = HttpScheme.HTTP;
        public HttpScheme HttpScheme
        {
            get { return _httpScheme; }
            set
            {
                _httpScheme = value;
                OnPropertyChanged();
            }
        }

        private string _obtainUri = "/api/auth/token/obtain/";
        public string ObtainUri
        {
            get { return _obtainUri; }
            set
            {
                _obtainUri = value;
                OnPropertyChanged();
            }
        }

        private string _refreshUri = "/api/auth/token/refresh/";
        public string RefreshUri
        {
            get { return _refreshUri; }
            set
            {
                _refreshUri = value;
                OnPropertyChanged();
            }
        }

        private string _testUri = "/tests/";
        public string TestUri
        {
            get { return _testUri; }
            set
            {
                _testUri = value;
                OnPropertyChanged();
            }
        }

        // 인증 후 Timeout 이 발동 되며 Token 을 Refresh 한다.
        // 주기를 확인하기 위해 현재 deus-ex-machina 는 1분으로 설정하고 있다.
        private int _refreshTimeoutMin = 1;
        public int RefreshTimeoutMin { get { return _refreshTimeoutMin; } set { _refreshTimeoutMin = value;  OnPropertyChanged(); } }
        private System.Timers.Timer RefreshTimer;
        private JwtAuthenticator authToken;
        public JwtAuthenticator AuthToken { get { return authToken; } set { authToken = value; OnPropertyChanged(); } }
        public bool IsAuthenticated { get; set; }
        private bool _isSkiip = false;
        public bool IsSkip
        {
            get { return _isSkiip; }
            set
            {
                _isSkiip = value;
                OnPropertyChanged();
            }
        }

        private bool _isAuthed = false;
        public bool IsAuthed
        {
            get { return _isAuthed; }
            set
            {
                _isAuthed = value;
                OnPropertyChanged();
            }
        }

        // public ICommand _logonCommand;
        public ICommand LogonCommand {
            get;
            private set;
        }

        public ICommand SkipCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Progressbar 컨트롤과 연동되어 수행되는 BackgroundWorker 인스턴스
        /// </summary>
        private readonly BackgroundWorker Worker;

        /// <summary>
        /// View Control 을 종료하는 대리자 선언
        /// </summary>
        public delegate void CloseLogonPopupDelegate();
        public delegate void DeactivateInputControl();
        public delegate void ActivateInputControl();
        public CloseLogonPopupDelegate CloseLogonPopup { get; set; }
        public DeactivateInputControl DeactivateInput { get; set; }
        public ActivateInputControl ActivateInput { get; set; }

        public JwtLogOnModel() {
            LogonCommand = new RelayCommandEx(Login, o=> !this.Worker.IsBusy);
            SkipCommand = new RelayCommand(Skip);

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += DoWork;
            Worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedEventHandler);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Skip 버튼을 누르면 일반모드로 실행된다. 
        /// Recording 을 하지 않는다.
        /// </summary>
        /// <param name="parameter"></param>
        private void Skip(object parameter)
        {
            // UI 팝업을 어떻게 종료하는가가 문제다.
            CloseLogonPopup();
        }

        private void Login(object parameter)
        {
            DeactivateInput();
            var passwordContainer = parameter as IHavePassword;
            if (passwordContainer != null)
            {
                var secureString = passwordContainer.Password;
                PlainPassword = ConvertToUnsecureString(secureString);
            }

            if (PlainPassword.Count() == 0 || Username.Count() == 0)
            {
                MessageBox.Show("Invalid Input, Plz Check");
                ActivateInput();
            }
            else {
                // Progress Bar Pop up And Authenticate

                // 이벤트 쓰레드를 사용하는 경우, 아래와 같이 Thread 및 For 문을 사용할 경우
                // UI 쓰레드가 UI 를 전혀 업데이트할 수 없으므로 피해야 하는 패턴이다.
                /*
                for(int i = 0; i < 100; i++)
                {
                    Progress += 1;
                    Thread.Sleep(100);
                }*/
                // 테스트 목적으로 생성, BackgroundWokrer 를 통해서 작업을 수행한다.
                // Progress += 10;
                Worker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// JwtLogOnModel 에 설정된 호스트 정보를 기반으로 JwtRestClient 객체를 생성 및 전달
        /// </summary>
        /// <returns></returns>
        public JwtRestClient BuildJwtRestClient()
        {
            return new JwtRestClient()
            {
                Server = ServerName,
                Port = Port,
                Scheme = HttpScheme.HTTP,
                ObtainUri = ObtainUri,
                RefreshUri = RefreshUri,
                TestsUri = TestUri
            };
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            Worker.ReportProgress(30);
            var client = BuildJwtRestClient();
            // SetTimer(); // Start Timer ...
            try
            {
                AuthToken = client.Authenticate(Username, PlainPassword);
                IsAuthenticated = true;
                StatusMessage = "인증에 성공하였습니다.";
                SetTimer(); // Start Timer ...
            }
            catch (JwtAuthenticationException)
            {
                StatusMessage = "잘못된 계정 정보입니다.";
                IsAuthenticated = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                StatusMessage = "인증 과정에 에러가 발생, 서버 연결을 확인하세요.";
                IsAuthenticated = false;
            }
            Worker.ReportProgress(50);
            e.Result = 0;
        }

        /// <summary>
        /// BackgroundWorker 가 진행률을 보고 시에 발생하는 이벤트
        /// 진행률을 업데이트한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }

        /// <summary>
        /// BackgroundWorker 클래스 종료시 발생하는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompletedEventHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            Progress = 100;
            ActivateInput();
        }

        private void SetTimer()
        {
            RefreshTimer = new System.Timers.Timer(RefreshTimeoutMin * 60 * 1000);
            RefreshTimer.Elapsed += async (_sender, _e) =>
            {
                await RefreshTokenTimeout();
            };
            RefreshTimer.AutoReset = true;
            RefreshTimer.Enabled = true;
            RefreshTimer.Start();
        }
        /// <summary>
        /// 비동기 작업 
        /// See, https://docs.microsoft.com/ko-kr/dotnet/standard/parallel-programming/how-to-return-a-value-from-a-task
        /// </summary>
        /// <returns></returns>
        private Task<int> RefreshTokenTimeout()
        {
            FiddleFiddleLogger.FiddleLog(
                string.Format("[FiddleFiddle] [CERT] Time Out Event Occur : {0} Minutes", RefreshTimeoutMin)
            );
            // Set bRefreshTime true
            AuthToken.RefreshTime = true;
            // Locking AuthToken
            lock (AuthToken._locker)
            {
                try
                {
                    var tmp_token = BuildJwtRestClient().Refresh(AuthToken);
                    AuthToken.access = tmp_token.access;
                }
                catch(Exception e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        string.Format("While Refreshing Token : {0}", e.Message)
                    );
                }
            }
            // resume everything
            AuthToken.RefreshTime = false;
            return Task.FromResult(0);
        }

        /// <summary>
        /// This is from MvvmPassword Sample Project
        /// </summary>
        /// <param name="securePassword"></param>
        /// <returns></returns>
        private string ConvertToUnsecureString(System.Security.SecureString securePassword)
        {
            if (securePassword == null)
            {
                return string.Empty;
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FiddleFiddle
{
    /// <summary>
    /// JwtRestClient is dependent on RestSharp Module, To achieve loosely coupled model, I should try some more tests.
    /// </summary>
    public class JwtRestClient
    {
        /// <summary>
        /// 추후의 바인딩을 위해 INotifyPropertyChanged 를 사용한다.
        /// MSDN C# lock Pattern, https://docs.microsoft.com/ko-kr/dotnet/csharp/language-reference/keywords/lock-statement
        /// </summary>
        public class JwtAuthenticator : INotifyPropertyChanged
        {
            #region PrivateMember
            private string _access;
            private string _refresh;
            private bool _refreshTime = false;
            public Object _locker = new Object();

            #endregion // Private Member Region

            /// <summary>
            /// Locking 패턴을 구현한다. 대신 Locker 와 refreshTime 을 두어 조정한다.
            /// </summary>
            public string access {
                get {

                    if (_refreshTime)
                    {
                        lock(_locker)
                        {
                            return _access;
                        }
                    }
                    else
                    {
                        return _access;
                    }
                }
                set
                {
                    if (_refreshTime)
                    {
                        lock (_locker)
                        {
                            _access = value;
                            OnPropertyChanged();
                        }
                    }
                    else
                    {
                        _access = value;
                        OnPropertyChanged();
                    }
                }
            }
            public string refresh
            {
                get
                {
                    if (_refreshTime)
                    {
                        lock (_locker)
                        {
                            return _refresh;
                        }
                    }
                    else
                    {
                        return _refresh;
                    }
                }
                set
                {
                    if (_refreshTime)
                    {
                        lock (_locker)
                        {
                            _refresh = value;
                            OnPropertyChanged();
                        }
                    }
                    else
                    {
                        _refresh = value;
                        OnPropertyChanged();
                    }
                }
            }
            public bool RefreshTime
            {
                get
                {
                    return _refreshTime;
                }
                set
                {
                    _refreshTime = value;
                    OnPropertyChanged();
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// When you refresh your jwt token, DRF return only access token. 
        /// Therefore you define class and then deserialize refresh token, to 
        /// </summary>
        public class JwtRefresher
        {
            public string access;
        }

        public class JwtAuthenticationException : Exception { }
        public class JwtEmptyTokenException : Exception { }
        public class JwtRefreshExpiredException : Exception { }
        public enum HttpScheme
        {
            HTTP = 0,
            HTTPS = 1
        }

        public string Server { get; set; }
        public int Port { get; set; }
        public HttpScheme Scheme { get; set; }
        public JwtAuthenticator Token { get; set; }
        public string ObtainUri { get; set; }
        public string RefreshUri { get; set; }
        public string TestsUri { get; set; }

        /// <summary>
        /// Set up Default Settings.
        /// You can change it after creating instance.
        /// </summary>
        public JwtRestClient()
        {
            Server = "yes24.cert.com"; // ip address is not robust porperties, so we just replace it with hostname, like yes24.cert.com
            Port = 8000;
            Scheme = HttpScheme.HTTP;
            ObtainUri = "/api/auth/token/obtain/";
            RefreshUri = "/api/auth/token/refresh/";
            TestsUri = "/tests/";
            Token = new JwtAuthenticator();
        }

        /// <summary>
        /// Join Server, Port, Scheme members and then make connection string,
        /// Since it depends on User Input, you have to check whether or not server is active.
        /// </summary>
        /// <returns></returns>
        public string Connection()
        {
            string connection = "";
            switch(Scheme)
            {
                case HttpScheme.HTTP:
                    connection = string.Format("{0}://{1}:{2}", "http", Server, Port);
                    break;
                case HttpScheme.HTTPS:
                    connection = string.Format("{0}://{1}:{2}", "https", Server, Port);
                    break;
                default: // default, consider it http scheme
                    connection = string.Format("{0}://{1}:{2}", "http", Server, Port);
                    break;
            }

            return connection;
        }

        public void Authenticate(string username, string password, Action<JwtAuthenticator> Callback)
        {
            throw new NotImplementedException();

            var client = new RestClient(Connection());
            var auth = new RestRequest(ObtainUri, Method.POST) {
                RequestFormat = DataFormat.Json
            };

            auth.AddBody(new
            {
                username = username,
                password = password
            });

            client.ExecuteAsync(auth, (resp) => {
                if (resp.ResponseStatus == ResponseStatus.Completed)
                {
                    var token = JsonConvert.DeserializeObject<JwtAuthenticator>(resp.Content);

//                    TestContext.WriteLine(token.access);
//                    TestContext.WriteLine("access" + token.access);
//                    TestContext.WriteLine("refresh" + token.refresh);
                }
            });
            

            // return new JwtAuthenticator() { };
        }

        public JwtAuthenticator Authenticate(string username, string password)
        {
            var client = new RestClient(Connection());
            var auth = new RestRequest(ObtainUri, Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            auth.AddBody(new
            {
                username = username,
                password = password
            });

            var resp = client.Execute(auth);

            if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<JwtAuthenticator>(resp.Content);
                return token;
            }
            else
            {
                throw new JwtAuthenticationException();
            }
        }

        public JwtAuthenticator Refresh(JwtAuthenticator jwt)
        {
            if (string.IsNullOrEmpty(jwt.access) && string.IsNullOrEmpty(jwt.refresh))
            {
                throw new JwtEmptyTokenException();
            }
            else
            {
                var client = new RestClient(Connection());
                var auth = new RestRequest(RefreshUri, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                auth.AddBody(new
                {
                    refresh = jwt.refresh
                });
                var resp = client.Execute(auth);

                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var token = JsonConvert.DeserializeObject<JwtRefresher>(resp.Content);
                    return new JwtAuthenticator() {
                        access = token.access,
                        refresh = jwt.refresh
                    };
                }
                else
                {
                    /// if Jwt's refresh token was expire, you have to force user to log-in again.
                    throw new JwtRefreshExpiredException();
                }

            }
        }

        /// <summary>
        /// DRF use json data format, We just use object to send data.
        /// This is for Model Based Communication. But there is something huddle to overcome.
        /// How datetime is converted.
        /// See, https://docs.microsoft.com/ko-kr/dotnet/csharp/programming-guide/classes-and-structs/how-to-initialize-a-dictionary-with-a-collection-initializer
        /// 
        /// </summary>
        /// <param name="data"></param>
        public IRestResponse Insert(object data, JwtAuthenticator jwt)
        {
            var client = new RestClient(Connection());

            var req = GetRequest(TestsUri, 
                data,
                Method.POST, 
                DataFormat.Json,
                new Dictionary<string, string>()
                {
                    { "Authorization", string.Format("Bearer {0}", jwt.access) }
                });

            var res = client.Execute(req);

            return res;
        }

        public IRestRequest GetRequest(string uri,
            object body = null,
            Method method=Method.GET, 
            DataFormat dataFormat=DataFormat.Json,
            Dictionary<string, string> Headers=null
            )
        {
            var req = new RestRequest(uri, method)
            {
                RequestFormat = dataFormat
            };

            foreach(var pair in Headers)
            {
                req.AddHeader(pair.Key, pair.Value);
            }

            req.AddBody(body);

            return req;
        }
    }
}

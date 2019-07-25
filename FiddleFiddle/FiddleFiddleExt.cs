using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Mime;
using static FiddleFiddle.JwtRestClient;
using System.Windows.Interop;
using System.Web;
using System.Security.Cryptography;
using System.Windows.Documents.Serialization;

namespace FiddleFiddle
{
    public class FiddleFiddleExt : IAutoTamper
    {
        public readonly Logger logger = FiddlerApplication.Log;
        public JwtAuthenticator Token;
        public bool IsAuthenticated = false;
        public FiddleFiddleModel FiddleModel;

        public FiddleFiddleExt()
        {

        }

        public void OnLoad()
        {
            /* Hosting a WPF Composite Control in Windows Forms
             * https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/walkthrough-hosting-a-wpf-composite-control-in-windows-forms
             */
            FiddleModel = new FiddleFiddleModel();
            FiddleModel.SetupCertPanel();
            FiddleModel.LogOnPopUP();
        }

        public void AutoTamperResponseAfter(Session oSession)
        {

            var oS = oSession; // Shotname
            // 로딩중에 에러가 발생하는 것은 AutoTamperResponseAfter 에서 사용하는 객체들의 Null 체크가 필요함을 의미한다.
            // Check whether or not models are intialized properly.
            if (FiddleModel == null || FiddleModel.LogOnModel == null)
            {
                return;
            }

            // if NotAuthenticatedUser, escape Function
            if (FiddleModel.LogOnModel.AuthToken == null && FiddleModel.LogOnModel.IsAuthenticated == false)
            {
                return;
            }
            
            if (
                oS.hostname == FiddleModel.LogOnModel.ServerName
                && oS.port == FiddleModel.LogOnModel.Port
                && (oS.responseCode != 401 || oS.responseCode != 500)
                )
            {
                oSession["ui-hide"] = "true";
                return;
            }

            // Hide Rest Api Call
            if (oSession.hostname == FiddleModel.LogOnModel.ServerName)
            {
                oSession["ui-hide"] = "true";
                return;
            }

            // Whitelist-based Host Filter
            if (!FiddleModel.CertTabModel.CheckAllowHost(oS.hostname))
            {
                return;
            }

            if (oS.isTunnel || oS.isFTP) 
                return;

            // whitelist-based mimetype checker
            if (oS.oResponse.MIMEType.ToLower().StartsWith("image"))
                return;
            if (oS.oResponse["Content-Type"].ToLower().Contains("javascript"))
                return;
            if (oS.oResponse["Content-Type"].ToLower().Contains("text/css"))
                return;
            if (oS.oResponse["Content-Type"].ToLower().Contains("woff"))
                return;

            // Decode Request & Response to parse transaction and check communication is over properly.
            oS.utilDecodeRequest();
            oS.utilDecodeResponse();
            
            if (oS.hostname != FiddleModel.LogOnModel.ServerName) {
                Task.Factory.StartNew(() => {
                    SendRestRequest(oS, FiddleModel.LogOnModel.AuthToken, FiddleModel.LogOnModel.BuildJwtRestClient());
                });
            }
        }


        /// <summary>
        /// Rules:
        /// 1. All of the Parameter's name is lower-case.
        /// 2. All of the Parameter's value is base64-encoded ascii-bytes-stream
        /// </summary>
        /// <param name="oS"></param>
        /// <param name="auth"></param>
        /// <param name="client"></param>
        public void SendRestRequest(Session oS, JwtAuthenticator auth, JwtRestClient client)
        {
            Uri uri = new Uri(oS.fullUrl);

            string urlKey = string.Format(
                "{0}_{1}{2}", 
                uri.Scheme.ToLower(), // #0
                uri.Host.ToLower(), // #1
                uri.AbsolutePath.Replace("/", "").ToLower() // #2
            ); // sample: https_www.yes24.comtemplatesftlogin.aspx

            List<string> paramKeyList = new List<string>();
            paramKeyList.Add(urlKey);
            bool HasBody = oS.requestBodyBytes.Length > 0 ? true : false; // Check whether or not there is body part.
            Dictionary<string, string> bodyParam = new Dictionary<string, string>();
            Dictionary<string, string> uriParam = new Dictionary<string, string>();
            string paramMd5PHashKey = null;

            Dictionary<string, string> requestDicHeader = new Dictionary<string, string>();
            Dictionary<string, string> responseDicHeader = new Dictionary<string, string>();

            string requestBody = oS.GetRequestBodyAsString();

            // Parse Body Part
            if (oS.oRequest.headers.Exists("Content-Type"))
            {
                if (oS.oRequest.headers["Content-Type"].Contains("application/json"))
                {
                    try
                    {
                        var ConvertedBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
                        foreach (var kv in ConvertedBody)
                        {
                            var originKey = kv.Key.ToString();
                            var lowerKey = kv.Key.ToLower();
                            var encodedVal = Convert.ToBase64String(Encoding.ASCII.GetBytes(kv.Value)); // Plain -> ASCII-Bytes -> Base64-Encoding
                                                                                                        // Key Dup Check
                            if (!bodyParam.ContainsKey(originKey))
                            {
                                bodyParam.Add(originKey, encodedVal);
                            }

                            if(!paramKeyList.Contains(lowerKey))
                            { paramKeyList.Add(lowerKey); }
                        }
                    }
                    catch { /* TODO : Error Reporting Function */ }
                }
                else if (oS.oRequest.headers["Content-Type"].Contains("application/x-www-form-urlencoded"))
                {
                    var queryString = HttpUtility.ParseQueryString(requestBody);
                    if(queryString != null)
                    {
                        foreach(var key in queryString.AllKeys)
                        {
                            var originKey = key;
                            var lowerKey = key.ToLower();
                            var encodedVal = Convert.ToBase64String(Encoding.ASCII.GetBytes(queryString.Get(originKey)));

                            if (!paramKeyList.Contains(lowerKey))
                                paramKeyList.Add(lowerKey);

                            if (!bodyParam.ContainsKey(originKey))
                                bodyParam.Add(originKey, encodedVal);
                        }
                    }
                }
            }

            //Parse Url Part
            var uriQueryString = HttpUtility.ParseQueryString(uri.Query);
            if(uriQueryString != null)
            {
                foreach (var key in uriQueryString.AllKeys)
                {
                    var originKey = key;
                    string lowerKey = key.ToLower();
                    string encodedVal = null;
                    try
                    {
                        encodedVal = Convert.ToBase64String(Encoding.ASCII.GetBytes(uriQueryString.Get(key.ToString())));

                    }catch { /* TODO: Error Reporting Function */ }
                    
                    if (!paramKeyList.Contains(lowerKey))
                        paramKeyList.Add(lowerKey);

                    if (!uriParam.ContainsKey(originKey))
                        uriParam.Add(originKey, encodedVal);
                }
            }

            // Sorting & Calculate MD5 Hash
            var orderedParamKeyList = paramKeyList.OrderBy(x => x);
            string strOrderedParamKeyList = string.Join("|", orderedParamKeyList.ToArray<string>());

            using (MD5 md5Hash = MD5.Create())
            {
                paramMd5PHashKey = GetMd5Hash(md5Hash, strOrderedParamKeyList);
            }

            foreach(var header in oS.RequestHeaders)
            {
                if (!requestDicHeader.ContainsKey(header.Name))
                    requestDicHeader.Add(header.Name, header.Value);
                else
                    requestDicHeader[header.Name] = string.Join("|", requestDicHeader[header.Name], header.Value);
            }

            foreach(var header in oS.ResponseHeaders)
            {
                if (!responseDicHeader.ContainsKey(header.Name))
                    responseDicHeader.Add(header.Name, header.Value);
                else
                    responseDicHeader[header.Name] = string.Join("|", responseDicHeader[header.Name], header.Value);
            }

            try
            {
                var InsertResult = client.Insert(new {
                    // Basic Information
                    method = oS.RequestMethod,
                    full_url = oS.fullUrl,
                    url = oS.RequestHeaders.RequestPath,

                    // Parsed Information
                    req_header = requestDicHeader,
                    res_code = oS.responseCode,
                    res_header = responseDicHeader,
                    url_param = uriParam,
                    body_param = bodyParam,

                    // Client-Side Information
                    client_ip = oS.clientIP,
                    client_port = oS.clientPort,
                    client_process = oS.LocalProcess,

                    // Server-Side Information
                    server_ip = oS.m_hostIP,
                    server_port = oS.port,
                    hostname = oS.hostname,

                    // Additionarl Information
                    is_https = oS.isHTTPS,
                    has_body = HasBody,

                    // Redundant Check Information
                    url_key = urlKey,
                    param_hash_key = paramMd5PHashKey,
                    param_key = strOrderedParamKeyList          
                }, auth);


                if (InsertResult.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    // Fail to create new Record
                    FiddleFiddleLogger.FiddleLog(string.Format(" #error '{2}' >> '{0}' >>> \n _START\n {1} \n_END\n", InsertResult.Content, InsertResult.StatusCode.ToString(), oS.fullUrl));
                }
                else
                {
                    // Success to create new Record
                }
            }
            catch { /* TODO: Error Reporting Function */}

        }

        public void DoYourJob(Session oS, JwtAuthenticator auth, JwtRestClient client)
        {
            // ref1. https://stackoverflow.com/questions/29413942/c-sharp-anonymous-object-with-properties-from-dictionary/29428640#29428640
            // see, `ref1` to dictionary to anonymous object.

            var uri = new Uri(oS.fullUrl);
            var c_url = string.Format("{0}_{1}{2}", uri.Scheme.ToLower(), uri.Host.ToLower(), uri.AbsolutePath.Replace("/", "").ToLower());
            var tmp_param = new List<string>();
            bool has_body = oS.requestBodyBytes.Length > 0 ? true : false;
            var body_param = new Dictionary<string, string>();
            var url_param = new Dictionary<string, string>();


            // Json Request 파싱을 위해 데이터 타입을 체크 필요
            // MimeType Filter 필요, Content-Type: application/json
            if ( oS.oRequest.headers["Content-Type"].Contains("application/json") )
            {
                try
                {
                    var query1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(oS.GetRequestBodyAsString());
                    
                    foreach (var kv in query1)
                    {
                        FiddleFiddleLogger.FiddleLog(
                            string.Format("JsonConvert Result {0}", kv.Key.ToLower())
                        );
                        if (!body_param.ContainsKey(kv.Key.ToLower())){
                            body_param.Add(
                                kv.Key.ToLower(),
                                Convert.ToBase64String(
                                        Encoding.ASCII.GetBytes(
                                            kv.Value
                                        ) // convert UTF8 string to bytes array
                                        // 2018.04.11, ASCII 인코딩으로 전환 뒤 이를 Base64 로 변환하는 것이 유리한 전략이며
                                        // 데이터를 인코딩 혹은 디코딩하기에 유리하다.
                                ) // convert bytes array to base64 string, that is base64-Encoding
                            ); // Add Key-Value Pair to Dictionary
                        }
                        if (!tmp_param.Contains(kv.Key.ToLower()))
                        {
                            tmp_param.Add(kv.Key.ToLower());
                        }
                        
                    }
                }catch(Exception e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        string.Format("[CERT] While Json Converting, {0}", e.Message)
                    );
                    FiddleFiddleLogger.FiddleLog(
                        // 인덱싱 실수를 하지 않기 위해 인덱스 번호를 마킹하는 것을 좋은 습관이다.
                        string.Format("Continuation >> {0} {1} {2} {3}",
                            oS.hostname,  // 0
                            oS.port,  // 1
                            oS.responseCode,  // 2
                            oS.url // 3
                        )
                    );
                }

                // json_body_param = new Dictionary(json_body_param.Intersect(query1));
            }
            else if (oS.RequestMethod.ToLower() == "post" || oS.oRequest.headers["Content-Type"].Contains("application/x-www-form-urlencoded"))
            {// POST Parameter 추출
                    try
                {
                    var body = HttpUtility.ParseQueryString(oS.GetRequestBodyAsString());
                    // 디코딩 오류를 극복하기 위한 전략이 필요하다.
                    var query2 = body != null ? body:null;
                    // query2 = HttpUtility.ParseQueryString(oS.GetRequestBodyAsString(oS.GetRequestBodyEncoding().GetDecoder());
                    // oS.GetRequestBodyEncoding().GetString(ref oS.requestBodyBytes, oS.requestBodyBytes.Length);
                    // 디코딩 전략, 디코딩에 실패하는 데이터는 버린다. 
                    // 굳이 Decoding 을 여기서 할 필요가 없다. Base64 Encoding 후에 Viewer 에서 처리하도록 지연시킨다. 
                    // 아래의 링크를 참조바란다. 
                    // Eric Lawrence, https://www.telerik.com/forums/request-body-encoding
                    if (query2 != null)
                    {
                        foreach (var item in query2)
                        {
                            tmp_param.Add(item.ToString().ToLower());
                            body_param.Add(
                                item.ToString(),
                                Convert.ToBase64String(
                                    Encoding.UTF8.GetBytes(
                                        query2[item.ToString()]
                                    ) // convert UTF8 string to bytes array
                                ) // convert bytes array to base64 string, that is base64-Encoding
                            ); // Add Parameter Key and Value to Dictionary
                        }
                    }
                }
                catch (Exception e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        string.Format("[CERT] While POST Parmeter Converting, {0}", e.Message)
                    );
                    FiddleFiddleLogger.FiddleLog(
                        // 인덱싱 실수를 하지 않기 위해 인덱스 번호를 마킹하는 것을 좋은 습관이다.
                        string.Format("Continuation >> {0} {1} {2} {3}",
                            oS.hostname,  // 0
                            oS.port,  // 1
                            oS.responseCode,  // 2
                            oS.url // 3
                        )
                    );
                }
            }

            // GET Paramter 추출
            var query3 = HttpUtility.ParseQueryString(uri.Query);
            foreach (var item in query3)
            {
                tmp_param.Add(item.ToString().ToLower());
                // query3[item.ToString()];
                url_param.Add(
                    item.ToString(),
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                           query3[item.ToString()]
                        ) // convert UTF8 string to bytes array
                    ) // convert bytes array to base64 string, that is base64-Encoding);
                );
            }
            // 정렬 
            var ordered_param = tmp_param.OrderBy(x => x);

            string source = string.Join("|", ordered_param);
            string hash = "";
            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, source);
            }

            // String expires_in = HttpUtility.ParseQueryString(uri.Query)

            var req_head = new Dictionary<string, string>();
            foreach (var item in oS.RequestHeaders)
            {
                req_head.Add(item.Name, item.Value);
            }

            var res_head = new Dictionary<string, string>();
            // Dictionary 변환에 어려움이 있음, 해결이 필요하다.
            /// TODO: Dictionary 변환 작업 필요 Set-Cookie 의 경우 동일한 키 이름에 중복된 값이 존재
            
            foreach(var item in oS.ResponseHeaders)
            {
                if(res_head.ContainsKey(item.Name))
                {
                    res_head[item.Name] = String.Join("|", res_head[item.Name], item.Value);
                }
                else
                {
                    res_head[item.Name] = item.Value;
                }
            }

            var res = client.Insert(new
            {
                req_header = req_head,
                method = oS.RequestMethod,
                full_url = oS.fullUrl,
                url = oS.RequestHeaders.RequestPath, 
                hostname = oS.hostname,
                url_param = url_param,
                body_param = body_param,
                client_ip = oS.clientIP,
                client_port = oS.clientPort,
                server_ip = oS.m_hostIP,
                server_port = oS.port,
                client_process = oS.LocalProcess,
                res_header = res_head,
                is_https = oS.isHTTPS,
                has_body = has_body,
                url_key=c_url,
                param_hash_key = hash,
                param_key= source
            },
            auth);

            if (res.StatusCode != System.Net.HttpStatusCode.Created)
                FiddleFiddleLogger.FiddleLog(
                        res.Content + res.StatusCode.ToString()
                      );



        }

        #region DEBUG

        /// <summary>
        /// It represent a formatted debugging messages. you could check the object's properties.
        /// Be cautious when you handle `TargetParameterCountException`. 
        /// these exceptions will be rasied when you try to access some properties. 
        /// I don't know the reason why this exception occur occasionally.
        /// But, I guess that the issue of security prevent a code to access the properties' value.
        /// </summary>
        /// <param name="ob">Target Object</param>
        /// <param name="props"></param>
        public void DebugObjectsPropertiesUsingReflection(object ob, string [] props)
        {
            FiddleFiddleLogger.FiddleLog(
                          String.Format(
                              "Try to access {0}",
                              ob.GetType().Name
                          )
                      );

            List<KeyValuePair<string, object>> Prefs = new List<KeyValuePair<string, object>>();
            foreach (var prop in ob.GetType().GetProperties())
            {
                if (!props.Contains(prop.Name))
                    continue; // Jump to Next

                try
                {
                    // Check for Type and Conversion to present data properly
                    // It is responsible for decoding ResponseBody, RequestBody
                    if (prop.PropertyType == typeof(Byte[]))
                    {
                        FiddleFiddleLogger.FiddleLog(
                            String.Format(
                                "Try to Convert Byte[] `{0}` Property into Hexlified String",
                                prop.Name
                            )
                        );

                        Prefs.Add(
                            new KeyValuePair<string, object>(
                                prop.Name,
                                FiddleFiddleHex.ConvertByteArrayToHexlifiedStringEx(
                                    (byte[])prop.GetValue(ob, null), 
                                    15
                                )
                                // Below Conversion is too non-responsible for the consequences of the action.
                                // Encoding.UTF8.GetString(
                                //     (byte[])prop.GetValue(ob, null)
                                // )
                            )
                        );
                    }
                    else
                    {
                        FiddleFiddleLogger.FiddleLog(String.Format("Try to Resolve `{0}` Property", prop.Name));
                        Prefs.Add(new KeyValuePair<string, object>(prop.Name, prop.GetValue(ob, null)));
                    }
                }
                catch (TargetParameterCountException e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        String.Format("While Resolving `{0}` Property, \n Error Occurred : {1}",
                        prop.Name,
                        e.Message));
                }
                catch (Exception e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        String.Format("[CAUTION][DANGER] Not Filtered Explicitly!! `{0}` Property, \n Error Occurred : {1}",
                        prop.Name,
                        e.Message));
                }
            }
            FiddleFiddleLogger.FiddleDebugLogWithPairs(Prefs.ToArray());
        }
        /// <summary>
        /// Suddenly, The question comes up. it is about "What differences between Property and Field".
        /// It has some points. 
        /// when is it inialized, 
        /// or It must be initialized?
        /// </summary>
        /// <param name="ob"></param>
        public void DebugObjectsFieldsUsingReflection(object ob)
        {
            FiddleFiddleLogger.FiddleLog(
                        String.Format(
                            "Try to access {0}",
                            ob.GetType().Name
                        )
                    );
            List<KeyValuePair<string, object>> Prefs = new List<KeyValuePair<string, object>>();
            foreach (var field in ob.GetType().GetFields())
            {
                try
                {
                    if (field.FieldType == typeof(byte[]))
                    {
                        FiddleFiddleLogger.FiddleLog(
                            String.Format(
                                "Try to Convert Byte[] `{0}` Property into UTF8.String",
                                field.Name
                            )
                        );

                        Prefs.Add(
                            new KeyValuePair<string, object>(
                                field.Name,
                                FiddleFiddleHex.ConvertByteArrayToHexlifiedStringEx(
                                    (byte[])field.GetValue(ob),
                                    15
                                )
                            )
                        );
                    }
                    else
                    {
                        FiddleFiddleLogger.FiddleLog(String.Format("Try to Resolve `{0}` Property", field.Name));
                        Prefs.Add(new KeyValuePair<string, object>(
                                field.Name,
                                field.GetValue(ob)
                            )
                        );
                    }
                }catch(Exception e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        String.Format("While Resolving `{0}` Fields, \n `{2}` Error Occurred : {1}",
                        field.Name,
                        e.Message,
                        e.GetType().FullName
                        )
                    );
                }

            }
            FiddleFiddleLogger.FiddleDebugLogWithPairs(Prefs.ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ob"></param>
        public void DebugObjectsPropertiesUsingReflection(object ob)
        {
            FiddleFiddleLogger.FiddleLog(
                          String.Format(
                              "Try to access {0}",
                              ob.GetType().Name
                          )
                      );
            List<KeyValuePair<string, object>> Prefs = new List<KeyValuePair<string, object>>();
            foreach (var prop in ob.GetType().GetProperties())
            {
                try
                {
                    // Check for Type and Conversion to present data properly
                    // It is responsible for decoding ResponseBody, RequestBody
                    if (prop.PropertyType == typeof(Byte[]))
                    {
                        FiddleFiddleLogger.FiddleLog(
                            String.Format(
                                "Try to Convert Byte[] `{0}` Property into UTF8.String",
                                prop.Name
                            )
                        );

                        Prefs.Add(
                            new KeyValuePair<string, object>(
                                prop.Name,
                                FiddleFiddleHex.ConvertByteArrayToHexlifiedStringEx(
                                    (byte[])prop.GetValue(ob, null),
                                    15
                                )
                            )
                        );
                    }
                    else
                    {
                        FiddleFiddleLogger.FiddleLog(String.Format("Try to Resolve `{0}` Property", prop.Name));
                        Prefs.Add(new KeyValuePair<string, object>(prop.Name, prop.GetValue(ob, null)));
                    }
                }
                catch (TargetParameterCountException e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        String.Format("While Resolving `{0}` Property, \n Error Occurred : {1}",
                        prop.Name,
                        e.Message));
                }
                catch (Exception e)
                {
                    FiddleFiddleLogger.FiddleLog(
                        String.Format("[CAUTION][DANGER] Not Filtered Explicitly!! `{0}` Property, \n Error Occurred : {1}",
                        prop.Name,
                        e.Message));
                }
            }
            FiddleFiddleLogger.FiddleDebugLogWithPairs(Prefs.ToArray());
        }
        #endregion DEBUG
        
        #region NotImplemented
        public void AutoTamperResponseBefore(Session oSession)
        {
            // throw new NotImplementedException();
        }
        public void OnBeforeReturningError(Session oSession)
        {
            // throw new NotImplementedException();
        }
        public void OnBeforeUnload()
        {
            // throw new NotImplementedException();
        }
        public void AutoTamperRequestAfter(Session oSession)
        {
            // throw new NotImplementedException();
        }
        public void AutoTamperRequestBefore(Session oSession)
        {
            // throw new NotImplementedException();
        }

        #endregion Not Implemented
        
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public class FiddleUtil
        {
            /// <summary>
            /// Convert a string from one charset to another charset
            /// </summary>
            /// <param name="strText">source string</param>
            /// <param name="strSrcEncoding">original encoding name</param>
            /// <param name="strDestEncoding">dest encoding name</param>
            /// <returns></returns>
            public static String StringEncodingConvert(String strText, String strSrcEncoding, String strDestEncoding)
            {
                System.Text.Encoding srcEnc = System.Text.Encoding.GetEncoding(strSrcEncoding);
                System.Text.Encoding destEnc = System.Text.Encoding.GetEncoding(strDestEncoding);
                byte[] bData = srcEnc.GetBytes(strText);
                byte[] bResult = System.Text.Encoding.Convert(srcEnc, destEnc, bData);
                return destEnc.GetString(bResult);
            }
        }

    }
}

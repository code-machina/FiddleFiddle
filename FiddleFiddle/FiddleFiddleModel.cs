using Fiddler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;

namespace FiddleFiddle
{

    public static class MimeTypeFilters
    {
        public static class Text
        {

        }
    }

    /// <summary>
    /// Construct Object and Serialize it into Json or Any Type
    /// var Model = new FiddleFiddleModel(session);
    /// string data = Model.SerializeToJson();
    /// 
    /// </summary>
    public partial class FiddleFiddleModel
    {

        public JwtLogOnModel LogOnModel { get; set; }
        public Session FiddleSession { get; set; }
        private JwtLogOnControl LogonControlView { get; set; }
        public FiddleFiddleCertTabModel CertTabModel {get;set;}

        public FiddleFiddleModel(Session session)
        {

        }

        public ICommand LogOnCommand { get; set; }

        public FiddleFiddleModel() {
            LogonControlView = new JwtLogOnControl();
            LogOnModel = new JwtLogOnModel();
            CertTabModel = new FiddleFiddleCertTabModel();
            LogOnCommand = new RelayCommand(WrapLogOnPopUP);
        }

        private void WrapLogOnPopUP(object param)
        {
            LogonControlView = new JwtLogOnControl();
            var window = LogonControlView;
            var helper = new WindowInteropHelper(window);
            window.Height = 550;
            window.Width = 450;

            window.DataContext = LogOnModel;
            LogOnModel.CloseLogonPopup = window.Close;
            LogOnModel.DeactivateInput = DeactivateLogOnPopup;
            LogOnModel.ActivateInput = ActivateLogOnPopup;
            // MainApplication 실행 후에 팝업을 호출하기 위해서는 
            // 아래와 같이 Owner 를 지정해 주어야 한다. 그렇지 않으면 오류가 발생
            helper.Owner = FiddlerApplication.UI.Handle;
            window.ShowDialog();
        }

        public void SetupCertPanel()
        {
            var panel = new CertPanel();
            panel.DataContext = this;
            panel.InitializeComponent();
            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = panel
            };

            TabPage CertTab = new TabPage
            {
                Name = "FiddleFiddle",
                Text = "FiddleFiddle",
                ImageIndex = (int)Fiddler.SessionIcons.JSON,
            };

            CertTab.Controls.Add(host);

            // http://docs.telerik.com/fiddler/Extend-Fiddler/AddIcon
            // TabPage 에 탭을 추가하고 타이틀 지정하기
            FiddlerApplication.UI.tabsViews.TabPages.Add(CertTab);
        }

        public void LogOnPopUP() {
            var window = LogonControlView;
            var helper = new WindowInteropHelper(window);
            window.Height = 550;
            window.Width = 450;

            window.DataContext = LogOnModel;
            LogOnModel.CloseLogonPopup = window.Close;
            LogOnModel.DeactivateInput = DeactivateLogOnPopup;
            LogOnModel.ActivateInput = ActivateLogOnPopup;
            // helper.Owner = FiddlerApplication.UI.Handle;
            window.ShowDialog();
        }

        public void DeactivateLogOnPopup()
        {
            var window = LogonControlView;
            window.Login.IsEnabled = false;
            window.Skip.IsEnabled = false;
            window.UserPassword.IsEnabled = false;
            window.UsernameTextbox.IsEnabled = false;
        }

        public void ActivateLogOnPopup()
        {
            var window = LogonControlView;
            window.Login.IsEnabled = true;
            window.Skip.IsEnabled = true;
            window.UserPassword.IsEnabled = true;
            window.UsernameTextbox.IsEnabled = true;
        }
    }
        
    public partial class FiddleFiddleModel
    {
        /* RequestHeader 처리 메서드 중심으로 구현
         * 
         */
    }

    public partial class FiddleFiddleModel
    {
        /* ResponseHeader 처리 메서드 중심으로 구현
         * 
         */
    }

    public partial class FiddleFiddleModel
    {
        /* Url 파싱 및 Parameter 시그니처 추출 작업
         * 
         */
    }

}

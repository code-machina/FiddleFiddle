using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Diagnostics;

namespace FiddleFiddle
{
    /// <summary>
    /// CertPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CertPanel : StackPanel
    {
        public CertPanel()
        {
            InitializeComponent();
        }

        private void Init(object sender, EventArgs e)
        {
            // ElementHost 에 Child 로 등록하기 위해서 정의해야 하는 메서드이다.
            
            FiddleFiddleLogger.FiddleLog("[CERT] Success Call CertPanel.Init");

        }

    }

    public class CertPanelEventArgs : EventArgs
    {

    }
}

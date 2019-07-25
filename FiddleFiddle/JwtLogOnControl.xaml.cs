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
using static FiddleFiddle.JwtLogOnModel;

namespace FiddleFiddle
{
    /// <summary>
    /// Interaction logic for JwtLogOnControl.xaml
    /// </summary>
    public partial class JwtLogOnControl : Window, IHavePassword
    {
        public JwtLogOnControl()
        {
            InitializeComponent();
        }

        public System.Security.SecureString Password
        {
            get
            {
                return UserPassword.SecurePassword;
            }
        }
    }
}

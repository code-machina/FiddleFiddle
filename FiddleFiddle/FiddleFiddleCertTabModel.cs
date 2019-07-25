using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FiddleFiddle
{
    public class FiddleFiddleCertTabModel : INotifyPropertyChanged
    {
        // 비동기 참조 Collection
        // 다수의 쓰레드의 참조에 변경되지 않음
        // https://docs.microsoft.com/ko-kr/dotnet/standard/collections/thread-safe/how-to-use-foreach-to-remove
        // Cache 데이터 매칭이 필요
        // IDisposable 을 통해 Serialize/Deserialize 케이스를 알고 있음
        private BlockingCollection<TargetHost> _whiteCollection;
        public BlockingCollection<TargetHost> WhiteCollection {
            get { return _whiteCollection; }
            set {
                _whiteCollection = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<TargetHost> hostCollectionView;
        public ObservableCollection<TargetHost> HostCollectionView
        {
            get { return hostCollectionView; }
            set {
                hostCollectionView = value;
                OnPropertyChanged();
            }
        }

        public class TargetHost
        {
            public string hostname { get; set; }
            public bool block { get; set; }
            public string comment { get; set; }
        }

        public bool CheckAllowHost(string hostname)
        {
            if (HostCollectionView.Where(x => x.hostname == hostname && !x.block).Count() > 0)
                return true;
            else
                return false;
        }

        private string hostname;
        public string InputHostname
        {
            get { return hostname;  }
            set { hostname = value; OnPropertyChanged(); }
        }
        private string comment;
        public string InputComment
        {
            get { return comment; }
            set { comment = value; OnPropertyChanged(); }
        }
        private bool inputBlockRadio;
        public bool InputBlockRadio
        {
            get { return inputBlockRadio; }
            set { inputBlockRadio = value; OnPropertyChanged(); }
        }

        private bool inputAllowRadio;
        public bool InputAllowRadio
        {
            get { return inputAllowRadio; }
            set { inputAllowRadio = value; OnPropertyChanged(); }
        }

        public ICommand RegisterHostCommand { get; private set; }
        public ICommand RemoveHostCommand { get; private set; }
        public TargetHost SelectedItem { get; set; }
        public FiddleFiddleCertTabModel()
        {
            RegisterHostCommand = new RelayCommand(RegisterHost);
            RemoveHostCommand = new RelayCommand(RemoveHost);
            WhiteCollection = new BlockingCollection<TargetHost>();
            HostCollectionView = new ObservableCollection<TargetHost>();

            HostCollectionView.Add(
                new TargetHost {
                    hostname = "www.yes24.com", block=false, comment="기본 설정"
                });
            HostCollectionView.Add(
                new TargetHost {
                    hostname = "shiftbooks.yes24.com", block = false, comment = "기본 설정"
                });
            HostCollectionView.Add(
                new TargetHost {
                    hostname = "ssl.yes24.com", block = false, comment = "기본 설정"
                });
            HostCollectionView.Add(
                new TargetHost {
                    hostname = "movie.yes24.com", block = false, comment = "기본 설정"
                });
            HostCollectionView.Add(
                new TargetHost {
                    hostname = "ticket.yes24.com", block = false, comment = "기본 설정"
                });
            HostCollectionView.Add(
                new TargetHost {
                    hostname = "uscm.yes24.com", block = false, comment = "기본 설정"
                });
            HostCollectionView.Add(
                new TargetHost {
                    hostname = "blog.yes24.com", block = false, comment = "기본 설정"
                });

        }
        private void RemoveHost(object selected)
        {
            if (SelectedItem != null)
            {
                HostCollectionView.Remove(SelectedItem);
            }
            else
            {
                MessageBox.Show("선택한 항목이 없습니다.");
            }
        }
        private void RegisterHost(object parameter)
        {
            if (!(HostCollectionView.Where( x => x.hostname == InputHostname).Count() > 0))
            {
                HostCollectionView.Add(new TargetHost { hostname = InputHostname, block = InputAllowRadio ? false: true, comment= InputComment});
                OnPropertyChanged();
            }
            else
            {
                MessageBox.Show(string.Format("{0} : 이미 등록된 호스트 입니다.", InputHostname));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

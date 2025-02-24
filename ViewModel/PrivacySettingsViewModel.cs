using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using taskConnectProject.Views;

namespace taskConnectProject.ViewModel
{
    public class PrivacySettingsViewModel : INotifyPropertyChanged
    {
        private bool _searchPrivacy;
        private bool _storeContacts;
        private bool _useSitesYouVisit;
        private bool _useYourActivity;
        private bool _personalizedAds;
        private bool _autoplayVideos;
        private bool _clearAppCache;

        public bool SearchPrivacy { get => _searchPrivacy; set => SetProperty(ref _searchPrivacy, value); }
        public bool StoreContacts { get => _storeContacts; set => SetProperty(ref _storeContacts, value); }
        public bool UseSitesYouVisit { get => _useSitesYouVisit; set => SetProperty(ref _useSitesYouVisit, value); }
        public bool UseYourActivity { get => _useYourActivity; set => SetProperty(ref _useYourActivity, value); }
        public bool PersonalizedAds { get => _personalizedAds; set => SetProperty(ref _personalizedAds, value); }
        public bool AutoplayVideos { get => _autoplayVideos; set => SetProperty(ref _autoplayVideos, value); }
        public bool ClearAppCache { get => _clearAppCache; set => SetProperty(ref _clearAppCache, value); }

        public event PropertyChangedEventHandler PropertyChanged;
        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


}

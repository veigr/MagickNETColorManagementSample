using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageMagick;
using Livet;

namespace MagickNETColorManagementSample.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public void Initialize()
        {
            const string profilesPath = @"C:\Windows\System32\spool\drivers\color";
            var profiles = Directory.GetFiles(profilesPath);
            this.LocalProfiles = profiles
                .Where(x => Path.GetExtension(x) == ".icc" || Path.GetExtension(x) == ".icm")
                .Select(x => new ICCProfile(Path.GetFileName(x), new ColorProfile(x)))
                .Concat(new[]
                {
                    new ICCProfile("[preset]AdobeRGB1998", ColorProfile.AdobeRGB1998),
                    new ICCProfile("[preset]AppleRGB", ColorProfile.AppleRGB),
                    new ICCProfile("[preset]CoatedFOGRA39", ColorProfile.CoatedFOGRA39),
                    new ICCProfile("[preset]ColorMatchRGB", ColorProfile.ColorMatchRGB),
                    new ICCProfile("[preset]SRGB", ColorProfile.SRGB),
                    new ICCProfile("[preset]USWebCoatedSWOP", ColorProfile.USWebCoatedSWOP),
                })
                .ToArray();

            this.BlackPointCompensation = true;
            this.RenderingIntent = default(RenderingIntent);
            this.SubstituteSourceProfile = this.LocalProfiles
                .FirstOrDefault(x => x.Name.ToUpper().Contains("SRGB"));
            this.DestinationProfile = this.LocalProfiles
                .FirstOrDefault(x => x.Name.ToUpper().Contains("SRGB"));
        }

        public void Drop(object sender, DragEventArgs e)
        {
            if (!(e.Data is DataObject)) return;

            var data = (DataObject)e.Data;
            if (!data.ContainsFileDropList()) return;

            var files = data.GetFileDropList().Cast<string>().ToArray();
            if (!files.Any()) return;

            this.ImageFilePath = files.First();
        }


        #region ImageFilePath変更通知プロパティ
        private string _ImageFilePath;

        public string ImageFilePath
        {
            get
            { return this._ImageFilePath; }
            set
            {
                if (this._ImageFilePath == value)
                    return;
                this._ImageFilePath = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.ImageSource);
            }
        }
        #endregion


        #region ImageSource変更通知プロパティ
        public BitmapSource ImageSource
        {
            get
            {
                if (this.ImageFilePath == null) return null;
                if (!File.Exists(this.ImageFilePath)) return null;

                using (var image = new MagickImage(this.ImageFilePath))
                {
                    //RenderingIntentとBlackPointCompensationは適用したい変換(AddProfile)より前に指定しないとダメ
                    image.RenderingIntent = this.RenderingIntent;
                    image.BlackPointCompensation = this.BlackPointCompensation;

                    if(image.GetColorProfile() == null)
                        image.AddProfile(this.SubstituteSourceProfile.Profile);
                    image.AddProfile(this.DestinationProfile.Profile);
                    return image.ToBitmapSource();
                }
            }
        }
        #endregion



        #region BlackPointCompensation変更通知プロパティ
        private bool _BlackPointCompensation;

        public bool BlackPointCompensation
        {
            get
            { return this._BlackPointCompensation; }
            set
            { 
                if (this._BlackPointCompensation == value)
                    return;
                this._BlackPointCompensation = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.ImageSource);
            }
        }
        #endregion


        #region RenderingIntent変更通知プロパティ
        private RenderingIntent _RenderingIntent;

        public RenderingIntent RenderingIntent
        {
            get
            { return this._RenderingIntent; }
            set
            { 
                if (this._RenderingIntent == value)
                    return;
                this._RenderingIntent = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.ImageSource);
            }
        }
        #endregion


        #region SubstituteSourceProfile変更通知プロパティ
        private ICCProfile _SubstituteSourceProfile;

        public ICCProfile SubstituteSourceProfile
        {
            get
            { return this._SubstituteSourceProfile; }
            set
            { 
                if (this._SubstituteSourceProfile == value)
                    return;
                this._SubstituteSourceProfile = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.ImageSource);
            }
        }
        #endregion


        #region DestinationProfile変更通知プロパティ
        private ICCProfile _DestinationProfile;

        public ICCProfile DestinationProfile
        {
            get
            { return this._DestinationProfile; }
            set
            { 
                if (this._DestinationProfile == value)
                    return;
                this._DestinationProfile = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.ImageSource);
            }
        }
        #endregion



        #region LocalProfiles変更通知プロパティ
        private IEnumerable<ICCProfile> _LocalProfiles;

        public IEnumerable<ICCProfile> LocalProfiles
        {
            get
            { return this._LocalProfiles; }
            set
            { 
                if (this._LocalProfiles == value)
                    return;
                this._LocalProfiles = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

    }

    public class ICCProfile
    {
        public string Name { get; set; }
        public ColorProfile Profile { get; set; }

        public ICCProfile(string name, ColorProfile profile)
        {
            this.Name = name;
            this.Profile = profile;
        }
    }
}
